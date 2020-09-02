using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.SpawnWorkbench
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SpawnWorkbench : ParentCommand
    {
        public SpawnWorkbench() => LoadGeneratedCommands();

        public override string Command { get; } = "spawnworkbench";

        public override string[] Aliases { get; } = new string[] { "sw", "wb", "workbench", "bench" };

        public override string Description { get; } = "Spawns a workbench on all users or a user";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new Clear());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.benches"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: all / *, clear, user";
            return false;
        }
    }
}
