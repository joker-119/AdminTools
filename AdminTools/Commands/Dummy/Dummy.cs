using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Dummy
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Dummy : ParentCommand
    {
        public Dummy() => LoadGeneratedCommands();

        public override string Command { get; } = "dummy";

        public override string[] Aliases { get; } = new string[] { "dum" };

        public override string Description { get; } = "Spawns a dummy character on all users on a user";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.dummy"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand: Available ones: all / *, user";
            return false;
        }
    }
}
