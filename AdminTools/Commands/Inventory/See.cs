using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Text;

namespace AdminTools.Commands.Inventory
{
    public class See : ICommand
    {
        public string Command { get; } = "see";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Sees the inventory items a user has";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.inv"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: inventory see (player id / name)";
                return true;
            }

            Player Ply = Player.Get(arguments.At(0));
            if (Ply == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }

            StringBuilder InvBuilder = new StringBuilder();
            if (Ply.Inventory.items.Count != 0)
            {
                InvBuilder.Append("Player ");
                InvBuilder.Append(Ply.Nickname);
                InvBuilder.AppendLine(" has the following items in their inventory:");
                foreach (global::Inventory.SyncItemInfo Item in Ply.Inventory.items)
                {
                    InvBuilder.Append("- ");
                    InvBuilder.AppendLine(Item.id.ToString());
                }
            }
            else
            {
                InvBuilder.Append("Player ");
                InvBuilder.Append(Ply.Nickname);
                InvBuilder.Append(" does not have any items in their inventory");
            }
            response = InvBuilder.ToString();
            return true;
        }
    }
}
