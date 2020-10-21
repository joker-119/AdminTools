using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Linq;

namespace AdminTools.Commands.Ghost
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class TargetGhost : ParentCommand
    {
        public const string HELP_STR = "Usage: targetghost (player id / name) (player id / name)...";

        public TargetGhost() => LoadGeneratedCommands();

        public override string Command { get; } = "targetghost";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Sets a user to be invisible to another user";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.targetghost"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = HELP_STR;
                return false;
            }

            if (!GetPlayer(arguments.At(0), out var sourcePlayer))
            {
                response = "Invalid source player";
                return false;
            }

            foreach (var arg in arguments.Skip(1))
            {
                if (!GetPlayer(arg, out var victim))
                    continue;

                // Just remove if it's already in
                if (!sourcePlayer.TargetGhostsHashSet.Add(victim.Id))
                    sourcePlayer.TargetGhostsHashSet.Remove(victim.Id);
            }

            response = "Done";
            return true;
        }

        private bool GetPlayer(string str, out Player player)
        {
            player = Player.Get(str);
            return player != null;
        }
    }
}
