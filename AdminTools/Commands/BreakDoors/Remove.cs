using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.BreakDoors
{
    public class Remove : ICommand
    {
        public string Command { get; } = "remove";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Removes the ability to break doors/gates from a specific user";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.bd"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: breakdoors remove (player id / name)";
                return false;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            if (Ply.ReferenceHub.TryGetComponent(out BreakDoorComponent BdComponent))
            {
                Plugin.BdHubs.Remove(Ply);
                UnityEngine.Object.Destroy(BdComponent);
                response = $"Breaking doors is off for {Ply.Nickname}";
            }
            else
                response = $"Player {Ply.Nickname} does not have the ability to break doors";
            return true;
        }
    }
}
