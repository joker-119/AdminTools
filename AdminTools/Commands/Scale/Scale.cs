using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Scale
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Scale : ParentCommand
    {
        public Scale() => LoadGeneratedCommands();

        public override string Command { get; } = "scale";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Scales all users or a user by a specified value";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new Reset());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.size"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: all / *, reset, user";
            return false;
        }
    }
}
