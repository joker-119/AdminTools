using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
namespace AdminTools.Commands.Ghost
{
    public class Clear : ICommand
    {
        public string Command { get; } = "clear";

        public string[] Aliases { get; } = new string[] {  };

        public string Description { get; } = "Sets everyone to not be invisible";

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
                response = "Usage: ghost clear";
                return false;
            }

            foreach (Player Ply in Player.List)
                Ply.IsInvisible = false;

            response = "Everyone is no longer invisible";
            return true;
        }
    }
}
