using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;
using System.Linq;

namespace AdminTools.Commands.Jail
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Jail : ParentCommand
    {
        public Jail() => LoadGeneratedCommands();

        public override string Command { get; } = "jail";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Jails or unjails a user";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.jail"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: jail (player id / name)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (Plugin.JailedPlayers.Any(j => j.Userid == Ply.UserId))
            {
                Timing.RunCoroutine(EventHandlers.DoUnJail(Ply));
                response = $"Player {Ply.Nickname} has been unjailed now";
            }
            else
            {
                Timing.RunCoroutine(EventHandlers.DoJail(Ply));
                response = $"Player {Ply.Nickname} has been jailed now";
            }
            return true;
        }
    }
}
