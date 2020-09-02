using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UnityEngine;

namespace AdminTools.Commands.DropItem
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Spawns a specificed number of an item on all users";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.items"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: dropitem all (ItemType) (amount (15 max))";
                return false;
            }

            if (!Enum.TryParse(arguments.At(0), true, out ItemType Item))
            {
                response = $"Invalid value for item type: {arguments.At(0)}";
                return false;
            }

            if (!uint.TryParse(arguments.At(1), out uint Amount) && Amount > 15)
            {
                response = $"Invalid amount of item to drop: {arguments.At(1)}";
                return false;
            }

            foreach (Player Ply in Player.List)
                for (int i = 0; i < Amount; i++)
                    EventHandlers.SpawnItem(Item, Ply.Position, Vector3.zero);

            response = $"{Amount} of {Item.ToString()} was spawned on everyone (\"Hehexd\" - Galaxy119)";
            return true;
        }
    }
}
