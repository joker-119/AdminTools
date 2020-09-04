using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdminTools.Commands.SpawnWorkbench
{
    public class Count : ICommand
    {
        public string Command { get; } = "count";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Counts the number of workbenches a user has spawned in";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.benches"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: spawnworkbench count (player id / name)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (!Plugin.BchHubs.TryGetValue(Ply, out List<GameObject> objs) || objs.Count == 0)
            {
                response = $"{Ply.Nickname} has not spawned in any workbenches";
                return false;
            }

            response = $"{Ply.Nickname} has spawned in {(objs.Count != 1 ? $"{objs.Count} workbenches" : $"{objs.Count} workbench")}";
            return true;
        }
    }
}
