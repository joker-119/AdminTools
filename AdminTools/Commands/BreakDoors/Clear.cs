using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.BreakDoors
{
    public class Clear : ICommand
    {
        public string Command { get; } = "clear";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Removes the ability to break doors/gates from every player";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.bd"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: breakdoors clear";
                return false;
            }

            foreach (Player Ply in Plugin.BdHubs.Keys)
                if (Ply.ReferenceHub.TryGetComponent(out BreakDoorComponent BdCom))
                    UnityEngine.Object.Destroy(BdCom);

            Map.Broadcast(5, "Breaking doors has been removed from everyone now");
            response = "Instant killing has been removed from everyone";
            return true;
        }
    }
}
