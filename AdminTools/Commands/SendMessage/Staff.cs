using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.SendMessage
{
	public class Staff : ICommand
	{
		public string Command { get; } = "staff";

		public string[] Aliases { get; } = new string[] { "admin" };

		public string Description { get; } = "Broadcasts a message to all staff online";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
			if (!((CommandSender)sender).CheckPermission("at.sm"))
			{
				response = "You do not have permission to use this command";
				return false;
			}

			if (arguments.Count < 3)
			{
				response = "Usage: sendmessage staff (time) (message)";
				return false;
			}

			if (!ushort.TryParse(arguments.At(0), out ushort time) && time <= 0)
			{
				response = $"Invalid value for duration: {arguments.At(0)}";
				return false;
			}

			foreach (Player Ply in Player.List)
			{
				if (Ply.ReferenceHub.serverRoles.RemoteAdmin)
					Ply.Broadcast(time, EventHandlers.FormatArguments(arguments, 1), Broadcast.BroadcastFlags.AdminChat);
			}

			response = $"Message sent to all currently online staff";
			return true;
		}
	}
}
