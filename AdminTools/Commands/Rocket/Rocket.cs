using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;

namespace AdminTools.Commands.Rocket
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Rocket : ParentCommand
    {
        public Rocket() => LoadGeneratedCommands();

        public override string Command { get; } = "rocket";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Sends players high in the sky and explodes them";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.rocket"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: rocket ((player id / name) or (all / *)) (speed)";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    if (!float.TryParse(arguments.At(1), out float speed) && speed <= 0)
                    {
                        response = $"Speed argument invalid: {arguments.At(1)}";
                        return false;
                    }

                    foreach (Player Ply in Player.List)
                        Timing.RunCoroutine(EventHandlers.DoRocket(Ply, speed));

                    response = "Everyone has been rocketed into the sky (We're going on a trip, in our favorite rocketship)";
                    return true;
                default:
                    Player Pl = Player.Get(arguments.At(0));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }
                    else if (Pl.Role == RoleType.Spectator || Pl.Role == RoleType.None)
                    {
                        response = $"Player {Pl.Nickname} is not a valid class to rocket";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(1), out float spd) && spd <= 0)
                    {
                        response = $"Speed argument invalid: {arguments.At(1)}";
                        return false;
                    }

                    Timing.RunCoroutine(EventHandlers.DoRocket(Pl, spd));
                    response = $"Player {Pl.Nickname} has been rocketed into the sky (We're going on a trip, in our favorite rocketship)";
                    return true;
            }
        }
    }
}
