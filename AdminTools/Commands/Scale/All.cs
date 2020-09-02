using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Scale
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Scales all users by a specified value";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.size"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: scale (all / *) (value)";
                return false;
            }

            if (!float.TryParse(arguments.At(0), out float value))
            {
                response = $"Invalid value for scale: {arguments.At(0)}";
                return false;
            }

            foreach (Player Ply in Player.List)
                EventHandlers.SetPlayerScale(Ply.GameObject, value);

            response = $"Everyone's scale has been set to {value}";
            return true;
        }
    }
}
