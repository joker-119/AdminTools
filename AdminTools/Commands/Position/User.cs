using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdminTools.Commands.Position
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Modifies or retrieves the position of a specified user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.tp"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "\nUsage:\nposition user (player id / name) (set) (x position) (y position) (z position)\nposition user (player id / name) (get)\nposition user (player id / name) (add) (x, y, or z) (value)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out PositionModifier Mod))
            {
                response = $"Invalid position modifier: {arguments.At(1)}";
                return false;
            }

            switch (Mod)
            {
                case PositionModifier.Set:
                    if (arguments.Count != 5)
                    {
                        response = "Usage: position user (player id / name) (set) (x position) (y position) (z position)";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(2), out float xval))
                    {
                        response = $"Invalid value for x position: {arguments.At(2)}";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(3), out float yval))
                    {
                        response = $"Invalid value for x position: {arguments.At(3)}";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(4), out float zval))
                    {
                        response = $"Invalid value for x position: {arguments.At(4)}";
                        return false;
                    }

                    Ply.Position = new Vector3(xval, yval, zval);
                    response = $"Player {Ply.Nickname}'s positions have been set to {xval} {yval} {zval}";
                    return true;
                case PositionModifier.Get:
                    if (arguments.Count != 2)
                    {
                        response = "Usage: position user (player id / name) (get)";
                        return false;
                    }

                    response = $"Player {Ply.Nickname}'s ({Ply.Id}) position is {Ply.Position.x} {Ply.Position.y} {Ply.Position.z}";
                    return true;
                case PositionModifier.Add:
                    if (arguments.Count != 4)
                    {
                        response = "Usage: position user (player id / name) (add) (x, y, or z) (value)";
                        return false;
                    }
                    if (!Enum.TryParse(arguments.At(2), true, out VectorAxis Axis))
                    {
                        response = $"Invalid value for vector axis: {arguments.At(2)}";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(3), out float val))
                    {
                        response = $"Invalid value for position: {arguments.At(2)}";
                        return false;
                    }
                    switch (Axis)
                    {
                        case VectorAxis.X:
                            Ply.Position = new Vector3(Ply.Position.x + val, Ply.Position.y, Ply.Position.z);
                            response = $"Player {Ply.Nickname}'s x position has been added by {val}";
                            return true;
                        case VectorAxis.Y:
                            Ply.Position = new Vector3(Ply.Position.x, Ply.Position.y + val, Ply.Position.z);
                            response = $"Player {Ply.Nickname}'s y position has been added by {val}";
                            return true;
                        case VectorAxis.Z:
                            Ply.Position = new Vector3(Ply.Position.x, Ply.Position.y, Ply.Position.z + val);
                            response = $"Player {Ply.Nickname}'s z position has been added by {val}";
                            return true;
                    }
                    break;
                default:
                    response = "\nUsage:\nposition (all / *) (set) (x position) (y position) (z position)\nposition (all / *) (get)\nposition (all / *) (add) (x, y, or z) (value)";
                    return false;
            }

            response = "Something did not go right, the command should not reach this point";
            return false;
        }
    }
}
