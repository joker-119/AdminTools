using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using MEC;

namespace AdminTools.Commands.Rocket
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Sends a player high in the sky and explodes them";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
			EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
			if (!((CommandSender)sender).CheckPermission("at.rocket"))
			{
				response ="You do not have permission to use this command";
				return false;
			}

			if (arguments.Count != 2)
			{
				response = "Usage: rocket user (id / name) (speed)";
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
				response = $"Player {Ply.Nickname} is not a valid class to rocket";
				return false;
			}

			if (!float.TryParse(arguments.At(1), out float speed) && speed <= 0)
			{
				response = $"Speed argument invalid: {arguments.At(1)}";
				return false;
			}

			Timing.RunCoroutine(EventHandlers.DoRocket(Ply, speed));
			response = $"Player {Ply.Nickname} has been rocketed into the sky (We're going on a trip, in our favorite rocketship)";
			return true;
		}
    }
}
