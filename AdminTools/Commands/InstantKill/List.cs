using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Text;

namespace AdminTools.Commands.InstantKill
{
    public class List : ICommand
    {
        public string Command { get; } = "list";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Lists every player who has instant killing on";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ik"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: instakill list";
                return false;
            }

            StringBuilder PlayerLister = new StringBuilder(Plugin.IkHubs.Count != 0 ? "Players with instant killing on:\n" : "No players currently online have instant killing on");
            if (Plugin.IkHubs.Count == 0)
            {
                response = PlayerLister.ToString();
                return true;
            }

            foreach (Player Ply in Plugin.IkHubs.Keys)
            {
                PlayerLister.Append(Ply.Nickname);
                PlayerLister.Append(", ");
            }

            response = PlayerLister.ToString().Substring(0, PlayerLister.ToString().Length - 2);
            return true;
        }
    }
}
