using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.Inventory
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Drop : ParentCommand
    {
        public Drop() => LoadGeneratedCommands();

        public override string Command { get; } = "drop";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Drops the items in a players inventory";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.inv"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: inventory drop ((player id / name) or (all / *))";
                return false;
            }

            switch (arguments.At(0))
            {
                case "*":
                case "all":
                    foreach (Player Ply in Player.List)
                        Ply.Inventory.ServerDropAll();

                    response = "All items from everyones inventories has been dropped";
                    return true;
                default:
                    Player Pl = Player.Get(arguments.At(0));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    Pl.Inventory.ServerDropAll();
                    response = $"All items from {Pl.Nickname}'s inventory has been dropped";
                    return true;
            }
        }
    }
}