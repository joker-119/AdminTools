using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;

namespace AdminTools.Commands.SpawnRagdoll
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Spawns a specified amount of a ragdoll on a user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.dolls"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 3)
            {
                response = "Usage: spawnragdoll (player id / name) (RoleType) (amount)";
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
                response = $"This player is not a valid class to spawn a ragdoll on";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out RoleType Role))
            {
                response = $"Invalid value for role type: {arguments.At(1)}";
                return false;
            }

            if (!uint.TryParse(arguments.At(2), out uint Amount))
            {
                response = $"Invalid value for ragdoll amount: {arguments.At(2)}";
                return false;
            }

            Timing.RunCoroutine(EventHandlers.SpawnBodies(Ply, Role, (int)Amount));
            response = $"{Amount} {Role.ToString()} ragdolls have spawned on Player {Ply.Nickname}";
            return true;
        }
    }
}
