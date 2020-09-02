using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.TeleportX
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Teleports a users to another user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.tp"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: tpx user (player id / name (being teleported)) (player id / name (receiving teleported player))";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            Player Plyr = Player.Get(arguments.At(1));
            if (Plyr == null)
            {
                response = $"Player not found: {arguments.At(1)}";
                return false;
            }

            Ply.Position = Plyr.Position;
            response = $"Player {Ply.Nickname} has been teleported to Player {Plyr.Nickname}";
            return true;
        }
    }
}
