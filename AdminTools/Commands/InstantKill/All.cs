using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.InstantKill
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Gives instant killing to every player on the server";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.ik"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: instakill all / *";
                return false;
            }

            foreach (Player Ply in Player.List)
                if (!Ply.ReferenceHub.TryGetComponent(out InstantKillComponent _))
                    Ply.ReferenceHub.gameObject.AddComponent<InstantKillComponent>();

            response = "Everyone on the server can instantly kill other users now";
            return true;
        }
    }
}
