using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Hp
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Sets all users HP to a specified value";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.hp"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: hp (all / *) (value)";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int value))
            {
                response = $"Invalid value for HP: {value}";
                return false;
            }

            foreach (Player Ply in Player.List)
            {
                if (value <= 0)
                    Ply.Kill();
                else
                    Ply.Health = value;
            }

            response = $"Everyone's HP was set to {value}";
            return true;
        }
    }
}
