using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Ghost
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Sets everyone to be invisible";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ghost"))
            {
                response = "You do not have permission to run this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: ghost (all / *)";
                return false;
            }

            foreach (Player Ply in Player.List)
                Ply.IsInvisible = true;

            response = "Everyone is now invisible";
            return true;
        }
    }
}
