using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;

namespace AdminTools.Commands.Strip
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Clears a users inventory instantly";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!CommandProcessor.CheckPermissions(((CommandSender)sender), "strip", PlayerPermissions.PlayersManagement, "AdminTools", false))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: strip user (id / name)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            Ply.ClearInventory();
            response = $"Player {Ply.Nickname}'s inventory have been cleared now";
            return true;
        }
    }
}
