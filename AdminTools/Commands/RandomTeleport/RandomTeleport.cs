using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Linq;
using UnityEngine;

namespace AdminTools.Commands.RandomTeleport
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class RandomTeleport : ParentCommand
    {
        public RandomTeleport() => LoadGeneratedCommands();

        public override string Command { get; } = "randomtp";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Randomly teleports a user or all users to a random room in the facility";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!CommandProcessor.CheckPermissions(((CommandSender)sender), "randomtp", PlayerPermissions.PlayersManagement, "AdminTools", false))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: randomtp ((player id / name) or (all / *))";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    foreach (Player Ply in Player.List)
                    {
                        Room RandRoom = Map.Rooms[Plugin.NumGen.Next(0, Map.Rooms.Count())];
                        Ply.Position = RandRoom.Position + Vector3.up;
                    }

                    response = $"Everyone was teleported to a random room in the facility";
                    return true;
                default:
                    Player Pl = Player.Get(arguments.At(0));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    Room Rand = Map.Rooms[Plugin.NumGen.Next(0, Map.Rooms.Count())];
                    Pl.Position = Rand.Position + Vector3.up;

                    response = $"Player {Pl.Nickname} was teleported to {Rand.Name}";
                    return true;
            }
        }
    }
}
