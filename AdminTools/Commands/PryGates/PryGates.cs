using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Text;

namespace AdminTools.Commands.PryGates
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class PryGates : ParentCommand
    {
        public PryGates() => LoadGeneratedCommands();

        public override string Command { get; } = "prygate";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Gives the ability to pry gates to players, clear the ability from players, and shows who has the ability";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.prygate"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage:\nprygate ((player id / name) or (all / *))" +
                    "\nprygate clear" +
                    "\nprygate list" +
                    "\nprygate remove (player id / name)";
                return false;
            }

            switch (arguments.At(0))
            {
                case "clear":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: prygates clear";
                        return false;
                    }

                    Plugin.PryGateHubs.Clear();
                    response = "The ability to pry gates is cleared from all players now";
                    return true;
                case "list":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: prygates list";
                        return false;
                    }

                    StringBuilder PlayerLister = new StringBuilder(Plugin.PryGateHubs.Count != 0 ? "Players with the ability to pry gates:\n" : "No players currently online have the ability to pry gates");
                    if (Plugin.PryGateHubs.Count > 0)
                    {
                        foreach (Player Ply in Plugin.PryGateHubs)
                            PlayerLister.Append(Ply.Nickname + ", ");

                        int length = PlayerLister.ToString().Length;
                        response = PlayerLister.ToString().Substring(0, length - 2);
                        PlayerLister.Clear();
                        return true;
                    }
                    else
                    {
                        response = "There are no players currently online that can pry gates";
                        PlayerLister.Clear();
                        return true;
                    }
                case "remove":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: prygate remove (player id / name)";
                        return false;
                    }

                    Player Plyr = Player.Get(arguments.At(1));
                    if (Plyr == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    if (Plugin.PryGateHubs.Contains(Plyr))
                    {
                        Plugin.PryGateHubs.Remove(Plyr);
                        response = $"Player \"{Plyr.Nickname}\" cannot pry gates open now";
                    }
                    else
                        response = $"Player {Plyr.Nickname} does not have the ability to pry gates open";
                    return true;
                case "*":
                case "all":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: prygates (all / *)";
                        return false;
                    }

                    foreach (Player Ply in Player.List)
                    {
                        if (!Plugin.PryGateHubs.Contains(Ply))
                            Plugin.PryGateHubs.Add(Ply);
                    }

                    response = "The ability to pry gates open is on for all players now";
                    return true;
                default:
                    if (arguments.Count != 1)
                    {
                        response = "Usage: prygate (player id / name)";
                        return false;
                    }

                    Player Pl = Player.Get(arguments.At(0));
                    if (Pl == null)
                    {
                        response = $"Player \"{arguments.At(0)}\" not found";
                        return false;
                    }

                    if (!Plugin.PryGateHubs.Contains(Pl))
                    {
                        Plugin.PryGateHubs.Add(Pl);
                        response = $"Player \"{Pl.Nickname}\" can now pry gates open";
                        return true;
                    }
                    else
                    {
                        Plugin.PryGateHubs.Remove(Pl);
                        response = $"Player \"{Pl.Nickname}\" cannot pry gates open now";
                        return true;
                    }
            }
        }
    }
}
