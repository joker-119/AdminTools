using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Scale
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Scales a user by a specified value";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.size"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: scale user (player id / name)) (value)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return true;
            }

            if (!float.TryParse(arguments.At(1), out float value))
            {
                response = $"Invalid value for scale: {arguments.At(1)}";
                return false;
            }

            EventHandlers.SetPlayerScale(Ply.GameObject, value);
            response = $"Player {Ply.Nickname}'s scale has been set to {value}";
            return true;
        }
    }
}
