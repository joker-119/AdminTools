using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Dummy
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Spawns a dummy character on all users";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            int Index = 0;
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.dummy"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            Player Sender = Player.Get(((CommandSender)sender).Nickname);
            if (arguments.Count != 4)
            {
                response = "Usage: dummy (all / *) (RoleType) (x value) (y value) (z value)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(0), true, out RoleType Role))
            {
                response = $"Invalid value for role type: {arguments.At(0)}";
                return false;
            }

            if (!float.TryParse(arguments.At(1), out float xval))
            {
                response = $"Invalid x value for dummy size: {arguments.At(1)}";
                return false;
            }

            if (!float.TryParse(arguments.At(2), out float yval))
            {
                response = $"Invalid y value for dummy size: {arguments.At(2)}";
                return false;
            }

            if (!float.TryParse(arguments.At(3), out float zval))
            {
                response = $"Invalid z value for dummy size: {arguments.At(3)}";
                return false;
            }
            foreach (Player Ply in Player.List)
            {
                if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                    continue;

                EventHandlers.SpawnDummyModel(Sender, Ply.Position, Ply.GameObject.transform.localRotation, Role, xval, yval, zval, out int DummyIndex);
                Index = DummyIndex;
            }

            response = $"A {Role.ToString()} dummy has spawned on everyone, you now spawned in a total of {(Index != 1 ? $"{Index} dummies" : $"{Index} dummies")}";
            return true;
        }
    }
}
