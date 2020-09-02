using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Scale
{
    public class Reset : ICommand
    {
        public string Command { get; } = "reset";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Resets all users to the default size";

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
                response = "Usage: scale reset";
                return false;
            }

            foreach (Player Ply in Player.List)
                EventHandlers.SetPlayerScale(Ply.GameObject, 1);

            response = $"Everyone's scale has been reset";
            return true;
        }
    }
}