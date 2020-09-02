using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.InstantKill
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class InstantKill : ParentCommand
    {
        public InstantKill() => LoadGeneratedCommands();

        public override string Command { get; } = "instakill";

        public override string[] Aliases { get; } = new string[] { "ik" };

        public override string Description { get; } = "Manage instant kill properties for users";

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
            if (!((CommandSender)sender).CheckPermission("at.ik"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: give, all / *, clear, list";
            return false;
        }
    }
}
