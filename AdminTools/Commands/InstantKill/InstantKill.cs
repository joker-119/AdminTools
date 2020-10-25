using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NorthwoodLib.Pools;
using System;
using System.Text;

namespace AdminTools.Commands.InstantKill
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class InstantKill : ParentCommand
    {
        public InstantKill() => LoadGeneratedCommands();

        public override string Command { get; } = "instakill";

        public override string[] Aliases { get; } = new string[] { "ik" };

        public override string Description { get; } = "Manage instant kill properties for users";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ik"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage:\ninstakill ((player id / name) or (all / *))" +
                    "\ninstakill clear" +
                    "\ninstakill list" +
                    "\ninstakill remove (player id / name)";
                return false;
            }

            switch (arguments.At(0))
            {
                case "clear":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: instakill clear";
                        return false;
                    }

                    foreach (Player Ply in Plugin.IkHubs.Keys)
                        if (Ply.ReferenceHub.TryGetComponent(out InstantKillComponent IkCom))
                            UnityEngine.Object.Destroy(IkCom);

                    response = "Instant killing has been removed from everyone";
                    return true;
                case "list":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: instakill clear";
                        return false;
                    }

                    StringBuilder PlayerLister = StringBuilderPool.Shared.Rent(Plugin.IkHubs.Count != 0 ? "Players with instant killing on:\n" : "No players currently online have instant killing on");
                    if (Plugin.IkHubs.Count == 0)
                    {
                        response = PlayerLister.ToString();
                        return true;
                    }

                    foreach (Player Ply in Plugin.IkHubs.Keys)
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
                        response = "Usage: instakill remove (player id / name)";
                        return false;
                    }

                    Player Pl = Player.Get(arguments.At(1));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    if (Pl.ReferenceHub.TryGetComponent(out InstantKillComponent IkComponent))
                    {
                        Plugin.IkHubs.Remove(Pl);
                        UnityEngine.Object.Destroy(IkComponent);
                        response = $"Instant killing is off for {Pl.Nickname} now";
                    }
                    else
                        response = $"Player {Pl.Nickname} does not have the ability to instantly kill others";
                    return true;
                case "*":
                case "all":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: instakill all / *";
                        return false;
                    }

                    foreach (Player Ply in Player.List)
                        if (!Ply.ReferenceHub.TryGetComponent(out InstantKillComponent _))
                            Ply.ReferenceHub.gameObject.AddComponent<InstantKillComponent>();

                    response = "Everyone on the server can instantly kill other users now";
                    return true;
                default:
                    if (arguments.Count != 1)
                    {
                        response = "Usage: instakill (player id / name)";
                        return false;
                    }

                    Player Plyr = Player.Get(arguments.At(0));
                    if (Plyr == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (!Plyr.ReferenceHub.TryGetComponent(out InstantKillComponent ikComponent))
                    {
                        Plyr.GameObject.AddComponent<InstantKillComponent>();
                        response = $"Instant killing is on for {Plyr.Nickname}";
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(ikComponent);
                        response = $"Instant killing is off for {Plyr.Nickname}";
                    }
                    return true;
            }
        }
    }
}
