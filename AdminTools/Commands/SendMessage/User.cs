using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.SendMessage
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Broadcasts a message to a player";

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
				response = "Usage: sendmessage user (id / name) (time) (message)";
				return false;
			}

			Player Ply = Player.Get(arguments.At(0));
			if (Ply == null)
			{
				response = $"Player not found: {arguments.At(0)}";
				return false;
			}

			if (!ushort.TryParse(arguments.At(1), out ushort time) && time <= 0)
			{
				response = $"Invalid value for duration: {arguments.At(1)}";
				return false;
			}

			Ply.Broadcast(time, EventHandlers.FormatArguments(arguments, 2));
			response = $"Message sent to {Ply.Nickname}";
			return true;
		}
    }
}
