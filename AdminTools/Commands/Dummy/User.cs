using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Dummy
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Spawns a dummy character on a user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.dummy"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 5)
            {
                response = "Usage: dummy user (player id / name) (RoleType) (x value) (y value) (z value)";
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
                response = $"This player is not a valid class to spawn a dummy on";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out RoleType Role))
            {
                response = $"Invalid value for role type: {arguments.At(1)}";
                return false;
            }

            if (!float.TryParse(arguments.At(2), out float xval))
            {
                response = $"Invalid x value for dummy size: {arguments.At(2)}";
                return false;
            }

            if (!float.TryParse(arguments.At(3), out float yval))
            {
                response = $"Invalid y value for dummy size: {arguments.At(3)}";
                return false;
            }

            if (!float.TryParse(arguments.At(4), out float zval))
            {
                response = $"Invalid z value for dummy size: {arguments.At(4)}";
                return false;
            }

            EventHandlers.SpawnDummyModel(Ply.Position, Ply.GameObject.transform.localRotation, Role, xval, yval, zval);
            response = $"A {Role.ToString()} dummy has spawned on Player {Ply.Nickname}";
            return true;
        }
    }
}
