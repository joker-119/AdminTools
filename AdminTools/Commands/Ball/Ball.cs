using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Ball
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Ball : ParentCommand
    {
        public Ball() => LoadGeneratedCommands();

        public override string Command { get; } = "ball";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Spawns a bouncy ball (SCP-018) on a user or all users";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ball"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: ball ((player id/ name) or (all / *))";
                return false;
            }

            switch (arguments.At(0)) 
            {
                case "*":
                case "all":
                    foreach (Player Pl in Player.List)
                    {
                        if (Pl.Role == RoleType.Spectator || Pl.Role == RoleType.None)
                            continue;

                        EventHandlers.SpawnBallOnPlayer(Pl);
                    }
                    Cassie.Message("pitch_1.5 xmas_bouncyballs", true, false);
                    response = "The Balls started bouncing";
                    return true;
                default:
                    Player Ply = Player.Get(arguments.At(0));
                    if (Ply == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                    {
                        response = $"You cannot spawn a ball on that player right now";
                        return false;
                    }

                    EventHandlers.SpawnBallOnPlayer(Ply);
                    response = $"The Balls started bouncing for {Ply.Nickname}";
                    return true;
            }
        }
    }
}
