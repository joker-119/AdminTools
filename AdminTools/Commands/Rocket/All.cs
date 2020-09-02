using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using System;

namespace AdminTools.Commands.Rocket
{
    public class All : ICommand
    {
		public string Command { get; } = "all";

		public string[] Aliases { get; } = new string[] { "*" };

		public string Description { get; } = "Sends all players high in the sky and explodes them";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
			if (!((CommandSender)sender).CheckPermission("at.rocket"))
			{
				response = "You do not have permission to use this command";
				return false;
			}

			if (arguments.Count != 1)
			{
				response = "Usage: rocket all / * (speed)";
				return false;
			}

			if (!float.TryParse(arguments.At(0), out float speed) && speed <= 0)
			{
				response = $"Speed argument invalid: {arguments.At(0)}";
				return false;
			}

			foreach (Player Ply in Player.List)
				Timing.RunCoroutine(EventHandlers.DoRocket(Ply, speed));

			response = "Everyone has been rocketed into the sky (We're going on a trip, in our favorite rocketship)";
			return true;
		}
	}
}
