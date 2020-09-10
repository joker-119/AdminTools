using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdminTools.Commands.Dummy
{
    public class ClearAll : ICommand
    {
        public string Command { get; } = "clearall";

        public string[] Aliases { get; } = new string[] { "ca" };

        public string Description { get; } = "Removes all spawned dummies from everyone";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.dummy"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: dummy clear";
                return false;
            }

            foreach (KeyValuePair<Player, List<GameObject>> Ply in Plugin.DumHubs)
            {
                foreach (GameObject Dum in Ply.Value)
                    UnityEngine.Object.Destroy(Dum);
                Ply.Value.Clear();
            }

            Plugin.BchHubs.Clear();
            response = $"All spawned dummies have now been removed";
            return true;
        }
    }
}
