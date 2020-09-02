using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Grenade
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Grenade : ParentCommand
    {
        public Grenade() => LoadGeneratedCommands();

        public override string Command { get; } = "grenade";

        public override string[] Aliases { get; } = new string[] { "gn" };

        public override string Description { get; } = "Spawns a frag/flash/scp018 grenade on a user or users";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.grenade"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand. Available ones: all, user";
            return false;
        }
    }
}
