using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Ghost
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] {  };

        public string Description { get; } = "Sets a user to be invisible";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ghost"))
            {
                response = "You do not have permission to run this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: ghost user (player id / name)";
                return false;
            }

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
