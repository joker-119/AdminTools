using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.TeleportX
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class TeleportX : ParentCommand
    {
        public TeleportX() => LoadGeneratedCommands();

        public override string Command { get; } = "teleportx";

        public override string[] Aliases { get; } = new string[] { "tpx" };

        public override string Description { get; } = "Teleports all users or a user to another user";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new All());
            RegisterCommand(new User());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.tp"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            response = "Invalid subcommand: Available ones: all / *, user";
            return false;
        }
    }
}
