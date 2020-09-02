using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Grenade
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Spawns a frag/flash/scp018 grenade on a user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.grenade"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 2 || arguments.Count > 3)
            {
                response = "Usage: grenade (player id / name) (grenade name) (grenade time)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }
            else if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
            {
                response = $"Player {Ply.Nickname} is not a valid class to spawn a grenade on";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out GrenadeType GType))
            {
                response = $"Invalid value for grenade name: {arguments.At(1)}";
                return false;
            }

            if (GType == GrenadeType.Scp018)
                EventHandlers.SpawnGrenadeOnPlayer(Ply, GType, 0);
            else
            {
                if (arguments.Count != 3)
                {
                    response = "Usage: grenade (player id / name) (grenade name) (grenade time)";
                    return false;
                }
                if (!float.TryParse(arguments.At(2), out float Time))
                {
                    response = $"Invalid value for grenade timer: {arguments.At(2)}";
                    return false;
                }
                EventHandlers.SpawnGrenadeOnPlayer(Ply, GType, Time);
            }

            response = $"You spawned a {GType.ToString().ToLower()} on {Ply.Nickname}";
            return true;
        }
    }
}
