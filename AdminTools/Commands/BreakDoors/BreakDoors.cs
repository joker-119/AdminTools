using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.BreakDoors
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class BreakDoors : ParentCommand
    {
        public BreakDoors() => LoadGeneratedCommands();

        public override string Command { get; } = "breakdoors";

        public override string[] Aliases { get; } = new string[] { "bd" };

        public override string Description { get; } = "Manage breaking door/gate properties for players";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new Clear());
            RegisterCommand(new Give());
            RegisterCommand(new List());
            RegisterCommand(new Remove());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.bd"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: all / *, clear, give, list, remove";
            return false;
        }
    }
}
