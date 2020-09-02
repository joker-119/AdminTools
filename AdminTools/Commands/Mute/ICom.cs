using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Mute
{
    public class ICom : ICommand
    {
        public string Command { get; } = "icom";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Intercom mutes everyone in the server";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.mute"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "Usage: mute icom";
                return false;
            }

            foreach (Player player in Player.List)
                if (!player.ReferenceHub.serverRoles.RemoteAdmin)
                    player.IsIntercomMuted = true;

            response = "Everyone from the server who is not a staff has been intercom muted";
            return true;
        }
    }
}
