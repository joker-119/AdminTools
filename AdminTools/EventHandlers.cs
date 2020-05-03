using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EXILED;
using EXILED.Extensions;
using GameCore;
using Grenades;
using MEC;
using Mirror;
using RemoteAdmin;
using UnityEngine;
using Log = EXILED.Log;
using Object = UnityEngine.Object;

namespace AdminTools
{
	public class EventHandlers
	{
		private readonly Plugin plugin;
		List<ReferenceHub> ik_hubs = new List<ReferenceHub>();
		List<ReferenceHub> bd_hubs = new List<ReferenceHub>();
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnCommand(ref RACommandEvent ev)
		{
			try
			{
				if (ev.Command.Contains("REQUEST_DATA PLAYER_LIST SILENT"))
					return;

				string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				string scpFolder = Path.Combine(appData, "SCP Secret Laboratory");
				string logs = Path.Combine(scpFolder, "AdminLogs");
				string fileName = Path.Combine(logs, $"command_log-{ServerConsole.Port}.txt");
				if (!Directory.Exists(logs))
					Directory.CreateDirectory(logs);
				if (!File.Exists(fileName))
					File.Create(fileName).Close();
				string data =
					$"{DateTime.Now}: {ev.Sender.Nickname} ({ev.Sender.SenderId}) executed: {ev.Command} {Environment.NewLine}";
				File.AppendAllText(fileName, data);

				string[] args = ev.Command.Split(' ');
				ReferenceHub sender = ev.Sender.SenderId == "SERVER CONSOLE" || ev.Sender.SenderId == "GAME CONSOLE" ? PlayerManager.localPlayer.GetPlayer() : Player.GetPlayer(ev.Sender.SenderId);

				switch (args[0].ToLower())
				{
					case "kick":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.kick"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							IEnumerable<string> reasons = args.Where(s => s != args[0] && s != args[1]);
							string reason = "";
							foreach (string st in reasons)
								reason += st;
							GameObject obj = Player.GetPlayer(args[1])?.gameObject;
							if (obj == null)
							{
								ev.Sender.RAMessage("Player not found", false);
								return;
							}

							ServerConsole.Disconnect(obj, $"You have been kicked from the server: {reason}");
							ev.Sender.RAMessage("Player was kicked.");
							return;
						}
					case "muteall":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.mute"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							foreach (ReferenceHub hub in Player.GetHubs())
								if (!hub.serverRoles.RemoteAdmin)
									hub.characterClassManager.SetMuted(true);
							ev.Sender.RAMessage("All non-staff players have been muted.");
							return;
						}
					case "unmuteall":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.mute"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							foreach (ReferenceHub hub in Player.GetHubs())
								if (!hub.serverRoles.RemoteAdmin)
									hub.characterClassManager.SetMuted(false);
							ev.Sender.RAMessage("All non-staff players have been muted.");
							return;
						}
					case "rocket":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.rocket"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							ReferenceHub hub = Player.GetPlayer(args[1]);
							if (hub == null && args[1] != "*" && args[1] != "all")
							{
								ev.Sender.RAMessage("Player not found.");
								return;
							}

							if (!float.TryParse(args[2], out float result))
							{
								ev.Sender.RAMessage($"Speed argument invalid: {args[2]}");
								return;
							}

							if (args[1] == "*" || args[1] == "all")
								foreach (ReferenceHub h in Player.GetHubs())
									Timing.RunCoroutine(DoRocket(h, result));
							else
								Timing.RunCoroutine(DoRocket(hub, result));
							ev.Sender.RAMessage("We're going on a trip, in our favorite rocketship.");
							return;
						}
					case "bc":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.bc"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							IEnumerable<string> thing = args.Skip(2);
							string msg = "";
							foreach (string s in thing)
								msg += $"{s} ";
							uint time = uint.Parse(args[1]);
							foreach (GameObject p in PlayerManager.players)
								p.GetComponent<Broadcast>()
									.TargetAddElement(p.GetComponent<Scp049PlayerScript>().connectionToClient, msg, time,
										false);
							ev.Sender.RAMessage("Broadcast Sent.");
							break;
						}
					case "id":
						{
							ev.Allow = false;
							string id;
							ReferenceHub rh = Player.GetPlayer(args[1]);

							id = rh == null ? "Player not found" : rh.characterClassManager.UserId;
							ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} - {id}");
							break;
						}
					case "pbc":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.bc"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 4)
							{
								ev.Sender.RAMessage(
									"You must provide a players name/id, a number in seconds to display the broadcast, and a message",
									false);
								break;
							}

							if (!uint.TryParse(args[2], out uint result))
							{
								ev.Sender.RAMessage("You must provide a valid integer for a duration.", false);
								break;
							}

							IEnumerable<string> thing = args.Skip(3);
							string msg = "";
							foreach (string s in thing)
								msg += $"{s} ";
							Player.GetPlayer(args[1])?.Broadcast(result, msg, false);
							ev.Sender.RAMessage("Message sent.");
							break;
						}
					case "tut":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.tut"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 2)
							{
								ev.Sender.RAMessage("You must supply a player name or ID", false);
								return;
							}

							ReferenceHub rh = Player.GetPlayer(string.Join(" ", args.Skip(1)));
							if (rh == null)
							{
								ev.Sender.RAMessage("Player not found.", false);
								return;
							}

							if (rh.characterClassManager.CurClass != RoleType.Tutorial)
							{
								Timing.RunCoroutine(DoTut(rh));
								ev.Sender.RAMessage("Player set as tutorial.");
							}
							else
							{
								ev.Sender.RAMessage("Player unset as Tutorial (killed).");
								rh.characterClassManager.SetPlayersClass(RoleType.Spectator, rh.gameObject);
							}

							break;
						}
					case "hidetags":
						ev.Allow = false;
						if (!sender.CheckPermission("at.tags"))
						{
							ev.Sender.RAMessage("Permission denied.");
							return;
						}
						foreach (ReferenceHub hub in Player.GetHubs())
							if (hub.serverRoles.RemoteAdmin)
							{
								hub.serverRoles.HiddenBadge = hub.serverRoles.MyText;
								hub.serverRoles.NetworkGlobalBadge = null;
								hub.serverRoles.SetText(null);
								hub.serverRoles.SetColor(null);
								hub.serverRoles.GlobalSet = false;
								hub.serverRoles.RefreshHiddenTag();
							}

						ev.Sender.RAMessage("All staff tags hidden.");

						break;
					case "showtags":
						ev.Allow = false;
						if (!sender.CheckPermission("at.tags"))
						{
							ev.Sender.RAMessage("Permission denied.");
							return;
						}
						foreach (ReferenceHub hub in Player.GetHubs())
							if (hub.serverRoles.RemoteAdmin && !hub.serverRoles.RaEverywhere)
							{
								hub.serverRoles.HiddenBadge = null;
								hub.serverRoles.RpcResetFixed();
								hub.serverRoles.RefreshPermissions(true);
							}

						ev.Sender.RAMessage("All staff tags shown.");

						break;
					case "jail":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.jail"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 2)
							{
								ev.Sender.RaReply("Joker's Plugin#You must supply a player name or ID", false, true,
									string.Empty);
								return;
							}

							var array = args.Where(a => a != args[0]);
							string filter = null;
							foreach (string s in array)
								filter += s;
							ReferenceHub target = Player.GetPlayer(filter);
							if (target == null)
								ev.Sender.RaReply("Joker's Plugin#User not found.", false, true, string.Empty);
							if (plugin.JailedPlayers.Any(j => j.Userid == target.characterClassManager.UserId))
							{
								Timing.RunCoroutine(DoUnJail(target));
								ev.Sender.RaReply("Joker's Plugin#User unjailed.", true, true, string.Empty);
							}
							else
							{
								Timing.RunCoroutine(DoJail(target));
								ev.Sender.RaReply("Joker's Plugin#User jailed.", true, true, string.Empty);
							}

							break;
						}
					case "abc":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.bc"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 3)
							{
								ev.Sender.RAMessage("You must include a duration and a message.", false);
								return;
							}

							if (!uint.TryParse(args[1], out uint result))
							{
								ev.Sender.RAMessage("You must provide a valid integer for a duration.", false);
								break;
							}

							IEnumerable<string> thing2 = args.Skip(2);
							string msg = "";
							foreach (string s in thing2)
								msg += $"{s} ";
							foreach (GameObject o in PlayerManager.players)
							{
								ReferenceHub rh = o.GetComponent<ReferenceHub>();
								if (rh.serverRoles.RemoteAdmin)
									rh.Broadcast(result, $"{ev.Sender.Nickname}: {msg}", false);
							}

							ev.Sender.RAMessage("Message sent to all online staff members.");

							break;
						}
					case "drop":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.items"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							int result;
							if (args.Length != 4)
							{
								ev.Sender.RAMessage($"Invalid arguments.{args.Length}");
								break;
							}

							ReferenceHub hub = Player.GetPlayer(args[1]);
							if (hub == null)
							{
								ev.Sender.RAMessage("Player not found.");
								break;
							}

							ItemType item = (ItemType)Enum.Parse(typeof(ItemType), args[2]);

							if (!int.TryParse(args[3], out result))
							{
								ev.Sender.RAMessage("Not a number doufus.");
								break;
							}

							if (result > 200)
							{
								ev.Sender.RAMessage("Try a lower number that won't crash my servers, ty.");
								return;
							}

							for (int i = 0; i < result; i++)
								SpawnItem(item, hub.gameObject.transform.position, Vector3.zero);
							ev.Sender.RAMessage("Done. hehexd");
							return;
						}
					case "pos":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.tp"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.Sender.RAMessage("You must supply a player name/ID and a subcommand.", false);
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							foreach (ReferenceHub rh in hubs)
							{
								switch (args[2].ToLower())
								{
									case "set":
										{
											if (args.Length < 6)
											{
												ev.Sender.RAMessage("You must supply x, y and z coordinated.", false);
												return;
											}

											if (!float.TryParse(args[3], out float x))
											{
												ev.Sender.RAMessage("Invalid x coordinates.");
												return;
											}

											if (!float.TryParse(args[4], out float y))
											{
												ev.Sender.RAMessage("Invalid y coordinates.");
												return;
											}

											if (!float.TryParse(args[5], out float z))
											{
												ev.Sender.RAMessage("Invalid z coordinates.");
												return;
											}

											rh.plyMovementSync.OverridePosition(new Vector3(x, y, z), 0f, false);
											ev.Sender.RAMessage(
												$"Player {rh.nicknameSync.MyNick} - {rh.characterClassManager.UserId} moved to x{x} y{y} z{z}");
											break;
										}
									case "get":
										{
											Vector3 pos = rh.gameObject.transform.position;
											string ret =
												$"{rh.nicknameSync.MyNick} - {rh.characterClassManager.UserId} Position: x {pos.x} y {pos.y} z {pos.z}";
											ev.Sender.RAMessage(ret);
											break;
										}
									case "add":
										{
											if (args[3] != "x" && args[3] != "y" && args[3] != "z")
											{
												ev.Sender.RAMessage("Invalid coordinate plane selected.");
												return;
											}

											if (!float.TryParse(args[4], out float newPos))
											{
												ev.Sender.RAMessage("Invalid coordinate.");
												return;
											}

											Vector3 pos = rh.plyMovementSync.RealModelPosition;
											switch (args[3].ToLower())
											{
												case "x":
													rh.plyMovementSync.OverridePosition(
														new Vector3(pos.x + newPos, pos.y, pos.z), 0f);
													break;
												case "y":
													rh.plyMovementSync.OverridePosition(
														new Vector3(pos.x, pos.y + newPos, pos.z), 0f);
													break;
												case "z":
													rh.plyMovementSync.OverridePosition(
														new Vector3(pos.x, pos.y, pos.z + newPos), 0f);
													break;
											}

											ev.Sender.RAMessage(
												$"Player {rh.nicknameSync.MyNick} - {rh.characterClassManager.UserId} position changed.");
											break;
										}
								}
							}

							break;
						}
					case "tpx":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.tp"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.Sender.RAMessage(
									"You must supply a player name/ID to teleport and a player name/ID to teleport them to.");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}
							ReferenceHub target = Player.GetPlayer(args[2]);

							if (target == null)
							{
								ev.Sender.RAMessage($"Player {args[2]} not found.");
								return;
							}

							foreach (ReferenceHub rh in hubs)
							{
								rh.plyMovementSync.OverridePosition(target.plyMovementSync.RealModelPosition, 0f, false);
								ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} teleported to {target.nicknameSync.MyNick}");
							}

							break;
						}
					case "ghost":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.ghost"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 2)
							{
								ev.Sender.RAMessage("You must supply a playername to ghost.", false);
								return;
							}

							ReferenceHub rh = Player.GetPlayer(args[1]);
							if (rh == null)
							{
								ev.Sender.RAMessage("Player not found.", false);
								return;
							}

							if (EventPlugin.GhostedIds.Contains(rh.queryProcessor.PlayerId))
							{
								EventPlugin.GhostedIds.Remove(rh.queryProcessor.PlayerId);
								ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} removed from ghostmode.");
								return;
							}

							EventPlugin.GhostedIds.Add(rh.queryProcessor.PlayerId);
							ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} ghosted.");
							return;
						}
					case "scale":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.size"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 3)
							{
								ev.Sender.RAMessage("You must provide a target and scale size.");
								return;
							}

							if (!float.TryParse(args[2], out float scale))
							{
								ev.Sender.RAMessage("Invalid scale size selected.");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							foreach (ReferenceHub rh in hubs)
							{
								SetPlayerScale(rh.gameObject, scale);
								ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} size set to {scale}");
							}

							return;
						}
					case "size":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.size"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 5)
							{
								ev.Sender.RAMessage("You must provide a target, x size, y size and z size.", false);
								return;
							}

							if (!float.TryParse(args[2], out float x))
							{
								ev.Sender.RAMessage($"Invalid x size: {args[2]}", false);
								return;
							}

							if (!float.TryParse(args[3], out float y))
							{
								ev.Sender.RAMessage($"Invalid y size: {args[3]}", false);
								return;
							}

							if (!float.TryParse(args[4], out float z))
							{
								ev.Sender.RAMessage($"Invalid z size: {args[4]}", false);
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							foreach (ReferenceHub rh in hubs)
							{
								SetPlayerScale(rh.gameObject, x, y, z);
								ev.Sender.RAMessage($"{rh.nicknameSync.MyNick}'s size has been changed.");
							}

							return;
						}
					case "spawnworkbench":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.benches"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 5)
							{
								ev.Sender.RAMessage("Invalid number of arguments.", false);
								return;
							}

							if (!float.TryParse(args[2], out float x))
							{
								ev.Sender.RAMessage($"Invalid x size: {args[2]}", false);
								return;
							}

							if (!float.TryParse(args[3], out float y))
							{
								ev.Sender.RAMessage($"Invalid y size: {args[3]}", false);
								return;
							}

							if (!float.TryParse(args[4], out float z))
							{
								ev.Sender.RAMessage($"Invalid z size: {args[4]}", false);
								return;
							}

							ReferenceHub player = Player.GetPlayer(args[1]);
							if (player == null)
							{
								ev.Sender.RAMessage($"Player not found: {args[1]}", false);
								return;
							}

							GameObject gameObject;
							SpawnWorkbench(player.gameObject.transform.position + (gameObject = player.gameObject).GetComponent<Scp049PlayerScript>().plyCam.transform.forward * 2, gameObject.transform.rotation.eulerAngles, new Vector3(x, y, z));
							ev.Sender.RAMessage($"Ahh, yes. Enslaved game code.");
							return;
						}
					case "drops":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.items"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 4)
							{
								ev.Sender.RAMessage("haha no, try again with correct arguments 4head");
								return;
							}

							if (!float.TryParse(args[3], out float size))
							{
								ev.Sender.RAMessage("Invalid size");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}
							ItemType item = (ItemType)Enum.Parse(typeof(ItemType), args[2]);

							foreach (ReferenceHub player in hubs)
							{
								Pickup yesnt = player.inventory.SetPickup(item, -4.656647E+11f, player.transform.position,
									Quaternion.identity, 0, 0, 0);

								GameObject gameObject = yesnt.gameObject;
								gameObject.transform.localScale = Vector3.one * size;

								NetworkServer.UnSpawn(gameObject);
								NetworkServer.Spawn(yesnt.gameObject);
							}

							ev.Sender.RAMessage(
									$"Yay, items! With sizes!!");
							return;
						}
					case "dummy":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.dummy"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 6)
							{
								ev.Sender.RAMessage("You must supply a player, dummy role, x size, y size and z size");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							RoleType role = RoleType.None;
							try
							{
								role = (RoleType)Enum.Parse(typeof(RoleType), args[2]);
							}
							catch (Exception)
							{
								ev.Sender.RAMessage($"Invalid role selected: {args[2]}", false);
								return;
							}

							if (role == RoleType.None)
							{
								ev.Sender.RAMessage("Cannot spawn a dummy without a role.", false);
								return;
							}

							if (!float.TryParse(args[3], out float x))
							{
								ev.Sender.RAMessage("Invalid x value.");
								return;
							}
							if (!float.TryParse(args[4], out float y))
							{
								ev.Sender.RAMessage("Invalid y value.");
								return;
							}
							if (!float.TryParse(args[5], out float z))
							{
								ev.Sender.RAMessage("Invalid z value.");
								return;
							}

							foreach (ReferenceHub player in hubs)
							{
								SpawnDummyModel(player.GetPosition(), player.gameObject.transform.localRotation, role, x, y,
									z);
							}

							ev.Sender.RAMessage("Dummy(s) spawned.");
							break;
						}
					case "ragdoll":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.dolls"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 4)
							{
								ev.Sender.RAMessage("Try again");
								return;
							}

							if (!int.TryParse(args[3], out int count))
							{
								ev.Sender.RAMessage("Invalid number selected.");
								return;
							}

							if (!int.TryParse(args[2], out int role))
							{
								ev.Sender.RAMessage("Invalid roleID");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							ev.Sender.RAMessage("hehexd");
							foreach (ReferenceHub player in hubs)
							{
								Timing.RunCoroutine(SpawnBodies(player, role, count));
							}

							return;
						}
					case "config":
						{
							if (args[1].ToLower() == "reload")
							{
								ev.Allow = false;
								ServerStatic.PermissionsHandler.RefreshPermissions();
								ConfigFile.ReloadGameConfigs();
								ev.Sender.RAMessage($"Config files reloaded.");
							}

							return;
						}
					case "hp":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.hp"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 3)
							{
								ev.Sender.RAMessage("You must supply a player name/ID and an amount.", false);
								return;
							}

							if (!int.TryParse(args[2], out int result))
							{
								ev.Sender.RAMessage($"Invalid health amount: {args[2]}");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							foreach (ReferenceHub player in hubs)
							{
								if (result > player.playerStats.maxHP)
								{
									player.playerStats.maxHP = result;
								}

								player.playerStats.health = result;
								ev.Sender.RAMessage(
									$"{player.nicknameSync.MyNick} ({player.characterClassManager.UserId}'s health has been set to {result}");
							}

							return;
						}
					case "cleanup":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.cleanup"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}
							if (args.Length < 2)
							{
								ev.Sender.RAMessage("You must supply a type of cleanup: items or ragdolls.", false);
								return;
							}

							if (args[1].ToLower() == "items")
								foreach (Pickup item in Object.FindObjectsOfType<Pickup>())
									item.Delete();
							else if (args[1].ToLower() == "ragdolls")
								foreach (Ragdoll doll in Object.FindObjectsOfType<Ragdoll>())
									NetworkServer.Destroy(doll.gameObject);
							ev.Sender.RAMessage("Cleanup complete.");
							return;
						}
					case "grenade":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.grenade"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.Sender.RAMessage($"Too few arguments. Value: {args.Length}, Expected 3");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							switch (args[2].ToLower())
							{
								case "frag":
									foreach (ReferenceHub hub in hubs)
									{
										GrenadeManager gm = hub.GetComponent<GrenadeManager>();
										GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
										if (grenade == null)
										{
											ev.Sender.RAMessage($"Something broke that really really <b>really</b> shouldn't have.. Notify Joker with the following error code: GS-NRE", false);
											return;
										}
										Grenade component = Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
										component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
										NetworkServer.Spawn(component.gameObject);
									}
									ev.Sender.RAMessage("Tick, tick.. BOOM!");
									break;
								case "flash":
									foreach (ReferenceHub hub in hubs)
									{
										GrenadeManager gm = hub.GetComponent<GrenadeManager>();
										GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFlash);
										if (grenade == null)
										{
											ev.Sender.RAMessage($"Something broke that really really <b>really</b> shouldn't have.. Notify Joker with the following error code: GS-NRE", false);
											return;
										}
										Grenade component = Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
										component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
										NetworkServer.Spawn(component.gameObject);
									}
									ev.Sender.RAMessage("Don't look at the light!");
									break;
								case "ball":
									foreach (ReferenceHub hub in hubs)
									{
										Vector3 spawnrand = new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f));
										GrenadeManager gm = hub.GetComponent<GrenadeManager>();
										GrenadeSettings ball = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
										if (ball == null)
										{
											ev.Sender.RAMessage($"TheMoogle broke something in his code that shouldn't have been.. Notify Joker with the error code: Mog's Balls don't work", false);
											return;
										}
										Grenade component = Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
										component.InitData(gm, spawnrand, Vector3.zero);
										NetworkServer.Spawn(component.gameObject);
									}
									ev.Sender.RAMessage("The Balls started bouncing!");
									break;
								default:
									ev.Sender.RAMessage("Enter either \"frag\", \"flash\" or \"ball\".");
									break;
							}
							break;
						}
					case "ball":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.ball"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 2)
							{
								ev.Sender.RAMessage($"Too few arguments. Value: {args.Length}, Expected 2");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
								PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement("pitch_1.5 xmas_bouncyballs", true, false);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							foreach (ReferenceHub hub in hubs)
							{
								Vector3 spawnrand = new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f));
								GrenadeManager gm = hub.GetComponent<GrenadeManager>();
								GrenadeSettings ball = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
								if (ball == null)
								{
									ev.Sender.RAMessage($"TheMoogle broke something in his code that shouldn't have been.. Notify Joker with the error code: Mog's Balls don't work", false);
									return;
								}
								Grenade component = Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
								component.InitData(gm, spawnrand, Vector3.zero);
								NetworkServer.Spawn(component.gameObject);
							}

							ev.Sender.RAMessage("The Balls started bouncing!");
							break;
						}
					case "kill":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.kill"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							List<ReferenceHub> hubs = new List<ReferenceHub>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
									if (hub.characterClassManager.CurClass != RoleType.Spectator)
										hubs.Add(hub);
							}
							else
							{
								ReferenceHub rh = Player.GetPlayer(args[1]);
								if (rh == null)
								{
									ev.Sender.RAMessage("Player not found.", false);
									return;
								}
								hubs.Add(rh);
							}

							foreach (ReferenceHub hub in hubs)
							{
								int id = Player.GetPlayer(ev.Sender.SenderId) ? Player.GetPlayer(ev.Sender.SenderId).GetPlayerId() : 0;

								hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(119000000, ev.Sender.Nickname, DamageTypes.Wall, id), hub.gameObject);

								ev.Sender.RAMessage($"{hub.nicknameSync.MyNick} has been slayed.");
							}

							break;
						}
					case "inv":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.inv"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.Sender.RAMessage("Please provide a removal command and id");
								return;
							}

							switch (args[1].ToLower())
							{
								case "clear":
									if (args[2].ToLower() == "*" || args[2].ToLower() == "all")
									{
										foreach (ReferenceHub hub in Player.GetHubs())
											if (hub.characterClassManager.CurClass != RoleType.Spectator)
												hub.ClearInventory();

										ev.Sender.RAMessage("Cleared all items in everyone's inventory");
									}
									else
									{
										ReferenceHub player = Player.GetPlayer(args[2]);
										if (player == null)
										{
											ev.Sender.RAMessage($"Player {args[2]} not found");
											return;
										}

										player.ClearInventory();
										ev.Sender.RAMessage($"Cleared all items in {player.nicknameSync.MyNick}'s inventory");
									}
									break;
								case "drop":
									if (args[2].ToLower() == "*" || args[2].ToLower() == "all")
									{
										foreach (ReferenceHub hub in Player.GetHubs())
											if (hub.characterClassManager.CurClass != RoleType.Spectator)
												hub.inventory.ServerDropAll();

										ev.Sender.RAMessage("Dropped all items in everyone's inventory");
									}
									else
									{
										ReferenceHub player = Player.GetPlayer(args[2]);
										if (player == null)
										{
											ev.Sender.RAMessage($"Player {args[2]} not found");
											return;
										}

										player.inventory.ServerDropAll();
										ev.Sender.RAMessage($"Dropped all items in {player.nicknameSync.MyNick}'s inventory");
									}
									break;
								case "see":
									ReferenceHub ply = Player.GetPlayer(args[2]);
									if (ply == null)
									{
										ev.Sender.RAMessage($"Player {args[2]} not found");
										return;
									}

									if (ply.inventory.items.Count != 0)
									{
										string itemLister = $"Player {ply.nicknameSync.MyNick} has the following items in their inventory (in order): ";
										foreach (Inventory.SyncItemInfo item in ply.inventory.items)
										{
											itemLister += item.id + ", ";
										}
										itemLister = itemLister.Substring(0, itemLister.Count() - 2);
										ev.Sender.RAMessage(itemLister);
										return;
									}
									ev.Sender.RAMessage($"Player {ply.nicknameSync.MyNick} does not have any items in their inventory");
									break;
								default:
									ev.Sender.RAMessage("Please enter either \"clear\", \"drop\", or \"see\"");
									break;
							}
							break;
						}
					case "ik":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.ik"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 2)
							{
								ev.Sender.RAMessage("Please provide a id");
								return;
							}

							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
								{
									if (!hub.TryGetComponent(out InstantKillComponent ikComponent))
									{
										hub.gameObject.AddComponent<InstantKillComponent>();
										ik_hubs.Add(hub);
									}
								}

								ev.Sender.RAMessage("Instant killing is on for all players now");
							}
							else if (args[1].ToLower() == "list")
							{
								if (ik_hubs.Count != 0)
								{
									string playerLister = "Players with instant kill on: ";
									foreach (ReferenceHub hub in ik_hubs)
									{
										playerLister += hub.nicknameSync.MyNick + ", ";
									}
									playerLister = playerLister.Substring(0, playerLister.Count() - 2);
									ev.Sender.RAMessage(playerLister);
									return;
								}
								ev.Sender.RAMessage("No players currently online have instant killing on");
							}
							else if (args[1].ToLower() == "clear")
							{
								foreach (ReferenceHub hub in Player.GetHubs())
								{
									if (hub.TryGetComponent(out InstantKillComponent ikComponent))
									{
										UnityEngine.Object.Destroy(ikComponent);
										ik_hubs.Remove(hub);
									}
								}
								ev.Sender.RAMessage("Instant killing is off for all players now");
							}
							else
							{
								ReferenceHub ply = Player.GetPlayer(args[1]);
								if (ply == null)
								{
									ev.Sender.RAMessage($"Player {args[1]} not found");
									return;
								}

								if (!ply.TryGetComponent(out InstantKillComponent ikComponent))
								{
									ply.gameObject.AddComponent<InstantKillComponent>();
									ev.Sender.RAMessage($"Instant killing is on for {ply.nicknameSync.MyNick}");
									ik_hubs.Add(ply);
								}
								else
								{
									UnityEngine.Object.Destroy(ikComponent);
									ev.Sender.RAMessage($"Instant killing is off for {ply.nicknameSync.MyNick}");
									ik_hubs.Remove(ply);
								}
							}
							break;
						}
					case "bd":
						{
							ev.Allow = false;
							if (!sender.CheckPermission("at.bd"))
							{
								ev.Sender.RAMessage("Permission denied.");
								return;
							}

							if (args.Length < 2)
							{
								ev.Sender.RAMessage("Please provide a break command and an id (if needed) (Note: For \"list\" and \"clear\" you do not need an id)");
								return;
							}

							switch (args.Length)
							{
								case 2:
									switch (args[1].ToLower())
									{
										case "list":
											if (bd_hubs.Count != 0)
											{
												string playerLister = "Players with break doors permissions on: ";
												foreach (ReferenceHub hub in bd_hubs)
												{
													playerLister += hub.nicknameSync.MyNick + ", ";
												}
												playerLister = playerLister.Substring(0, playerLister.Count() - 2);
												ev.Sender.RAMessage(playerLister);
												return;
											}
											ev.Sender.RAMessage("No players currently online have break door permissions on");
											break;
										case "clear":
											foreach (ReferenceHub hub in Player.GetHubs())
											{
												if (hub.TryGetComponent(out BreakDoorComponent bdComponent))
												{
													UnityEngine.Object.Destroy(bdComponent);
													bd_hubs.Remove(hub);
													hub.SetBypassMode(false);
												}
											}
											ev.Sender.RAMessage("Break door permissions is off for all players now");
											break;
										default:
											ev.Sender.RAMessage("Please enter either \"all\", \"clear\", \"doors\", or \"list\"");
											break;
									}
									break;
								case 3:
									switch (args[1].ToLower())
									{
										case "doors":
											if (args[2].ToLower() == "*" || args[2].ToLower() == "all")
											{
												foreach (ReferenceHub hub in Player.GetHubs())
												{
													if (!hub.TryGetComponent(out BreakDoorComponent bdComponent))
													{
														hub.gameObject.AddComponent<BreakDoorComponent>();
														bd_hubs.Add(hub);
													}
													else
													{
														hub.SetBypassMode(false);
														bdComponent.BreakAll = false;
													}
												}
												ev.Sender.RAMessage("Instant breaking of doors is on for all players now");
												return;
											}

											ReferenceHub player = Player.GetPlayer(args[2]);
											if (player == null)
											{
												ev.Sender.RAMessage($"Player {args[2]} not found");
												return;
											}

											if (!player.TryGetComponent(out BreakDoorComponent doorBreak))
											{
												ev.Sender.RAMessage($"Instant breaking of doors is on for {player.nicknameSync.MyNick}");
												BreakDoorComponent doorBreakerDoor = player.gameObject.AddComponent<BreakDoorComponent>();
												doorBreakerDoor.BreakAll = false;
												player.SetBypassMode(false);
												bd_hubs.Add(player);
											}
											else
											{
												if (doorBreak.BreakAll)
												{
													ev.Sender.RAMessage($"Instant breaking of doors is on for {player.nicknameSync.MyNick}");
													doorBreak.BreakAll = false;
													player.SetBypassMode(false);
													return;
												}

												ev.Sender.RAMessage($"Instant breaking of doors is off for {player.nicknameSync.MyNick}");
												UnityEngine.Object.Destroy(doorBreak);
												bd_hubs.Remove(player);
											}
											break;
										case "all":
											if (args[2].ToLower() == "*" || args[2].ToLower() == "all")
											{
												foreach (ReferenceHub hub in Player.GetHubs())
												{
													if (!hub.TryGetComponent(out BreakDoorComponent bdComponent))
													{
														BreakDoorComponent doorBreakerAll = hub.gameObject.AddComponent<BreakDoorComponent>();
														bd_hubs.Add(hub);
														hub.SetBypassMode(true);
														doorBreakerAll.BreakAll = true;
													}
													else
													{
														bdComponent.BreakAll = true;
														hub.SetBypassMode(true);
													}
												}
												ev.Sender.RAMessage("Instant breaking of everything is on for all players now");
												return;
											}

											player = Player.GetPlayer(args[2]);
											if (player == null)
											{
												ev.Sender.RAMessage($"Player {args[2]} not found");
												return;
											}

											if (!player.TryGetComponent(out BreakDoorComponent doorBreaker))
											{
												ev.Sender.RAMessage($"Instant breaking of everything is on for {player.nicknameSync.MyNick}");
												BreakDoorComponent doorBreakerAll = player.gameObject.AddComponent<BreakDoorComponent>();
												doorBreakerAll.BreakAll = true;
												player.SetBypassMode(true);
												bd_hubs.Add(player);
											}
											else
											{
												if (!doorBreaker.BreakAll)
												{
													ev.Sender.RAMessage($"Instant breaking of everything is on for {player.nicknameSync.MyNick}");
													doorBreaker.BreakAll = true;
													player.SetBypassMode(true);
													return;
												}
												ev.Sender.RAMessage($"Instant breaking of everything is off for {player.nicknameSync.MyNick}");
												UnityEngine.Object.Destroy(doorBreaker);
												player.SetBypassMode(false);
												bd_hubs.Remove(player);
											}
											break;
										default:
											ev.Sender.RAMessage("Please enter either \"all\", \"clear\", \"doors\", or \"list\"");
											break;
									}
									break;
							}
						}
						break;
				}
			}
			catch (Exception e)
			{
				Log.Error($"Handling command error: {e}");
			}
		}

		private void SpawnDummyModel(Vector3 position, Quaternion rotation, RoleType role, float x, float y, float z)
		{
			GameObject obj =
				Object.Instantiate(
					NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
			CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
			if (ccm == null)
				Log.Error("CCM is null, doufus. You need to do this the harder way.");
			ccm.CurClass = role;
			ccm.RefreshPlyModel();
			obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
			obj.GetComponent<QueryProcessor>().PlayerId = 9999;
			obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
			obj.transform.localScale = new Vector3(x, y, z);
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			NetworkServer.Spawn(obj);
		}

		private IEnumerator<float> SpawnBodies(ReferenceHub player, int role, int count)
		{
			for (int i = 0; i < count; i++)
			{
				player.gameObject.GetComponent<RagdollManager>().SpawnRagdoll(player.gameObject.transform.position + (Vector3.up * 5),
					Quaternion.identity, role,
					new PlayerStats.HitInfo(1000f, player.characterClassManager.UserId, DamageTypes.Falldown,
						player.queryProcessor.PlayerId), false, "SCP-343", "SCP-343", 0);
				yield return Timing.WaitForSeconds(0.15f);
			}
		}

		public void SpawnWorkbench(Vector3 position, Vector3 rotation, Vector3 size)
		{
			GameObject bench =
				Object.Instantiate(
					NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "Work Station"));
			Offset offset = new Offset();
			offset.position = position;
			offset.rotation = rotation;
			offset.scale = Vector3.one;
			bench.gameObject.transform.localScale = size;

			NetworkServer.Spawn(bench);
			bench.GetComponent<WorkStation>().Networkposition = offset;
			bench.AddComponent<WorkStationUpgrader>();
		}

		public void SpawnItem(ItemType type, Vector3 pos, Vector3 rot)
		{
			PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(type, -4.656647E+11f, pos, Quaternion.Euler(rot), 0, 0, 0);
		}

		private IEnumerator<float> DoTut(ReferenceHub rh)
		{
			if (rh.serverRoles.OverwatchEnabled)
				rh.serverRoles.OverwatchEnabled = false;

			rh.characterClassManager.SetPlayersClass(RoleType.Tutorial, rh.gameObject, true);
			yield return Timing.WaitForSeconds(1f);
			var d = UnityEngine.Object.FindObjectsOfType<Door>();
			foreach (Door door in d)
				if (door.DoorName == "SURFACE_GATE")
					rh.plyMovementSync.OverridePosition(door.transform.position + Vector3.up * 2, 0f);
			rh.serverRoles.CallTargetSetNoclipReady(rh.characterClassManager.connectionToClient, true);
			rh.serverRoles.NoclipReady = true;
		}


		public void SetPlayerScale(GameObject target, float x, float y, float z)
		{
			try
			{
				NetworkIdentity identity = target.GetComponent<NetworkIdentity>();


				target.transform.localScale = new Vector3(1 * x, 1 * y, 1 * z);

				ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
				destroyMessage.netId = identity.netId;


				foreach (GameObject player in PlayerManager.players)
				{
					NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;

					if (player != target)
						playerCon.Send(destroyMessage, 0);

					object[] parameters = new object[] { identity, playerCon };
					typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
				}
			}
			catch (Exception e)
			{
				Log.Info($"Set Scale error: {e}");
			}
		}

		public void SetPlayerScale(GameObject target, float scale)
		{
			try
			{
				NetworkIdentity identity = target.GetComponent<NetworkIdentity>();


				target.transform.localScale = Vector3.one * scale;

				ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
				destroyMessage.netId = identity.netId;


				foreach (GameObject player in PlayerManager.players)
				{
					if (player == target)
						continue;

					NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;

					playerCon.Send(destroyMessage, 0);

					object[] parameters = new object[] { identity, playerCon };
					typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
				}
			}
			catch (Exception e)
			{
				Log.Info($"Set Scale error: {e}");
			}
		}

		public IEnumerator<float> DoRocket(ReferenceHub hub, float speed)
		{
			const int maxAmnt = 50;
			int amnt = 0;
			while (hub.characterClassManager.CurClass != RoleType.Spectator)
			{
				hub.plyMovementSync.OverridePosition(hub.gameObject.transform.position + Vector3.up * speed, 0f, false);
				amnt++;
				if (amnt >= maxAmnt)
				{
					hub.characterClassManager.GodMode = false;
					hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(1000000, "WORLD", DamageTypes.Grenade, 0),
						hub.gameObject);
				}

				yield return Timing.WaitForOneFrame;
			}
		}

		public IEnumerator<float> DoJail(ReferenceHub rh, bool skipadd = false)
		{
			List<ItemType> items = new List<ItemType>();
			foreach (var item in rh.inventory.items)
				items.Add(item.id);
			if (!skipadd)
				plugin.JailedPlayers.Add(new Jailed
				{
					Health = rh.playerStats.health,
					Position = rh.gameObject.transform.position,
					Items = items,
					Name = rh.characterClassManager.name,
					Role = rh.characterClassManager.CurClass,
					Userid = rh.characterClassManager.UserId,
				});
			if (rh.serverRoles.OverwatchEnabled)
				rh.serverRoles.OverwatchEnabled = false;
			yield return Timing.WaitForSeconds(1f);
			rh.characterClassManager.SetClassID(RoleType.Tutorial);
			rh.gameObject.transform.position = new Vector3(53f, 1020f, -44f);
			rh.inventory.items.Clear();
		}

		private IEnumerator<float> DoUnJail(ReferenceHub rh)
		{
			var jail = plugin.JailedPlayers.Find(j => j.Userid == rh.characterClassManager.UserId);
			rh.characterClassManager.SetClassID(jail.Role);
			foreach (ItemType item in jail.Items)
				rh.inventory.AddNewItem(item);
			yield return Timing.WaitForSeconds(1.5f);
			rh.playerStats.health = jail.Health;
			rh.plyMovementSync.OverridePosition(jail.Position, 0f);
			rh.gameObject.transform.position = jail.Position;
			plugin.JailedPlayers.Remove(jail);
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			try
			{
				if (plugin.JailedPlayers.Any(j => j.Userid == ev.Player.characterClassManager.UserId))
					Timing.RunCoroutine(DoJail(ev.Player, true));

				if (File.ReadAllText(plugin.OverwatchFilePath).Contains(ev.Player.characterClassManager.UserId))
				{
					Log.Debug($"Putting {ev.Player.characterClassManager.UserId} into overwatch.");
					ev.Player.serverRoles.OverwatchEnabled = true;
				}

				if (File.ReadAllText(plugin.HiddenTagsFilePath).Contains(ev.Player.characterClassManager.UserId))
				{
					Log.Debug($"Hiding {ev.Player.characterClassManager.UserId}'s tag.");
					ev.Player.serverRoles._hideLocalBadge = true;
					ev.Player.serverRoles.RefreshHiddenTag();
				}
			}
			catch (Exception e)
			{
				Log.Error($"Player Join: {e}");
			}
		}

		public void OnRoundEnd()
		{
			try
			{
				List<string> overwatchRead = File.ReadAllLines(plugin.OverwatchFilePath).ToList();
				List<string> tagsRead = File.ReadAllLines(plugin.HiddenTagsFilePath).ToList();

				foreach (ReferenceHub hub in Player.GetHubs())
				{
					string userId = hub.characterClassManager.UserId;

					if (hub.serverRoles.OverwatchEnabled && !overwatchRead.Contains(userId))
						overwatchRead.Add(userId);
					else if (!hub.serverRoles.OverwatchEnabled && overwatchRead.Contains(userId))
						overwatchRead.Remove(userId);

					if (hub.serverRoles._hideLocalBadge && !tagsRead.Contains(userId))
						tagsRead.Add(userId);
					else if (!hub.serverRoles._hideLocalBadge && tagsRead.Contains(userId))
						tagsRead.Remove(userId);
				}

				foreach (string s in overwatchRead)
					Log.Debug($"{s} is in overwatch.");
				foreach (string s in tagsRead)
					Log.Debug($"{s} has their tag hidden.");
				File.WriteAllLines(plugin.OverwatchFilePath, overwatchRead);
				File.WriteAllLines(plugin.HiddenTagsFilePath, tagsRead);
			}
			catch (Exception e)
			{
				Log.Error($"Round End: {e}");
			}
		}

		public void OnTriggerTesla(ref TriggerTeslaEvent ev)
		{
			if (ev.Player.characterClassManager.GodMode)
				ev.Triggerable = false;
		}

		public void OnSetClass(SetClassEvent ev)
		{
			if (plugin.GodTuts)
				ev.Player.characterClassManager.GodMode = ev.Role == RoleType.Tutorial;
		}

		public void OnPlayerLeave(PlayerLeaveEvent ev)
		{
			if (ik_hubs.Contains(ev.Player))
				ik_hubs.Remove(ev.Player);
			if (bd_hubs.Contains(ev.Player))
				bd_hubs.Remove(ev.Player);
		}

		public void OnRoundStart()
		{
			ik_hubs.Clear();
			bd_hubs.Clear();
		}
	}
}

