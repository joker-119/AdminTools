using CommandSystem;
using RemoteAdmin;
using System;

namespace AdminTools.Commands.Strip
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Strip : ParentCommand
    {
        public Strip() => LoadGeneratedCommands();

        public override string Command { get; } = "strip";

        public override string[] Aliases { get; } = new string[] { "stp" };

        public override string Description { get; } = "Clears a user or users inventories instantly";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!CommandProcessor.CheckPermissions(((CommandSender)sender), "strip", PlayerPermissions.PlayersManagement, "AdminTools", false))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: all / *, user";
            return false;
        }
    }
}
