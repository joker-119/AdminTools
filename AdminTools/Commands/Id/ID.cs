using CommandSystem;
using System;

namespace AdminTools.Commands.Id
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class ID : ParentCommand
    {
        public ID() => LoadGeneratedCommands();

        public override string Command { get; } = "id";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Gets the player ID of a selected user";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            response = "Invalid subcommand. Available ones: all / *, user";
            return false;
        }
    }
}
