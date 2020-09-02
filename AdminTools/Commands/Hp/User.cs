using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;


namespace AdminTools.Commands.Hp
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Sets a users HP to a specified value";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.hp"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: hp user (player id / name) (value)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (!int.TryParse(arguments.At(1), out int value))
            {
                response = $"Invalid value for HP: {value}";
                return false;
            }

            if (value <= 0)
                Ply.Kill();
            else
                Ply.Health = value;
            response = $"Player {Ply.Nickname}'s HP was set to {value}";
            return true;
        }
    }
}
