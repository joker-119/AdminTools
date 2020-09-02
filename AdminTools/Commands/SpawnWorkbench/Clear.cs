using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UnityEngine;

namespace AdminTools.Commands.SpawnWorkbench
{
    public class Clear : ICommand
    {
        public string Command { get; } = "clear";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Removes all spawned workbenches";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.benches"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: spawnworkbench clear";
                return false;
            }

            foreach (GameObject Bench in EventHandlers.Benches)
                UnityEngine.Object.Destroy(Bench);
            EventHandlers.Benches.Clear();

            response = $"All spawned workbenches have now been removed";
            return true;
        }
    }
}
