using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.TeleportX
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class TeleportX : ParentCommand
    {
        public TeleportX() => LoadGeneratedCommands();

        public override string Command { get; } = "teleportx";

        public override string[] Aliases { get; } = new string[] { "tpx" };

        public override string Description { get; } = "Teleports all users or a user to another user";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.tp"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: teleportx (People teleported: (player id / name) or (all / *)) (Teleported to: (player id / name) or (all / *))";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    Player Ply = Player.Get(arguments.At(1));
                    if (Ply == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }


                    foreach (Player Plyr in Player.List)
                    {
                        if (Plyr.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                            continue;

                        Plyr.Position = Ply.Position;
                    }

                    response = $"Everyone has been teleported to Player {Ply.Nickname}";
                    return true;
                default:
                    Player Pl = Player.Get(arguments.At(0));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    Player Plr = Player.Get(arguments.At(1));
                    if (Plr == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    Pl.Position = Plr.Position;
                    response = $"Player {Pl.Nickname} has been teleported to Player {Plr.Nickname}";
                    return true;
            }
        }
    }
}
