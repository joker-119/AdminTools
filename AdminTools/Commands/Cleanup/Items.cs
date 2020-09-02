using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Cleanup
{
    class Items : ICommand
    {
        public string Command { get; } = "items";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Cleans up items dropped on the ground from the server";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.cleanup"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: cleanup items";
                return false;
            }

            foreach (Pickup item in UnityEngine.Object.FindObjectsOfType<Pickup>())
                item.Delete();

            response = "Items have been cleaned up now";
            return true;
        }
    }
}
