using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using UnityEngine;


namespace AdminTools.Commands.DropItem
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Spawns a specificed number of an item on a user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.items"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 3)
            {
                response = "Usage: dropitem (player id / name) (ItemType) (amount (200 max))";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out ItemType Item))
            {
                response = $"Invalid value for item type: {arguments.At(1)}";
                return false;
            }

            if (!uint.TryParse(arguments.At(2), out uint Amount) && Amount > 200)
            {
                response = $"Invalid amount of item to drop: {arguments.At(2)}";
                return false;
            }

            for (int i = 0; i < Amount; i++)
                EventHandlers.SpawnItem(Item, Ply.Position, Vector3.zero);
            response = $"{Amount} of {Item.ToString()} was spawned on {Ply.Nickname} (\"Hehexd\" - Galaxy119)";
            return true;
        }
    }
}
