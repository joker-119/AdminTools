using CommandSystem;
using Exiled.API.Features;
using System;

namespace AdminTools.Commands.Id
{
    public class User : ICommand
    {
        public string Command { get; } = "user";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Gets the ID of a player in the server";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (arguments.Count != 1)
            {
                response = "Usage: id user (player id / name)";
                return false;
            }

            Player Ply;
            if (String.IsNullOrWhiteSpace(arguments.At(0)))
                Ply = Player.Get(((CommandSender)sender).Nickname);
            else
            {
                Ply = Player.Get(arguments.At(0));
                if (Ply == null)
                {
                    response = "Player not found";
                    return false;
                }
            }

            response = $"{Ply.Nickname} - {Ply.UserId} - {Ply.Id}";
            return true;
        }
    }
}
