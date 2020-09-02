using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
namespace AdminTools.Commands.InstantKill
{
    public class Give : ICommand
    {
        public string Command { get; } = "give";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Gives instant kill to a user, or takes it away from them";

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
                response = "Usage: instakill give (player id / name)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (!Ply.ReferenceHub.TryGetComponent(out InstantKillComponent ikComponent))
            {
                Ply.GameObject.AddComponent<InstantKillComponent>();
                response = $"Instant killing is on for {Ply.Nickname}";
            }
            else
            {
                UnityEngine.Object.Destroy(ikComponent);
                response = $"Instant killing is off for {Ply.Nickname}";
            }
            return true;
        }
    }
}
