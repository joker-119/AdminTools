using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NorthwoodLib.Pools;
using System;
using System.Text;

namespace AdminTools.Commands.BreakDoors
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class BreakDoors : ParentCommand
    {
        public BreakDoors() => LoadGeneratedCommands();

        public override string Command { get; } = "breakdoors";

        public override string[] Aliases { get; } = new string[] { "bd" };

        public override string Description { get; } = "Manage breaking door/gate properties for players";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.bd"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage:\nbreakdoors ((player id / name) or (all / *)) ((doors) or (all))" +
                    "\nbreakdoors clear" +
                    "\nbreakdoors list" +
                    "\nbreakdoors remove (player id / name)";
                return false;
            }

            switch (arguments.At(0))
            {
                case "clear":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: breakdoors clear";
                        return false;
                    }

                    foreach (Player Ply in Plugin.BdHubs.Keys)
                        if (Ply.ReferenceHub.TryGetComponent(out BreakDoorComponent BdCom))
                            UnityEngine.Object.Destroy(BdCom);

                    response = "Breaking doors has been removed from everyone";
                    return true;
                case "list":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: breakdoors list";
                        return false;
                    }

                    StringBuilder PlayerLister = StringBuilderPool.Shared.Rent(Plugin.BdHubs.Count != 0 ? "Players with break doors on:\n" : "No players currently online have breaking doors on");
                    if (Plugin.BdHubs.Count == 0)
                    {
                        response = PlayerLister.ToString();
                        return true;
                    }

                    foreach (Player Ply in Plugin.BdHubs.Keys)
                    {
                        PlayerLister.Append(Ply.Nickname);
                        PlayerLister.Append(", ");
                    }

                    string msg = PlayerLister.ToString().Substring(0, PlayerLister.ToString().Length - 2);
                    StringBuilderPool.Shared.Return(PlayerLister);
                    response = msg;
                    return true;
                case "remove":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: breakdoors remove (player id / name)";
                        return false;
                    }

                    Player Pl = Player.Get(arguments.At(1));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    if (Pl.ReferenceHub.TryGetComponent(out BreakDoorComponent BdComponent))
                    {
                        Plugin.BdHubs.Remove(Pl);
                        UnityEngine.Object.Destroy(BdComponent);
                        response = $"Breaking doors is off for {Pl.Nickname}";
                    }
                    else
                        response = $"Player {Pl.Nickname} does not have the ability to break doors";
                    return true;
                case "*":
                case "all":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: breakdoors (all / *) ((doors) or (all))";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out BreakType Type))
                    {
                        response = $"Invalid breaking type: {arguments.At(1)}";
                        return false;
                    }

                    foreach (Player Ply in Player.List)
                    {
                        if (!Ply.ReferenceHub.TryGetComponent(out BreakDoorComponent BdCom))
                        {
                            Ply.GameObject.AddComponent<BreakDoorComponent>();
                            switch (Type)
                            {
                                case BreakType.Doors:
                                    Ply.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = false;
                                    Ply.IsBypassModeEnabled = false;
                                    break;
                                case BreakType.All:
                                    Ply.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = true;
                                    Ply.IsBypassModeEnabled = true;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Type)
                            {
                                case BreakType.Doors:
                                    BdCom.breakAll = false;
                                    Ply.IsBypassModeEnabled = false;
                                    break;
                                case BreakType.All:
                                    BdCom.breakAll = true;
                                    Ply.IsBypassModeEnabled = true;
                                    break;
                            }
                        }
                    }

                    response = $"Breaking {((Type == BreakType.Doors) ? "doors" : "everything")} is on for everyone now";
                    return true;
                default:
                    if (arguments.Count != 2)
                    {
                        response = "Usage: breakdoors (player id / name) ((doors) or (all))";
                        return false;
                    }

                    Player Plyr = Player.Get(arguments.At(0));
                    if (Plyr == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out BreakType T))
                    {
                        response = $"Invalid breaking type: {arguments.At(1)}";
                        return false;
                    }

                    if (!Plyr.ReferenceHub.TryGetComponent(out BreakDoorComponent BdComp))
                    {
                        Plyr.GameObject.AddComponent<BreakDoorComponent>();
                        switch (T)
                        {
                            case BreakType.Doors:
                                Plyr.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = false;
                                Plyr.IsBypassModeEnabled = false;
                                break;
                            case BreakType.All:
                                Plyr.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = true;
                                Plyr.IsBypassModeEnabled = true;
                                break;
                        }

                        response = $"Breaking {((T == BreakType.Doors) ? "doors" : "all")} is on for {Plyr.Nickname}";
                    }
                    else
                    {
                        switch (T)
                        {
                            case BreakType.Doors:
                                BdComp.breakAll = false;
                                Plyr.IsBypassModeEnabled = false;
                                break;
                            case BreakType.All:
                                BdComp.breakAll = true;
                                Plyr.IsBypassModeEnabled = true;
                                break;
                        }

                        response = $"Breaking {((T == BreakType.Doors) ? "doors" : "all")} is on for {Plyr.Nickname}";
                    }
                    return true;
            }
        }
    }
}
