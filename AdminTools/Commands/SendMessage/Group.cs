using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.SendMessage
{
	public class Group : ICommand
	{
		public string Command { get; } = "group";

		public string[] Aliases { get; } = new string[] { };

		public string Description { get; } = "Sends a broadcast to everyone in a specific group";

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
				response = "Usage: sendmessage group (group) (time) (message)";
				return false;
			}

			UserGroup BroadcastGroup = ServerStatic.PermissionsHandler.GetGroup(arguments.At(0));
			if (BroadcastGroup == null)
			{
				response = $"Invalid group: {arguments.At(0)}";
				return false;
			}

			if (!ushort.TryParse(arguments.At(1), out ushort time) && time <= 0)
			{
				response = $"Invalid value for duration: {arguments.At(1)}";
				return false;
			}

			foreach (Player player in Player.List)
			{
				if (player.Group.BadgeText.Equals(BroadcastGroup.BadgeText))
					player.Broadcast(time, EventHandlers.FormatArguments(arguments, 2));
			}

			response = $"Message sent to all members of \"{arguments.At(0)}\"";
			return true;
		}
	}
}
