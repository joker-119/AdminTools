using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.InstantKill
{
    public class Clear : ICommand
    {
        public string Command { get; } = "clear";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Removes instant kill from anyone who has it";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ik"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: instakill clear";
                return false;
            }

            foreach (Player Ply in Plugin.IkHubs.Keys) 
                if (Ply.ReferenceHub.TryGetComponent(out BreakDoorComponent BdCom))
                    UnityEngine.Object.Destroy(BdCom);

            response = "Instant killing has been removed from everyone";
            return true;
        }
    }
}
