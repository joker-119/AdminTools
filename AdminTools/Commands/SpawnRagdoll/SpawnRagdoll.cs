using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;

namespace AdminTools.Commands.SpawnRagdoll
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SpawnRagdoll : ParentCommand
    {
        public SpawnRagdoll() => LoadGeneratedCommands();

        public override string Command { get; } = "spawnragdoll";

        public override string[] Aliases { get; } = new string[] { "sr", "ragdoll", "rd", "rag", "doll" };

        public override string Description { get; } = "Spawns a specified number of ragdolls on a user";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.dolls"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand: Available ones: all / *, user";
            return false;
        }
    }
}
