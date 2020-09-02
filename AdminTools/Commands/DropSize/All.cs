using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using System;
using UnityEngine;

namespace AdminTools.Commands.DropSize
{
    class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Drops a selected amount of a selected item on all users";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.items"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "\nUsage:\ndrops (all / *) (ItemType) (size) \ndrops (all / *) (ItemType) (x size) (y size) (z size)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(0), true, out ItemType Type))
            {
                response = $"Invalid value for item name: {arguments.At(0)}";
                return false;
            }

            switch (arguments.Count)
            {
                case 2:
                    if (!float.TryParse(arguments.At(1), out float size))
                    {
                        response = $"Invalid value for item scale: {arguments.At(1)}";
                        return false;
                    }
                    SpawnItem(Type, size, out string msg);
                    response = msg;
                    return true;
                    break;
                case 4:
                    if (!float.TryParse(arguments.At(1), out float xval))
                    {
                        response = $"Invalid value for item scale: {arguments.At(1)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(2), out float yval))
                    {
                        response = $"Invalid value for item scale: {arguments.At(2)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(3), out float zval))
                    {
                        response = $"Invalid value for item scale: {arguments.At(3)}";
                        return false;
                    }
                    SpawnItem(Type, xval, yval, zval, out string message);
                    response = message;
                    return true;
                default:
                    response = "\nUsage:\ndrops (all / *) (ItemType) (size) \ndrops (all / *) (ItemType) (x size) (y size) (z size)";
                    return false;
            }
        }

        private void SpawnItem(ItemType Type, float size, out string message)
        {
            foreach (Player Ply in Player.List)
            {
                if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                    continue;

                Pickup Item = Exiled.API.Extensions.Item.Spawn(Type, 0, Ply.Position);
                GameObject gameObject = Item.gameObject;
                gameObject.transform.localScale = Vector3.one * size;
                NetworkServer.UnSpawn(gameObject);
                NetworkServer.Spawn(Item.gameObject);
            }
            message =$"Spawned in a {Type.ToString()} that is a size of {size} at every player's position (\"Yay! Items with sizes!\" - Galaxy119)";
        }

        private void SpawnItem(ItemType Type, float x, float y, float z, out string message)
        {
            foreach (Player Ply in Player.List)
            {
                if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                    continue;

                Pickup Item = Exiled.API.Extensions.Item.Spawn(Type, 0, Ply.Position);
                GameObject gameObject = Item.gameObject;
                gameObject.transform.localScale = new Vector3(x, y, z);
                NetworkServer.UnSpawn(gameObject);
                NetworkServer.Spawn(Item.gameObject);
            }
            message = $"Spawned in a {Type.ToString()} that is {x}x{y}x{z} at every player's position (\"Yay! Items with sizes!\" - Galaxy119)";
        }
    }
}
