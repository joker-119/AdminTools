using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using System;
using UnityEngine;

namespace AdminTools.Commands.DropSize
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Drops a selected amount of a selected item on a user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.items"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 3)
            {
                response = "\nUsage:\ndropsize user (player id / name) (ItemType) (size) \ndropsize user (player id / name) (ItemType) (x size) (y size) (z size)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }
            else if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
            {
                response = $"Player {Ply.Nickname} is not a valid class to drop items to";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out ItemType Type))
            {
                response = $"Invalid value for item name: {arguments.At(1)}";
                return false;
            }

            switch (arguments.Count)
            {
                case 3:
                    if (!float.TryParse(arguments.At(2), out float size))
                    {
                        response = $"Invalid value for item scale: {arguments.At(2)}";
                        return false;
                    }
                    SpawnItem(Ply, Type, size, out string msg);
                    response = msg;
                    return true;
                case 5:
                    if (!float.TryParse(arguments.At(2), out float xval))
                    {
                        response = $"Invalid value for item scale: {arguments.At(2)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(3), out float yval))
                    {
                        response = $"Invalid value for item scale: {arguments.At(3)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(4), out float zval))
                    {
                        response = $"Invalid value for item scale: {arguments.At(4)}";
                        return false;
                    }

                    SpawnItem(Ply, Type, xval, yval, zval, out string message);
                    response = message;
                    return true;
                default:
                    response = "\nUsage:\ndropsize user (player id / name) (ItemType) (size) \ndropsize user (player id / name) (ItemType) (x size) (y size) (z size)";
                    return false;
            }
        }

        private void SpawnItem(Player Ply, ItemType Type, float size, out string message)
        {
            Pickup Item = Exiled.API.Extensions.Item.Spawn(Type, 0, Ply.Position);
            GameObject gameObject = Item.gameObject;
            gameObject.transform.localScale = Vector3.one * size;
            NetworkServer.UnSpawn(gameObject);
            NetworkServer.Spawn(Item.gameObject);
            message = $"Spawned in a {Type.ToString()} that is a size of {size} at {Ply.Nickname}'s position (\"Yay! Items with sizes!\" - Galaxy119)";
        }

        private void SpawnItem(Player Ply, ItemType Type, float x, float y, float z, out string message)
        {
            Pickup Item = Exiled.API.Extensions.Item.Spawn(Type, 0, Ply.Position);
            GameObject gameObject = Item.gameObject;
            gameObject.transform.localScale = new Vector3(x, y, z);
            NetworkServer.UnSpawn(gameObject);
            NetworkServer.Spawn(Item.gameObject);
            message = $"Spawned in a {Type.ToString()} that is {x}x{y}x{z} at {Ply.Nickname}'s position (\"Yay! Items with sizes!\" - Galaxy119)";
        }
    }
}
