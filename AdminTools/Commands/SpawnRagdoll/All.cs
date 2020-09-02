using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;

namespace AdminTools.Commands.SpawnRagdoll
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Spawns a specified amount of a ragdoll on all users";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.dolls"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: spawnragdoll (all / *) (RoleType) (amount)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(0), true, out RoleType Role))
            {
                response = $"Invalid value for role type: {arguments.At(0)}";
                return false;
            }

            if (!uint.TryParse(arguments.At(1), out uint Amount))
            {
                response = $"Invalid value for ragdoll amount: {arguments.At(1)}";
                return false;
            }

            foreach (Player Ply in Player.List)
            {
                if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                    continue;

                Timing.RunCoroutine(EventHandlers.SpawnBodies(Ply, Role, (int)Amount));
            }

            response = $"{Amount} {Role.ToString()} ragdolls have spawned on everyone";
            return true;
        }
    }
}
