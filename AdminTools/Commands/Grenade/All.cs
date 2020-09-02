using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Grenade
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Spawns a frag/flash/scp018 grenade on all users";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.grenade"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1 || arguments.Count > 2)
            {
                response = "Usage: grenade (all / *) (grenade name) (grenade time)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(0), true, out GrenadeType GType))
            {
                response = $"Invalid value for grenade name: {arguments.At(1)}";
                return false;
            }

            if (GType == GrenadeType.Scp018)
            {
                foreach (Player Ply in Player.List)
                {
                    if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                        continue;

                    EventHandlers.SpawnGrenadeOnPlayer(Ply, GType, 0);
                }
            }
            else
            {
                if (arguments.Count != 2)
                {
                    response = "Usage: grenade (all / *) (grenade name) (grenade time)";
                    return false;
                }
                if (!float.TryParse(arguments.At(1), out float Time))
                {
                    response = $"Invalid value for grenade timer: {arguments.At(2)}";
                    return false;
                }

                foreach (Player Ply in Player.List)
                {
                    if (Ply.Role == RoleType.Spectator || Ply.Role == RoleType.None)
                        continue;

                    EventHandlers.SpawnGrenadeOnPlayer(Ply, GType, Time);
                }
            }

            response = $"You spawned a {GType.ToString().ToLower()} on everyone";
            return true;
        }
    }
}
