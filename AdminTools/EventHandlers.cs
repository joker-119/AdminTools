using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using GameCore;
using Grenades;
using MEC;
using Mirror;
using RemoteAdmin;
using UnityEngine;
using Log = Exiled.API.Features.Log;
using Object = UnityEngine.Object;

namespace AdminTools
{
	public class EventHandlers
	{
		private readonly Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnCommand(SendingRemoteAdminCommandEventArgs ev)
		{
			try
			{
				if (ev.Name.Contains("REQUEST_DATA PLAYER_LIST"))
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
					$"{DateTime.Now}: {ev.Sender.Nickname} ({ev.Sender.Id}) executed: {ev.Name} {Environment.NewLine}";
				File.AppendAllText(fileName, data);

				string effort = ev.Name;
				foreach (string s in ev.Arguments)
					effort += $" {s}";
				string[] args = effort.Split(' ');

				Player sender = ev.Sender;

				switch (ev.Name)
				{
					case "kick":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.kick"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							IEnumerable<string> reasons = args.Where(s => s != args[0] && s != args[1]);
							string reason = "";
							foreach (string st in reasons)
								reason += st;
							GameObject obj = Player.Get(args[1])?.GameObject;
							if (obj == null)
							{
								ev.ReplyMessage = ("Player not found");
								ev.Success = false;
								return;
							}

							ServerConsole.Disconnect(obj, $"You have been kicked from the server: {reason}");
							ev.ReplyMessage = ("Player was kicked.");
							return;
						}
					case "muteall":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.mute"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							foreach (Player player in Player.List)
								if (!player.ReferenceHub.serverRoles.RemoteAdmin)
									player.IsMuted = true;
							ev.ReplyMessage = ("All non-staff players have been muted.");
							return;
						}
					case "unmuteall":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.mute"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							foreach (Player player in Player.List)
								if (!player.ReferenceHub.serverRoles.RemoteAdmin)
									player.IsMuted = false;
							ev.ReplyMessage = ("All non-staff players have been muted.");
							return;
						}
					case "rocket":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.rocket"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							Player player = Player.Get(args[1]);
							if (player == null && args[1] != "*" && args[1] != "all")
							{
								ev.ReplyMessage = ("Player not found.");
								return;
							}

							if (!float.TryParse(args[2], out float result))
							{
								ev.ReplyMessage = ($"Speed argument invalid: {args[2]}");
								return;
							}

							if (args[1] == "*" || args[1] == "all")
								foreach (Player h in Player.List)
									Timing.RunCoroutine(DoRocket(h, result));
							else
								Timing.RunCoroutine(DoRocket(player, result));
							ev.ReplyMessage = ("We're going on a trip, in our favorite rocketship.");
							return;
						}
					case "bc":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.bc"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							IEnumerable<string> thing = args.Skip(2);
							string msg = "";
							foreach (string s in thing)
								msg += $"{s} ";
							uint time = uint.Parse(args[1]);
							foreach (GameObject p in PlayerManager.players)
								p.GetComponent<Broadcast>()
									.TargetAddElement(p.GetComponent<Scp049_2PlayerScript>().connectionToClient, msg, (ushort)time,
										Broadcast.BroadcastFlags.Normal);
							ev.ReplyMessage = ("Broadcast Sent.");
							break;
						}
					case "id":
						{
							ev.IsAllowed = false;
							Player player = Player.Get(args[1]);

							string id = player == null ? "Player not found" : player.UserId;
							ev.ReplyMessage = ($"{player.Nickname} - {id}");
							break;
						}
					case "pbc":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.bc"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 4)
							{
								ev.ReplyMessage = ("You must provide a players name/id, a number in seconds to display the broadcast, and a message");
									ev.Success = false;
								break;
							}

							if (!uint.TryParse(args[2], out uint result))
							{
								ev.ReplyMessage = ("You must provide a valid integer for a duration.");
								ev.Success = false;
								break;
							}

							IEnumerable<string> thing = args.Skip(3);
							string msg = "";
							foreach (string s in thing)
								msg += $"{s} ";
							Player.Get(args[1])?.Broadcast((ushort)result, msg, Broadcast.BroadcastFlags.Normal);
							ev.ReplyMessage = ("Message sent.");
							break;
						}
					case "tut":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.tut"))
							{
								ev.ReplyMessage = ("Permission denied.");
								ev.Success = false;
								return;
							}

							if (args.Length < 2)
							{
								ev.ReplyMessage = ("You must supply a player name or ID");
								ev.Success = false;
								return;
							}

							Player player = Player.Get(string.Join(" ", args.Skip(1)));
							if (player == null)
							{
								ev.ReplyMessage = ("Player not found.");
								ev.Success = false;
								return;
							}

							if (player.Role != RoleType.Tutorial)
							{
								Timing.RunCoroutine(DoTut(player));
								ev.ReplyMessage = ("Player set as tutorial.");
							}
							else
							{
								ev.ReplyMessage = ("Player unset as Tutorial (killed).");
								player.Role = RoleType.Spectator;
							}

							break;
						}
					case "hidetags":
						ev.IsAllowed = false;
						if (!sender.CheckPermission("at.tags"))
						{
							ev.ReplyMessage = ("Permission denied.");
							return;
						}
						foreach (Player player in Player.List)
							if (player.ReferenceHub.serverRoles.RemoteAdmin)
							{
								player.BadgeHidden = true;
							}

						ev.ReplyMessage = ("All staff tags hidden.");

						break;
					case "showtags":
						ev.IsAllowed = false;
						if (!sender.CheckPermission("at.tags"))
						{
							ev.ReplyMessage = ("Permission denied.");
							return;
						}
						foreach (Player player in Player.List)
							if (player.ReferenceHub.serverRoles.RemoteAdmin && !player.ReferenceHub.serverRoles.RaEverywhere)
							{
								player.BadgeHidden = false;
							}

						ev.ReplyMessage = ("All staff tags shown.");

						break;
					case "jail":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.jail"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 2)
							{
								ev.ReplyMessage = ("You must supply a player name or ID");
								ev.Success = false;
								return;
							}

							IEnumerable<string> array = args.Where(a => a != args[0]);
							string filter = null;
							foreach (string s in array)
								filter += s;
							Player target = Player.Get(filter);
							if (target == null)
							{
								ev.ReplyMessage = ("User not found.");
								ev.Success = false;
								return;
							}

							if (plugin.JailedPlayers.Any(j => j.Userid == target.UserId))
							{
								Timing.RunCoroutine(DoUnJail(target));
								ev.ReplyMessage = ("User unjailed.");
							}
							else
							{
								Timing.RunCoroutine(DoJail(target));
								ev.ReplyMessage = ("User jailed.");
							}

							break;
						}
					case "abc":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.bc"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 3)
							{
								ev.ReplyMessage = ("You must include a duration and a message.");
								ev.Success = false;
								return;
							}

							if (!uint.TryParse(args[1], out uint result))
							{
								ev.ReplyMessage = ("You must provide a valid integer for a duration.");
								ev.Success = false;
								break;
							}

							IEnumerable<string> thing2 = args.Skip(2);
							string msg = "";
							foreach (string s in thing2)
								msg += $"{s} ";
							foreach (Player player in Player.List)
							{
								if (player.ReferenceHub.serverRoles.RemoteAdmin)
									player.Broadcast((ushort)result, $"{ev.Sender.Nickname}: {msg}", Broadcast.BroadcastFlags.AdminChat);
							}

							ev.ReplyMessage = ("Message sent to all online staff members.");

							break;
						}
					case "drop":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.items"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							int result;
							if (args.Length != 4)
							{
								ev.ReplyMessage = ($"Invalid arguments.{args.Length}");
								break;
							}

							Player player = Player.Get(args[1]);
							if (player == null)
							{
								ev.ReplyMessage = ("Player not found.");
								break;
							}

							ItemType item = (ItemType)Enum.Parse(typeof(ItemType), args[2]);

							if (!int.TryParse(args[3], out result))
							{
								ev.ReplyMessage = ("Not a number doufus.");
								break;
							}

							if (result > 200)
							{
								ev.ReplyMessage = ("Try a lower number that won't crash my servers, ty.");
								return;
							}

							for (int i = 0; i < result; i++)
								SpawnItem(item, player.Position);
							ev.ReplyMessage = ("Done. hehexd");
							return;
						}
					case "pos":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.tp"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.ReplyMessage = ("You must supply a player name/ID and a subcommand.");
								ev.Success = false;
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							foreach (Player player in players)
								switch (args[2].ToLower())
								{
									case "set":
									{
										if (args.Length < 6)
										{
											ev.ReplyMessage = ("You must supply x, y and z coordinated.");
											ev.Success = false;
											return;
										}

										if (!float.TryParse(args[3], out float x))
										{
											ev.ReplyMessage = ("Invalid x coordinates.");
											return;
										}

										if (!float.TryParse(args[4], out float y))
										{
											ev.ReplyMessage = ("Invalid y coordinates.");
											return;
										}

										if (!float.TryParse(args[5], out float z))
										{
											ev.ReplyMessage = ("Invalid z coordinates.");
											return;
										}

										player.Position = new Vector3(x, y, z);
										ev.ReplyMessage = (
											$"Player {player.Nickname} - {player.UserId} moved to x{x} y{y} z{z}");
										break;
									}
									case "get":
									{
										Vector3 pos = player.Position;
										string ret =
											$"{player.Nickname} - {player.UserId} Position: x {pos.x} y {pos.y} z {pos.z}";
										ev.ReplyMessage = (ret);
										break;
									}
									case "add":
									{
										if (args[3] != "x" && args[3] != "y" && args[3] != "z")
										{
											ev.ReplyMessage = ("Invalid coordinate plane selected.");
											return;
										}

										if (!float.TryParse(args[4], out float newPos))
										{
											ev.ReplyMessage = ("Invalid coordinate.");
											return;
										}

										Vector3 pos = player.Position;
										switch (args[3].ToLower())
										{
											case "x":
												player.Position = new Vector3(pos.x + newPos, pos.y, pos.z);
												break;
											case "y":
												player.Position = new Vector3(pos.x, pos.y + newPos, pos.z);
												break;
											case "z":
												player.Position = new Vector3(pos.x, pos.y, pos.z + newPos);
												break;
										}

										ev.ReplyMessage = (
											$"Player {player.Nickname} - {player.UserId} position changed.");
										break;
									}
								}

							break;
						}
					case "tpx":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.tp"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.ReplyMessage = (
									"You must supply a player name/ID to teleport and a player name/ID to teleport them to.");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}
							Player target = Player.Get(args[2]);

							if (target == null)
							{
								ev.ReplyMessage = ($"Player {args[2]} not found.");
								return;
							}

							foreach (Player player in players)
							{
								player.Position = target.Position;
								ev.ReplyMessage = ($"{player.Nickname} teleported to {target.Nickname}");
							}

							break;
						}
					case "ghost":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.ghost"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 2)
							{
								ev.ReplyMessage = ("You must supply a playername to ghost.");
								ev.Success = false;
								return;
							}

							Player player = Player.Get(args[1]);
							if (player == null)
							{
								ev.ReplyMessage = ("Player not found.");
								ev.Success = false;
								return;
							}

							if (player.IsInvisible)
							{
								player.IsInvisible = false;
								ev.ReplyMessage = ($"{player.Nickname} removed from ghostmode.");
								return;
							}

							player.IsInvisible = true;
							ev.ReplyMessage = ($"{player.Nickname} ghosted.");
							return;
						}
					case "scale":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.size"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 3)
							{
								ev.ReplyMessage = ("You must provide a target and scale size.");
								return;
							}

							if (!float.TryParse(args[2], out float scale))
							{
								ev.ReplyMessage = ("Invalid scale size selected.");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							foreach (Player player in players)
							{
								SetPlayerScale(player.GameObject, scale);
								ev.ReplyMessage = ($"{player.Nickname} size set to {scale}");
							}

							return;
						}
					case "size":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.size"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 5)
							{
								ev.ReplyMessage = ("You must provide a target, x size, y size and z size.");
								ev.Success = false;
								return;
							}

							if (!float.TryParse(args[2], out float x))
							{
								ev.ReplyMessage = ($"Invalid x size: {args[2]}");
								ev.Success = false;
								return;
							}

							if (!float.TryParse(args[3], out float y))
							{
								ev.ReplyMessage = ($"Invalid y size: {args[3]}");
								ev.Success = false;
								return;
							}

							if (!float.TryParse(args[4], out float z))
							{
								ev.ReplyMessage = ($"Invalid z size: {args[4]}");
								ev.Success = false;
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							foreach (Player player in players)
							{
								SetPlayerScale(player.GameObject, x, y, z);
								ev.ReplyMessage = ($"{player.Nickname}'s size has been changed.");
							}

							return;
						}
					case "spawnworkbench":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.benches"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 5)
							{
								ev.ReplyMessage = ("Invalid number of arguments.");
								ev.Success = false;
								return;
							}

							if (!float.TryParse(args[2], out float x))
							{
								ev.ReplyMessage = ($"Invalid x size: {args[2]}");
								ev.Success = false;
								return;
							}

							if (!float.TryParse(args[3], out float y))
							{
								ev.ReplyMessage = ($"Invalid y size: {args[3]}");
								ev.Success = false;
								return;
							}

							if (!float.TryParse(args[4], out float z))
							{
								ev.ReplyMessage = ($"Invalid z size: {args[4]}");
								ev.Success = false;
								return;
							}

							Player player = Player.Get(args[1]);
							if (player == null)
							{
								ev.ReplyMessage = ($"Player not found: {args[1]}");
								ev.Success = false;
								return;
							}
							
							SpawnWorkbench(player.Position + player.ReferenceHub.PlayerCameraReference.forward * 2, player.GameObject.transform.rotation.eulerAngles, new Vector3(x, y, z));
							ev.ReplyMessage = ($"Ahh, yes. Enslaved game code.");
							return;
						}
					case "drops":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.items"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 4)
							{
								ev.ReplyMessage = ("haha no, try again with correct arguments 4head");
								return;
							}

							if (!float.TryParse(args[3], out float size))
							{
								ev.ReplyMessage = ("Invalid size");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}
							ItemType item = (ItemType)Enum.Parse(typeof(ItemType), args[2]);

							foreach (Player player in players)
							{
								Pickup yesnt = Exiled.API.Extensions.Item.Spawn(item, 0, player.Position);

								GameObject gameObject = yesnt.gameObject;
								gameObject.transform.localScale = Vector3.one * size;

								NetworkServer.UnSpawn(gameObject);
								NetworkServer.Spawn(yesnt.gameObject);
							}

							ev.ReplyMessage = (
									$"Yay, items! With sizes!!");
							return;
						}
					case "dummy":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.dummy"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 6)
							{
								ev.ReplyMessage = ("You must supply a player, dummy role, x size, y size and z size");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							RoleType role = RoleType.None;
							try
							{
								role = (RoleType)Enum.Parse(typeof(RoleType), args[2]);
							}
							catch (Exception)
							{
								ev.ReplyMessage = ($"Invalid role selected: {args[2]}");
								ev.Success = false;
								return;
							}

							if (role == RoleType.None)
							{
								ev.ReplyMessage = ("Cannot spawn a dummy without a role.");
								ev.Success = false;
								return;
							}

							if (!float.TryParse(args[3], out float x))
							{
								ev.ReplyMessage = ("Invalid x value.");
								return;
							}
							if (!float.TryParse(args[4], out float y))
							{
								ev.ReplyMessage = ("Invalid y value.");
								return;
							}
							if (!float.TryParse(args[5], out float z))
							{
								ev.ReplyMessage = ("Invalid z value.");
								return;
							}

							foreach (Player player in players)
								SpawnDummyModel(player.Position, player.GameObject.transform.localRotation, role, x, y,
									z);

							ev.ReplyMessage = ("Dummy(s) spawned.");
							break;
						}
					case "ragdoll":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.dolls"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 4)
							{
								ev.ReplyMessage = ("Try again");
								return;
							}

							if (!int.TryParse(args[3], out int count))
							{
								ev.ReplyMessage = ("Invalid number selected.");
								return;
							}

							if (!int.TryParse(args[2], out int role))
							{
								ev.ReplyMessage = ("Invalid roleID");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							ev.ReplyMessage = ("hehexd");
							foreach (Player player in players) Timing.RunCoroutine(SpawnBodies(player, role, count));

							return;
						}
					case "config":
						{
							if (args[1].ToLower() == "reload")
							{
								ev.IsAllowed = false;
								ServerStatic.PermissionsHandler.RefreshPermissions();
								ConfigFile.ReloadGameConfigs();
								ev.ReplyMessage = ($"Config files reloaded.");
							}

							return;
						}
					case "hp":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.hp"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 3)
							{
								ev.ReplyMessage = ("You must supply a player name/ID and an amount.");
								ev.Success = false;
								return;
							}

							if (!int.TryParse(args[2], out int result))
							{
								ev.ReplyMessage = ($"Invalid health amount: {args[2]}");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							foreach (Player player in players)
							{
								if (result > player.MaxHealth) 
									player.MaxHealth = result;

								player.Health = result;
								ev.ReplyMessage = (
									$"{player.Nickname} ({player.UserId}'s health has been set to {result}");
							}

							return;
						}
					case "cleanup":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.cleanup"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}
							if (args.Length < 2)
							{
								ev.ReplyMessage = ("You must supply a type of cleanup: items or ragdolls.");
								ev.Success = false;
								return;
							}

							if (args[1].ToLower() == "items")
								foreach (Pickup item in Object.FindObjectsOfType<Pickup>())
									item.Delete();
							else if (args[1].ToLower() == "ragdolls")
								foreach (Ragdoll doll in Object.FindObjectsOfType<Ragdoll>())
									NetworkServer.Destroy(doll.gameObject);
							ev.ReplyMessage = ("Cleanup complete.");
							return;
						}
					case "grenade":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.grenade"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.ReplyMessage = ($"Too few arguments. Value: {args.Length}, Expected 3");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							switch (args[2].ToLower())
							{
								case "frag":
									foreach (Player player in players)
									{
										GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
										GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
										if (grenade == null)
										{
											ev.ReplyMessage = ($"Something broke that really really <b>really</b> shouldn't have.. Notify Joker with the following error code: GS-NRE");
											ev.Success = false;
											return;
										}
										Grenade component = Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
										component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
										NetworkServer.Spawn(component.gameObject);
									}
									ev.ReplyMessage = ("Tick, tick.. BOOM!");
									break;
								case "flash":
									foreach (Player player in players)
									{
										GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
										GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFlash);
										if (grenade == null)
										{
											ev.ReplyMessage = ($"Something broke that really really <b>really</b> shouldn't have.. Notify Joker with the following error code: GS-NRE");
											ev.Success = false;
											return;
										}
										Grenade component = Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
										component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
										NetworkServer.Spawn(component.gameObject);
									}
									ev.ReplyMessage = ("Don't look at the light!");
									break;
								case "ball":
									foreach (Player player in players)
									{
										Vector3 spawnrand = new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f));
										GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
										GrenadeSettings ball = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
										if (ball == null)
										{
											ev.ReplyMessage = ($"TheMoogle broke something in his code that shouldn't have been.. Notify Joker with the error code: Mog's Balls don't work");
											ev.Success = false;
											return;
										}
										Grenade component = Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
										component.InitData(gm, spawnrand, Vector3.zero);
										NetworkServer.Spawn(component.gameObject);
									}
									ev.ReplyMessage = ("The Balls started bouncing!");
									break;
								default:
									ev.ReplyMessage = ("Enter either \"frag\", \"flash\" or \"ball\".");
									break;
							}
							break;
						}
					case "ball":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.ball"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 2)
							{
								ev.ReplyMessage = ($"Too few arguments. Value: {args.Length}, Expected 2");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
								Cassie.Message("pitch_1.5 xmas_bouncyballs", true, false);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							foreach (Player player in players)
							{
								Vector3 spawnrand = new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f));
								GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
								GrenadeSettings ball = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
								if (ball == null)
								{
									ev.ReplyMessage = ($"TheMoogle broke something in his code that shouldn't have been.. Notify Joker with the error code: Mog's Balls don't work");
									ev.Success = false;
									return;
								}
								Grenade component = Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
								component.InitData(gm, spawnrand, Vector3.zero);
								NetworkServer.Spawn(component.gameObject);
							}

							ev.ReplyMessage = ("The Balls started bouncing!");
							break;
						}
					case "kill":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.kill"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							List<Player> players = new List<Player>();
							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (player.Role != RoleType.Spectator)
										players.Add(player);
							}
							else
							{
								Player player = Player.Get(args[1]);
								if (player == null)
								{
									ev.ReplyMessage = ("Player not found.");
									ev.Success = false;
									return;
								}
								players.Add(player);
							}

							foreach (Player player in players)
							{
								int id = player.Id;

								player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(119000000, ev.Sender.Nickname, DamageTypes.Wall, id), player.GameObject);

								ev.ReplyMessage = ($"{player.Nickname} has been slayed.");
							}

							break;
						}
					case "inv":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.inv"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 3)
							{
								ev.ReplyMessage = ("Please provide a removal command and id");
								return;
							}

							switch (args[1].ToLower())
							{
								case "drop":
									if (args[2].ToLower() == "*" || args[2].ToLower() == "all")
									{
										foreach (Player player in Player.List)
											if (player.Role != RoleType.Spectator)
												player.Inventory.ServerDropAll();

										ev.ReplyMessage = ("Dropped all items in everyone's inventory");
									}
									else
									{
										Player player = Player.Get(args[2]);
										if (player == null)
										{
											ev.ReplyMessage = ($"Player {args[2]} not found");
											return;
										}

										player.Inventory.ServerDropAll();
										ev.ReplyMessage = ($"Dropped all items in {player.Nickname}'s inventory");
									}
									break;
								case "see":
									Player ply = Player.Get(args[2]);
									if (ply == null)
									{
										ev.ReplyMessage = ($"Player {args[2]} not found");
										return;
									}

									if (ply.Inventory.items.Count != 0)
									{
										string itemLister = $"Player {ply.Nickname} has the following items in their inventory (in order): ";
										foreach (Inventory.SyncItemInfo item in ply.Inventory.items) itemLister += item.id + ", ";
										itemLister = itemLister.Substring(0, itemLister.Count() - 2);
										ev.ReplyMessage = (itemLister);
										return;
									}
									ev.ReplyMessage = ($"Player {ply.Nickname} does not have any items in their inventory");
									break;
								default:
									ev.ReplyMessage = ("Please enter either \"clear\", \"drop\", or \"see\"");
									break;
							}
							break;
						}
					case "ik":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.ik"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 2)
							{
								ev.ReplyMessage = ("Please provide a id");
								return;
							}

							if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
							{
								foreach (Player player in Player.List)
									if (!player.ReferenceHub.TryGetComponent(out InstantKillComponent _))
									{
										player.ReferenceHub.gameObject.AddComponent<InstantKillComponent>();
									}

								ev.ReplyMessage = ("Instant killing is on for all players now");
							}
							else if (args[1].ToLower() == "list")
							{
								if (Plugin.IkHubs.Count != 0)
								{
									string playerLister = "Players with instant kill on: ";
									foreach (Player player in Plugin.IkHubs.Keys) 
										playerLister += player.Nickname + ", ";
									playerLister = playerLister.Substring(0, playerLister.Count() - 2);
									ev.ReplyMessage = (playerLister);
									return;
								}
								ev.ReplyMessage = ("No players currently online have instant killing on");
							}
							else if (args[1].ToLower() == "clear")
							{
								foreach (Player player in Plugin.IkHubs.Keys)
									Object.Destroy(player.ReferenceHub.GetComponent<InstantKillComponent>());

								ev.ReplyMessage = ("Instant killing is off for all players now");
							}
							else
							{
								Player ply = Player.Get(args[1]);
								if (ply == null)
								{
									ev.ReplyMessage = ($"Player {args[1]} not found");
									return;
								}

								if (!ply.ReferenceHub.TryGetComponent(out InstantKillComponent ikComponent))
								{
									ply.GameObject.AddComponent<InstantKillComponent>();
									ev.ReplyMessage = ($"Instant killing is on for {ply.Nickname}");
								}
								else
								{
									UnityEngine.Object.Destroy(ikComponent);
									ev.ReplyMessage = ($"Instant killing is off for {ply.Nickname}");
								}
							}
							break;
						}
					case "bd":
						{
							ev.IsAllowed = false;
							if (!sender.CheckPermission("at.bd"))
							{
								ev.ReplyMessage = ("Permission denied.");
								return;
							}

							if (args.Length < 2)
							{
								ev.ReplyMessage = ("Please provide a break command and an id (if needed) (Note: For \"list\" and \"clear\" you do not need an id)");
								return;
							}

							switch (args.Length)
							{
								case 2:
									switch (args[1].ToLower())
									{
										case "list":
											if (Plugin.BdHubs.Count != 0)
											{
												string playerLister = "Players with break doors permissions on: ";
												foreach (Player player in Plugin.BdHubs.Keys)
													playerLister += player.Nickname + ", ";
												playerLister = playerLister.Substring(0, playerLister.Count() - 2);
												ev.ReplyMessage = (playerLister);
												return;
											}
											ev.ReplyMessage = ("No players currently online have break door permissions on");
											break;
										case "clear":
											foreach (Player player in Plugin.BdHubs.Keys)
												Object.Destroy(player.ReferenceHub.GetComponent<BreakDoorComponent>());

											ev.ReplyMessage = ("Break door permissions is off for all players now");
											break;
										default:
											ev.ReplyMessage = ("Please enter either \"all\", \"clear\", \"doors\", or \"list\"");
											break;
									}
									break;
								case 3:
									switch (args[1].ToLower())
									{
										case "doors":
											if (args[2].ToLower() == "*" || args[2].ToLower() == "all")
											{
												foreach (Player player in Player.List)
													if (!player.ReferenceHub.TryGetComponent(out BreakDoorComponent bdComponent))
													{
														bdComponent = player.GameObject.AddComponent<BreakDoorComponent>();
														Plugin.BdHubs.Add(player, bdComponent);
													}
													else
													{
														bdComponent.breakAll = false;
													}

												ev.ReplyMessage = ("Instant breaking of doors is on for all players now");
												return;
											}

											Player ply = Player.Get(args[2]);
											if (ply == null)
											{
												ev.ReplyMessage = ($"Player {args[2]} not found");
												return;
											}

											if (!ply.ReferenceHub.TryGetComponent(out BreakDoorComponent doorBreak))
											{
												ev.ReplyMessage = ($"Instant breaking of doors is on for {ply.Nickname}");
												doorBreak = ply.GameObject.AddComponent<BreakDoorComponent>();
												Plugin.BdHubs.Add(ply, doorBreak);
												doorBreak.breakAll = false;
											}
											else
											{
												if (doorBreak.breakAll)
												{
													ev.ReplyMessage = ($"Instant breaking of doors is on for {ply.Nickname}");
													doorBreak.breakAll = false;
													return;
												}

												ev.ReplyMessage = ($"Instant breaking of doors is off for {ply.Nickname}");
												UnityEngine.Object.Destroy(doorBreak);
												Plugin.BdHubs.Remove(ply);
											}
											break;
										case "all":
											if (args[2].ToLower() == "*" || args[2].ToLower() == "all")
											{
												foreach (Player player in Player.List)
													if (!player.ReferenceHub.TryGetComponent(out BreakDoorComponent bdComponent))
													{
														bdComponent = player.GameObject.AddComponent<BreakDoorComponent>();
														bdComponent.breakAll = true;
														Plugin.BdHubs.Add(player, bdComponent);
													}
													else
													{
														bdComponent.breakAll = true;
													}

												ev.ReplyMessage = ("Instant breaking of everything is on for all players now");
												return;
											}

											ply = Player.Get(args[2]);
											if (ply == null)
											{
												ev.ReplyMessage = ($"Player {args[2]} not found");
												return;
											}

											if (!ply.ReferenceHub.TryGetComponent(out BreakDoorComponent doorBreaker))
											{
												ev.ReplyMessage = ($"Instant breaking of everything is on for {ply.Nickname}");
												doorBreak = ply.GameObject.AddComponent<BreakDoorComponent>();
												doorBreak.breakAll = true;
												Plugin.BdHubs.Add(ply, doorBreak);
											}
											else
											{
												if (!doorBreaker.breakAll)
												{
													ev.ReplyMessage = ($"Instant breaking of everything is on for {ply.Nickname}");
													doorBreaker.breakAll = true;
													return;
												}
												ev.ReplyMessage = ($"Instant breaking of everything is off for {ply.Nickname}");
												UnityEngine.Object.Destroy(doorBreaker);
												Plugin.BdHubs.Remove(ply);
											}
											break;
										default:
											ev.ReplyMessage = ("Please enter either \"all\", \"clear\", \"doors\", or \"list\"");
											break;
									}
									break;
							}
						}
						break;
					case "strip":
						if (!CommandProcessor.CheckPermissions(new PlayerCommandSender(ev.Sender.ReferenceHub.queryProcessor), args[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools", false)) return;
						ev.IsAllowed = false;
						if (args.Length < 2)
						{
							ev.ReplyMessage = ("Syntax: strip ((id/name)/*/all)");
							return;
						}
						if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
						{
							foreach (Player player in Player.List)
								if (player.Role != RoleType.Spectator)
									player.ClearInventory();

							ev.ReplyMessage = ("Cleared all items in everyone's inventory");
						}
						else
						{
							Player player = Player.Get(args[1]);
							if (player == null)
							{
								ev.ReplyMessage = ($"Player {args[1]} not found");
								return;
							}

							player.ClearInventory();
							ev.ReplyMessage = ($"Cleared all items in {player.Nickname}'s inventory");
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
			obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
			obj.GetComponent<QueryProcessor>().PlayerId = 9999;
			obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
			obj.transform.localScale = new Vector3(x, y, z);
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			NetworkServer.Spawn(obj);
		}

		private IEnumerator<float> SpawnBodies(Player player, int role, int count)
		{
			for (int i = 0; i < count; i++)
			{
				player.GameObject.GetComponent<RagdollManager>().SpawnRagdoll(player.Position + Vector3.up * 5,
					Quaternion.identity, Vector3.zero, role,
					new PlayerStats.HitInfo(1000f, player.UserId, DamageTypes.Falldown,
						player.Id), false, "SCP-343", "SCP-343", 0);
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

		public void SpawnItem(ItemType type, Vector3 pos)
		{
			Exiled.API.Extensions.Item.Spawn(type, 0, pos);
		}

		private IEnumerator<float> DoTut(Player player)
		{
			if (player.IsOverwatchEnabled)
				player.IsOverwatchEnabled = false;

			player.Role = RoleType.Tutorial;
			yield return Timing.WaitForSeconds(1f);
			Door[] d = UnityEngine.Object.FindObjectsOfType<Door>();
			foreach (Door door in d)
				if (door.DoorName == "SURFACE_GATE")
				{
					player.Position = door.transform.position + Vector3.up * 2;
					break;
				}

			player.ReferenceHub.serverRoles.CallTargetSetNoclipReady(player.ReferenceHub.characterClassManager.connectionToClient, true);
			player.ReferenceHub.serverRoles.NoclipReady = true;
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

		public IEnumerator<float> DoRocket(Player player, float speed)
		{
			const int maxAmnt = 50;
			int amnt = 0;
			while (player.Role != RoleType.Spectator)
			{
				player.Position = player.Position + Vector3.up * speed;
				amnt++;
				if (amnt >= maxAmnt)
				{
					player.IsGodModeEnabled = false;
					player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(1000000, "WORLD", DamageTypes.Grenade, 0),
						player.GameObject);
				}

				yield return Timing.WaitForOneFrame;
			}
		}

		public IEnumerator<float> DoJail(Player player, bool skipadd = false)
		{
			List<Inventory.SyncItemInfo> items = new List<Inventory.SyncItemInfo>();
			foreach (Inventory.SyncItemInfo item in player.Inventory.items)
				items.Add(item);
			if (!skipadd)
				plugin.JailedPlayers.Add(new Jailed
				{
					Health = player.Health,
					Position = player.Position,
					Items =  items,
					Name = player.Nickname,
					Role = player.Role,
					Userid = player.UserId,
				});
			if (player.IsOverwatchEnabled)
				player.IsOverwatchEnabled = false;
			yield return Timing.WaitForSeconds(1f);
			player.Role = RoleType.Tutorial;
			player.Position = new Vector3(53f, 1020f, -44f);
			player.ResetInventory(new List<Inventory.SyncItemInfo>());
		}

		private IEnumerator<float> DoUnJail(Player player)
		{
			Jailed jail = plugin.JailedPlayers.Find(j => j.Userid == player.UserId);
			player.Role = jail.Role;
			player.ResetInventory(jail.Items);
			yield return Timing.WaitForSeconds(1.5f);
			player.Health = jail.Health;
			player.Position = jail.Position;
			plugin.JailedPlayers.Remove(jail);
		}

		public void OnPlayerJoin(JoinedEventArgs ev)
		{
			try
			{
				if (plugin.JailedPlayers.Any(j => j.Userid == ev.Player.UserId))
					Timing.RunCoroutine(DoJail(ev.Player, true));

				if (File.ReadAllText(plugin.OverwatchFilePath).Contains(ev.Player.UserId))
				{
					Log.Debug($"Putting {ev.Player.UserId} into overwatch.");
					ev.Player.IsOverwatchEnabled = true;
				}

				if (File.ReadAllText(plugin.HiddenTagsFilePath).Contains(ev.Player.UserId))
				{
					Log.Debug($"Hiding {ev.Player.UserId}'s tag.");
					ev.Player.BadgeHidden = true;
				}
			}
			catch (Exception e)
			{
				Log.Error($"Player Join: {e}");
			}
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			try
			{
				List<string> overwatchRead = File.ReadAllLines(plugin.OverwatchFilePath).ToList();
				List<string> tagsRead = File.ReadAllLines(plugin.HiddenTagsFilePath).ToList();

				foreach (Player player in Player.List)
				{
					string userId = player.UserId;

					if (player.IsOverwatchEnabled && !overwatchRead.Contains(userId))
						overwatchRead.Add(userId);
					else if (!player.IsOverwatchEnabled && overwatchRead.Contains(userId))
						overwatchRead.Remove(userId);

					if (player.ReferenceHub.serverRoles._hideLocalBadge && !tagsRead.Contains(userId))
						tagsRead.Add(userId);
					else if (!player.ReferenceHub.serverRoles._hideLocalBadge && tagsRead.Contains(userId))
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

		public void OnTriggerTesla(TriggeringTeslaEventArgs ev)
		{
			if (ev.Player.IsGodModeEnabled)
				ev.IsTriggerable = false;
		}

		public void OnSetClass(ChangingRoleEventArgs ev)
		{
			if (plugin.Config.GodTuts)
				ev.Player.IsGodModeEnabled = ev.NewRole == RoleType.Tutorial;
		}

		public void OnWaitingForPlayers()
		{
			Plugin.IkHubs.Clear();
			Plugin.BdHubs.Clear();
		}
	}
}

