using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Ghost
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Ghost : ParentCommand
    {
        public Ghost() => LoadGeneratedCommands();

        public override string Command { get; } = "ghost";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Sets everyone or a user to be invisible";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ghost"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage:\nghost ((player id / name) or (all / *))" +
                    "\nghost clear";
                return false;
            }

            switch (arguments.At(0))
            {
                case "clear":
                    foreach (Player Pl in Player.List)
                        Pl.IsInvisible = false;

                    response = "Everyone is no longer invisible";
                    return true;
                case "*":
                case "all":
                    foreach (Player Pl in Player.List)
                        Pl.IsInvisible = true;

                    response = "Everyone is now invisible";
                    return true;
                default:
                    Player Ply = Player.Get(arguments.At(0));
                    if (Ply == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (!Ply.IsInvisible)
                    {
                        Ply.IsInvisible = true;
                        response = $"Player {Ply.Nickname} is now invisible";
                    }
                    else
                    {
                        Ply.IsInvisible = false;
                        response = $"Player {Ply.Nickname} is no longer invisible";
                    }
                    return true;
            }
        }
    }
}
