using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace AdminTools.Commands.SpawnWorkbench
{
    public class Clear : ICommand
    {
        public string Command { get; } = "clear";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Removes all spawned workbenches from everyone";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.benches"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 3)
            {
                response = "Usage:\nspawnworkbench clear (player id / name) (minimum index) (maximum index)\n\nNOTE: Minimum index < Maximum index, You can remove from a range of all the benches you spawned (From 1 to (how many you spawned))";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (!int.TryParse(arguments.At(1), out int Min) && Min < 0)
            {
                response = $"Invalid value for minimum index: {arguments.At(1)}";
                return false;
            }

            if (!int.TryParse(arguments.At(2), out int Max) && Max < 0)
            {
                response = $"Invalid value for maximum index: {arguments.At(1)}";
                return false;
            }

            if (Max < Min)
            {
                response = $"{Max} is not greater than {Min}";
                return false;
            }

            if (!Plugin.BchHubs.TryGetValue(Ply, out List<GameObject> objs))
            {
                response = $"{Ply.Nickname} has not spawned in any workbenches";
                return false;
            }

            if (Min > objs.Count)
            {
                response = $"{Min} (minimum) is higher than the number of workbenches {Ply.Nickname} spawned! (Which is {objs.Count})";
                return false;
            }

            if (Max > objs.Count)
            {
                response = $"{Max} (maximum) is higher than the number of workbenches {Ply.Nickname} spawned! (Which is {objs.Count})";
                return false;
            }

            Min = Min == 0 ? 0 : Min - 1;
            Max = Max == 0 ? 0 : Max - 1;

            for (int i = Min; i <= Max; i++)
            {
                UnityEngine.Object.Destroy(objs.ElementAt(i));
                objs[i] = null;
            }
            objs.RemoveAll(r => r == null);

            response = $"All workbenches from {Min + 1} to {Max + 1} have been cleared from Player {Ply.Nickname}";
            return true;
        }
    }
}
