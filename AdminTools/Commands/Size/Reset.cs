using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Size
{
    public class Reset : ICommand
    {
        public string Command { get; } = "reset";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Resizes all players back to a scale of one";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.size"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: size reset";
                return false;
            }

            foreach (Player Ply in Player.List)
            {
                if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                    continue;

                EventHandlers.SetPlayerScale(Ply.GameObject, 1, 1, 1);
            }

            response = $"Everyone's size has been reset";
            return true;
        }
    }
}
