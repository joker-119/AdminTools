using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.DropItem
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class DropItem : ParentCommand
    {
        public DropItem() => LoadGeneratedCommands();

        public override string Command { get; } = "dropitem";

        public override string[] Aliases { get; } = new string[] { "drop", "dropi", "di" };

        public override string Description { get; } = "Drops a specified amount of a specified item on either all users or a user";

        public override void LoadGeneratedCommands() 
        {
            RegisterCommand(new All());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.items"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: all / *, user";
            return false;
        }
    }
}
