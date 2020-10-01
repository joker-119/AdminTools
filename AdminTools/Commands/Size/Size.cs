using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Size
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    class Size : ParentCommand
    {
        public Size() => LoadGeneratedCommands();

        public override string Command { get; } = "size";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Sets the size of all users or a user";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.size"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage:\nsize (player id / name) or (all / *)) (x value) (y value) (z value)" +
                    "\nsize reset";
                return false;
            }

            switch (arguments.At(0))
            {
                case "reset":
                    foreach (Player Ply in Player.List)
                    {
                        if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                            continue;

                        EventHandlers.SetPlayerScale(Ply.GameObject, 1, 1, 1);
                    }

                    response = $"Everyone's size has been reset";
                    return true;
                case "*":
                case "all":
                    if (arguments.Count != 4)
                    {
                        response = "Usage: size (all / *) (x) (y) (z)";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(1), out float xval))
                    {
                        response = $"Invalid value for x size: {arguments.At(1)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(2), out float yval))
                    {
                        response = $"Invalid value for y size: {arguments.At(2)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(3), out float zval))
                    {
                        response = $"Invalid value for z size: {arguments.At(3)}";
                        return false;
                    }

                    foreach (Player Ply in Player.List)
                    {
                        if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                            continue;

                        EventHandlers.SetPlayerScale(Ply.GameObject, xval, yval, zval);
                    }

                    response = $"Everyone's scale has been set to {xval} {yval} {zval}";
                    return true;
                default:
                    if (arguments.Count != 4)
                    {
                        response = "Usage: size (player id / name) (x) (y) (z)";
                        return false;
                    }

                    Player Pl = Player.Get(arguments.At(0));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(1), out float x))
                    {
                        response = $"Invalid value for x size: {arguments.At(1)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(2), out float y))
                    {
                        response = $"Invalid value for y size: {arguments.At(2)}";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(3), out float z))
                    {
                        response = $"Invalid value for z size: {arguments.At(3)}";
                        return false;
                    }

                    EventHandlers.SetPlayerScale(Pl.GameObject, x, y, z);
                    response = $"Player {Pl.Nickname}'s scale has been set to {x} {y} {z}";
                    return true;
            }
        }
    }
}
