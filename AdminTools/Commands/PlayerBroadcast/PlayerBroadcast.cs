using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;

namespace AdminTools.Commands.PlayerBroadcast
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class PlayerBroadcast : ParentCommand
    {
        public PlayerBroadcast() => LoadGeneratedCommands();

        public override string Command { get; } = "playerbroadcast";

        public override string[] Aliases { get; } = new string[] { "pbc" };

        public override string Description { get; } = "Sends a message to all currently online staff on the server";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!CommandProcessor.CheckPermissions(((CommandSender)sender), "broadcast", PlayerPermissions.Broadcasting, "AdminTools", false))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: playerbroadcast (player id / name) (time) (message)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (!ushort.TryParse(arguments.At(1), out ushort time) && time <= 0)
            {
                response = $"Invalid value for duration: {arguments.At(1)}";
                return false;
            }

            Ply.Broadcast(time, EventHandlers.FormatArguments(arguments, 2));
            response = $"Message sent to {Ply.Nickname}";
            return true;
        }
    }
}
