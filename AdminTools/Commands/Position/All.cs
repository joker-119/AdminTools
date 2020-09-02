using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdminTools.Commands.Position
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" } ;

        public string Description { get; } = "Modifies or retrieves the position of all user";

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
                response = "\nUsage:\nposition (all / *) (set) (x position) (y position) (z position)\nposition (all / *) (get)\nposition (all / *) (add) (x, y, or z) (value)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(0), true, out PositionModifier Mod))
            {
                response = $"Invalid position modifier: {arguments.At(0)}";
                return false;
            }

            switch (Mod)
            {
                case PositionModifier.Set:
                    if (arguments.Count != 4)
                    {
                        response = "Usage: position (all / *) (set) (x position) (y position) (z position)";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(1), out float xval))
                    {
                        response = $"Invalid value for x position: {arguments.At(1)}";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(2), out float yval))
                    {
                        response = $"Invalid value for x position: {arguments.At(2)}";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(3), out float zval))
                    {
                        response = $"Invalid value for x position: {arguments.At(3)}";
                        return false;
                    }
                    if (Player.List.Count() == 0)
                    {
                        response = "There are no players currently online";
                        return true;
                    }
                    foreach (Player Ply in Player.List)
                    {
                        Ply.Position = new Vector3(xval, yval, zval);
                    }
                    response = $"All player's positions have been set to {xval} {yval} {zval}";
                    return true;
                case PositionModifier.Get:
                    if (arguments.Count != 1)
                    {
                        response = "Usage: position (all / *) (get)";
                        return false;
                    }
                    StringBuilder PositionBuilder = new StringBuilder();
                    if (Player.List.Count() == 0)
                    {
                        response = "There are no players currently online";
                        return true;
                    }
                    PositionBuilder.Append("\n");
                    foreach (Player Ply in Player.List)
                    {
                        PositionBuilder.Append(Ply.Nickname);
                        PositionBuilder.Append("'s (");
                        PositionBuilder.Append(Ply.Id);
                        PositionBuilder.Append(")");
                        PositionBuilder.Append(" position: ");
                        PositionBuilder.Append(Ply.Position.x);
                        PositionBuilder.Append(" ");
                        PositionBuilder.Append(Ply.Position.y);
                        PositionBuilder.Append(" ");
                        PositionBuilder.AppendLine(Ply.Position.z.ToString());
                    }
                    string Message = PositionBuilder.ToString();
                    PositionBuilder.Clear();
                    response = Message;
                    return true;
                case PositionModifier.Add:
                    if (arguments.Count != 3)
                    {
                        response = "Usage: position (all / *) (add) (x, y, or z) (value)";
                        return false;
                    }
                    if (!Enum.TryParse(arguments.At(1), true, out VectorAxis Axis))
                    {
                        response = $"Invalid value for vector axis: {arguments.At(1)}";
                        return false;
                    }
                    if (!float.TryParse(arguments.At(2), out float val))
                    {
                        response = $"Invalid value for position: {arguments.At(2)}";
                        return false;
                    }
                    switch (Axis)
                    {
                        case VectorAxis.X:
                            foreach (Player Ply in Player.List)
                                Ply.Position = new Vector3(Ply.Position.x + val, Ply.Position.y, Ply.Position.z);

                            response = $"Every player's x position has been added by {val}";
                            return true;
                        case VectorAxis.Y:
                            foreach (Player Ply in Player.List)
                                Ply.Position = new Vector3(Ply.Position.x, Ply.Position.y + val, Ply.Position.z);

                            response = $"Every player's y position has been added by {val}";
                            return true;
                        case VectorAxis.Z:
                            foreach (Player Ply in Player.List)
                                Ply.Position = new Vector3(Ply.Position.x, Ply.Position.y, Ply.Position.z + val);

                            response = $"Every player's z position has been added by {val}";
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
