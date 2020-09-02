using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.SendMessage
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SendMessage : ParentCommand
    {
        public SendMessage() => LoadGeneratedCommands();

        public override string Command { get; } = "sendmessage";

        public override string[] Aliases { get; } = new string[] { "sm" };

        public override string Description { get; } = "Broadcasts a message to either a user or a group";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Group());
            RegisterCommand(new Staff());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.sm"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: group, user";
            return false;
        }
    }
}
