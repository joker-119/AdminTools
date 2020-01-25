using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EXILED;
using MEC;
using Mirror;
using UnityEngine;
using Utf8Json.Resolvers.Internal;

namespace AdminTools
{
	public class EventHandlers
	{
		private readonly Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnCommand(ref RACommandEvent ev)
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
			string data = $"{DateTime.Now}: {ev.Sender.Nickname} ({ev.Sender.SenderId}) executed: {ev.Command} {Environment.NewLine}";
			File.AppendAllText(fileName, data);

			string[] args = ev.Command.Split(' ');
			ReferenceHub sender = Plugin.GetPlayer(ev.Sender.SenderId);

			switch (args[0].ToLower())
			{
				case "kick":
				{
					ev.Allow = false;
					IEnumerable<string> reasons = args.Where(s => s != args[0] && s != args[1]);
					string reason = "";
					foreach (string st in reasons)
						reason += st;
					GameObject obj = Plugin.GetPlayer(args[1])?.gameObject;
					if (obj == null)
					{
						ev.Sender.RAMessage("Player not found", false);
						return;
					}
					ServerConsole.Disconnect(obj, $"You have been kicked from the server: {reason}");
					ev.Sender.RAMessage("Player was kicked.");
					return;
				}
				case "reconnectrs":
				{
					foreach (ReferenceHub hub in Plugin.GetHubs())
						hub.playerStats.RpcRoundrestart(0);
					Application.Quit();
					ev.Sender.RAMessage("Restarting server...");
					return;
				}
				case "muteall":
				{
					ev.Allow = false;
					foreach (ReferenceHub hub in Plugin.GetHubs())
						if (!hub.serverRoles.RemoteAdmin)
							hub.characterClassManager.SetMuted(true);
					ev.Sender.RAMessage("All non-staff players have been muted.");
					return;
				}
				case "unmuteall":
				{
					ev.Allow = false;
					foreach (ReferenceHub hub in Plugin.GetHubs())
						if (!hub.serverRoles.RemoteAdmin)
							hub.characterClassManager.SetMuted(false);
					ev.Sender.RAMessage("All non-staff players have been muted.");
					return;
				}
				case "rocket":
				{
					ev.Allow = false;
					ReferenceHub hub = Plugin.GetPlayer(args[1]);
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
						foreach (ReferenceHub h in Plugin.GetHubs())
							Timing.RunCoroutine(DoRocket(h, result));
					else
						Timing.RunCoroutine(DoRocket(hub, result));
					ev.Sender.RAMessage("We're going on a trip, in our favorite rocketship.");
					return;
				}
				case "bc":
				{
					ev.Allow = false;
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
					ReferenceHub rh = Plugin.GetPlayer(args[1]);
					
					id = rh == null ? "Player not found" : rh.characterClassManager.UserId;
					ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} - {id}");
					break;
				}
				case "pbc":
				{
					ev.Allow = false;
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
					Plugin.GetPlayer(args[1])?.Broadcast(result, msg);
					ev.Sender.RAMessage("Message sent.");
					break;
				}
				case "tut":
				{
					ev.Allow = false;
					
					if (args.Length < 2)
					{
						ev.Sender.RAMessage("You must supply a player name or ID", false);
						return;
					}

					ReferenceHub rh = Plugin.GetPlayer(string.Join(" ", args.Skip(1)));
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
					foreach (ReferenceHub hub in Plugin.GetHubs())
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
					foreach (ReferenceHub hub in Plugin.GetHubs())
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
					if (args.Length < 2)
					{
						ev.Sender.RaReply("Joker's Plugin#You must supply a player name or ID", false, true, string.Empty);
						return;
					}

					var array = args.Where(a => a != args[0]);
					string filter = null;
					foreach (string s in array)
						filter += s;
					ReferenceHub target = Plugin.GetPlayer(filter);
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
							rh.Broadcast(result, $"{ev.Sender.Nickname}: {msg}");
					}

					ev.Sender.RAMessage("Message sent to all online staff members.");

					break;
				}
				case "drop":
				{
					ev.Allow = false;
					int result;
					if (args.Length != 4)
					{
						ev.Sender.RAMessage($"Invalid arguments.{args.Length}");
						break;
					}

					ReferenceHub hub = Plugin.GetPlayer(args[1]);
					if (hub == null)
					{
						ev.Sender.RAMessage("Player not found.");
						break;
					}

					ItemType type = GetItemFromId(args[2]);

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
						SpawnItem(type, hub.gameObject.transform.position, Vector3.zero);
					ev.Sender.RAMessage("Done. hehexd");
					return;
				}
				case "pos":
				{
					ev.Allow = false;
					
					if (args.Length < 3)
					{
						ev.Sender.RAMessage("You must supply a player name/ID and a subcommand.", false);
						return;
					}

					ReferenceHub rh = Plugin.GetPlayer(args[1]);
					if (rh == null)
					{
						ev.Sender.RAMessage("Player not found.", false);
						return;
					}

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
							rh.plyMovementSync.OverridePosition(new Vector3(x, y, z), 0f, false );
							ev.Sender.RAMessage($"Player {rh.nicknameSync.MyNick} - {rh.characterClassManager.UserId} moved to x{x} y{y} z{z}");
							break;
						}
						case "get":
						{
							Vector3 pos = rh.gameObject.transform.position;
							string ret = $"{rh.nicknameSync.MyNick} - {rh.characterClassManager.UserId} Position: x {pos.x} y {pos.y} z {pos.z}";
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
									rh.plyMovementSync.OverridePosition(new Vector3(pos.x + newPos, pos.y, pos.z), 0f);
									break;
								case "y":
									rh.plyMovementSync.OverridePosition(new Vector3(pos.x, pos.y + newPos, pos.z), 0f);
									break;
								case "z":
									rh.plyMovementSync.OverridePosition(new Vector3(pos.x, pos.y, pos.z + newPos), 0f);
									break;
							}
							ev.Sender.RAMessage($"Player {rh.nicknameSync.MyNick} - {rh.characterClassManager.UserId} position changed.");
							break;
						}
					}

					break;
				}
				case "tpx":
				{
					ev.Allow = false;
					
					if (args.Length < 3)
					{
						ev.Sender.RAMessage("You must supply a player name/ID to teleport and a player name/ID to teleport them to.");
						return;
					}

					ReferenceHub rh = Plugin.GetPlayer(args[1]);
					ReferenceHub target = Plugin.GetPlayer(args[2]);
					if (rh == null)
					{
						ev.Sender.RAMessage($"Player {args[1]} not found.");
						return;
					}

					if (target == null)
					{
						ev.Sender.RAMessage($"Player {args[2]} not found.");
						return;
					}
					rh.plyMovementSync.OverridePosition(target.plyMovementSync.RealModelPosition, 0f, false);
					ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} teleported to {target.nicknameSync.MyNick}");
					break;
				}
				case "ghost":
				{
					ev.Allow = false;
					if (args.Length < 2)
					{
						ev.Sender.RAMessage("You must supply a playername to ghost.", false);
						return;
					}

					ReferenceHub rh = Plugin.GetPlayer(args[1]);
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
				case "restart":
				{
					ev.Allow = false;
					if (ev.Sender.Nickname != "BLART" || ev.Sender.SenderId != "BLART")
					{
						ev.Sender.RAMessage("Permission denied.");
						return;
					}
					ev.Sender.RAMessage("Restarting server forcefully...");
					foreach (PlayerStats stats in UnityEngine.Object.FindObjectsOfType<PlayerStats>())
						stats.CallRpcRoundrestart(0f);
					Application.Quit();
					return;
				}
				case "scale":
				{
					ev.Allow = false;
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

					ReferenceHub rh = Plugin.GetPlayer(args[1]);
					if (rh == null)
					{
						ev.Sender.RAMessage("Player not found.");
						return;
					}
					
					SetPlayerScale(rh.gameObject, scale);
					ev.Sender.RAMessage($"{rh.nicknameSync.MyNick} size set to {scale}");
					return;
				}
				case "size":
				{
					ev.Allow = false;
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

					ReferenceHub rh = Plugin.GetPlayer(args[1]);

					if (rh == null)
					{
						ev.Sender.RAMessage($"Player not found: {args[1]}", false);
						return;
					}
					
					SetPlayerScale(rh.gameObject, x, y, z);
					ev.Sender.RAMessage($"{rh.nicknameSync.MyNick}'s size has been changed.");
					return;
				}
			}
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
		
		 public ItemType GetItemFromId(string id)
		 {
			 switch (id.ToUpper())
			{
				case "018":
				case "BALL":
					return ItemType.SCP018;
				case "05":
					return ItemType.KeycardO5;
				case "AMMO1":
					return ItemType.Ammo556;
				case "AMMO2":
					return ItemType.Ammo762;
				case "AMMO3":
					return ItemType.Ammo9mm;
				case "CENGINEER":
					return ItemType.KeycardContainmentEngineer;
				case "CHAOS_INSURGENCY":
					return ItemType.KeycardChaosInsurgency;
				case "CI":
					return ItemType.KeycardChaosInsurgency;
				case "COIN":
					return ItemType.Coin;
				case "COM15":
					return ItemType.GunCOM15;
				case "CONTAINMENT_ENGINEER":
					return ItemType.KeycardContainmentEngineer;
				case "DISARMER":
					return ItemType.Disarmer;
				case "E11":
					return ItemType.GunE11SR;
				case "E11_AMMO":
					return ItemType.Ammo556;
				case "E11_STANDARD_RIFLE":
					return ItemType.GunE11SR;
				case "EAMMO":
					return ItemType.Ammo556;
				case "EPSILON":
					return ItemType.GunE11SR;
				case "EPSILON11":
					return ItemType.GunE11SR;
				case "EPSILON11_STANDARD_RIFLE":
					return ItemType.GunE11SR;
				case "EPSILON_RIFLE":
					return ItemType.GunE11SR;
				case "EPSILON_STANDARD_RIFLE":
					return ItemType.GunE11SR;
				case "E_AMMO":
					return ItemType.Ammo556;
				case "FACILITY_MANAGER":
					return ItemType.KeycardFacilityManager;
				case "FLASH":
					return ItemType.GrenadeFlash;
				case "FLASHBANG":
					return ItemType.GrenadeFlash;
				case "FLASHLIGHT":
					return ItemType.Flashlight;
				case "FMANAGER":
					return ItemType.KeycardFacilityManager;
				case "FRAG":
					return ItemType.GrenadeFrag;
				case "FRAG_GRENADE":
					return ItemType.GrenadeFrag;
				case "GRENADE":
					return ItemType.GrenadeFrag;
				case "GUARD":
					return ItemType.KeycardGuard;
				case "JANITOR":
					return ItemType.KeycardJanitor;
				case "LOGICER":
					return ItemType.GunLogicer;
				case "LOGICIZER":
					return ItemType.GunLogicer;
				case "MAJOR_SCIENTIST":
					return ItemType.KeycardScientistMajor;
				case "MEDKIT":
					return ItemType.Medkit;
				case "MICRO":
					return ItemType.MicroHID;
				case "MICROHID":
					return ItemType.MicroHID;
				case "MP4":
					return ItemType.GunMP7;
				case "MP7":
					return ItemType.GunMP7;
				case "MSCIENTIST":
					return ItemType.KeycardScientistMajor;
				case "MTFC":
					return ItemType.KeycardNTFCommander;
				case "MTFL":
					return ItemType.KeycardNTFLieutenant;
				case "MTF_COMMANDER":
					return ItemType.KeycardNTFCommander;
				case "MTF_LIEUTENANT":
					return ItemType.KeycardNTFLieutenant;
				case "O5":
					return ItemType.KeycardO5;
				case "O5_LEVEL":
					return ItemType.KeycardO5;
				case "P90":
					return ItemType.GunProject90;
				case "PISTOL":
					return ItemType.GunCOM15;
				case "PROJECT_90":
					return ItemType.GunProject90;
				case "RADIO":
					return ItemType.Radio;
				case "RIFLE":
					return ItemType.GunE11SR;
				case "SCIENTIST":
					return ItemType.KeycardScientist;
				case "SENIOR_GUARD":
					return ItemType.KeycardSeniorGuard;
				case "SGUARD":
					return ItemType.KeycardSeniorGuard;
				case "WEAPON_MANAGER":
					return ItemType.WeaponManagerTablet;
				case "WEAPON_MANAGER_TABLET":
					return ItemType.WeaponManagerTablet;
				case "WMT":
					return ItemType.WeaponManagerTablet;
				case "ZMANAGER":
					return ItemType.KeycardZoneManager;
				case "ZONE_MANAGER":
					return ItemType.KeycardZoneManager;
				default:
					return ItemType.Coin;
			}
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
					if (player == target)
						continue;
					
					NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;

					playerCon.Send(destroyMessage, 0);
					
					object[] parameters = new object[] {identity, playerCon};
					typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
				}
			}
			catch (Exception e)
			{
				Plugin.Info($"Set Scale error: {e}");
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
					
					object[] parameters = new object[] {identity, playerCon};
					typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
				}
			}
			catch (Exception e)
			{
				Plugin.Info($"Set Scale error: {e}");
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
					ev.Player.serverRoles.OverwatchEnabled = true;
				if (File.ReadAllText(plugin.HiddenTagsFilePath).Contains(ev.Player.characterClassManager.UserId))
				{
					ev.Player.serverRoles._hideLocalBadge = true;
					ev.Player.serverRoles.RefreshHiddenTag();
				}
			}
			catch (Exception e)
			{
				Plugin.Error($"Player Join: {e}");
			}
		}

		public void OnRoundEnd()
		{
			try
			{
				List<string> overwatchRead = File.ReadAllLines(plugin.OverwatchFilePath).ToList();
				List<string> tagsRead = File.ReadAllLines(plugin.HiddenTagsFilePath).ToList();

				foreach (ReferenceHub hub in Plugin.GetHubs())
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

				File.WriteAllLines(plugin.OverwatchFilePath, overwatchRead);
				File.WriteAllLines(plugin.HiddenTagsFilePath, tagsRead);
			}
			catch (Exception e)
			{
				Plugin.Error($"Round End: {e}");
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
	}
}