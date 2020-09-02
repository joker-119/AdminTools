using CommandSystem;
using Exiled.API.Features;
using System;
using System.Linq;
using System.Text;

namespace AdminTools.Commands.Id
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Gets the ID of every player in the server";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (arguments.Count != 0)
            {
                response = "Usage: id all / *";
                return false;
            }

            StringBuilder Builder = new StringBuilder();
            if (Player.List.Count() == 0)
            {
                Builder.AppendLine("There are no players currently online in the server");
                response = Builder.ToString();
                return true;
            }
            else
            {
                Builder.AppendLine("List of ID's on the server:");
                foreach (Exiled.API.Features.Player Ply in Exiled.API.Features.Player.List)
                {
                    Builder.Append(Ply.Nickname);
                    Builder.Append(" - ");
                    Builder.Append(Ply.UserId);
                    Builder.Append(" - ");
                    Builder.AppendLine(Ply.Id.ToString());
                }
                response = Builder.ToString();
                return true;
            }
        }
    }
}
