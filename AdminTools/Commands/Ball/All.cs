using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Ball
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Spawns a bouncy ball (SCP-018) on all users";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ball"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: ball all / *";
                return false;
            }

            foreach (Player Ply in Player.List)
            {
                if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                    continue;

                EventHandlers.SpawnBallOnPlayer(Ply);
            }
            Cassie.Message("pitch_1.5 xmas_bouncyballs", true, false);
            response = "The Balls started bouncing";
            return true;
        }
    }
}
