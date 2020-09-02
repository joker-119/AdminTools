using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Inventory
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Inventory : ParentCommand
    {
        public Inventory() => LoadGeneratedCommands();

        public override string Command { get; } = "inventory";

        public override string[] Aliases { get; } = new string[] { "inv" };

        public override string Description { get; } = "Manages player inventories";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Drop());
            RegisterCommand(new See());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.inv"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: drop, see";
            return false;
        }
    }
}
