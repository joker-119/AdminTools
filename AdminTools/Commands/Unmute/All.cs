using CommandSystem;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using System;

namespace AdminTools.Commands.Unmute
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Removes all mutes from everyone in the server";

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
                response = "Usage: unmute all";
                return false;
            }

            foreach (Player Ply in Player.List)
                if (!Ply.ReferenceHub.serverRoles.RemoteAdmin)
                {
                    Ply.IsIntercomMuted = false;
                    Ply.IsMuted = false;
                }

            response = "Everyone from the server who is not a staff can now speak freely";
            return true;
        }
    }
}
