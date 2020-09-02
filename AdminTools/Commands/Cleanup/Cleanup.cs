using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Cleanup
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Cleanup : ParentCommand
    {
        public Cleanup() => LoadGeneratedCommands();

        public override string Command { get; } = "cleanup";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Cleans up items and ragdolls from the server";

        public override void LoadGeneratedCommands() 
        {
            RegisterCommand(new Items());
            RegisterCommand(new Ragdolls());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.cleanup"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: items, ragdolls";
            return false;
        }
    }
}
