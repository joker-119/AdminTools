using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Text;

namespace AdminTools.Commands.Regeneration
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Regeneration : ParentCommand
    {
        public Regeneration() => LoadGeneratedCommands();

        public override string Command { get; } = "reg";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Gives regeneration to players, clears regeneration from players, and shows who has regeneration";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!((CommandSender)sender).CheckPermission("at.reg"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage:\nreg ((player id / name) or (all / *)) ((doors) or (all))" +
                    "\nreg clear" +
                    "\nreg list" +
                    "\nreg health (value)" +
                    "\nreg time (value)";
                return false;
            }

            switch (arguments.At(0))
            {
                case "clear":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: reg clear";
                        return false;
                    }

                    foreach (Player Ply in Plugin.RgnHubs.Keys)
                        if (Ply.ReferenceHub.TryGetComponent(out RegenerationComponent RgCom))
                            UnityEngine.Object.Destroy(RgCom);

                    response = "Regeneration has been removed from everyone";
                    return true;
                case "list":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: regen list";
                        return false;
                    }

                    StringBuilder PlayerLister = new StringBuilder(Plugin.RgnHubs.Count != 0 ? "Players with regeneration on:\n" : "No players currently online have regeneration on");
                    if (Plugin.RgnHubs.Count == 0)
                    {
                        response = PlayerLister.ToString();
                        return true;
                    }

                    foreach (Player Ply in Plugin.RgnHubs.Keys)
                    {
                        PlayerLister.Append(Ply.Nickname);
                        PlayerLister.Append(", ");
                    }

                    response = PlayerLister.ToString().Substring(0, PlayerLister.ToString().Length - 2);
                    return true;
                case "heal":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: reg heal (value)";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(1), out float healvalue) || healvalue < 0.05)
                    {
                        response = $"Invalid value for healing: {arguments.At(1)}";
                        return false;
                    }

                    Plugin.HealthGain = healvalue;
                    response = $"Players with regeneration will heal {healvalue} HP per interval";
                    return true;
                case "time":
                    if (arguments.Count != 2)
                    {
                        response = "Usage: reg time (value)";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(1), out float healtime) || healtime < 0.05)
                    {
                        response = $"Invalid value for healing time interval: {arguments.At(1)}";
                        return false;
                    }

                    Plugin.HealthInterval = healtime;
                    response = $"Players with regeneration will heal every {healtime} seconds";
                    return true;
                case "*":
                case "all":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: reg (all / *)";
                        return false;
                    }

                    foreach (Player Ply in Player.List)
                        if (!Ply.ReferenceHub.TryGetComponent(out RegenerationComponent _))
                            Ply.ReferenceHub.gameObject.AddComponent<RegenerationComponent>();

                    response = "Everyone on the server can regenerate health now";
                    return true;
                default:
                    if (arguments.Count != 1)
                    {
                        response = "Usage: reg (player id / name)";
                        return false;
                    }

                    Player Pl = Player.Get(arguments.At(0));
                    if (Pl == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (!Pl.ReferenceHub.TryGetComponent(out RegenerationComponent rgnComponent))
                    {
                        Pl.GameObject.AddComponent<RegenerationComponent>();
                        response = $"Regeneration is on for {Pl.Nickname}";
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(rgnComponent);
                        response = $"Regeneration is off for {Pl.Nickname}";
                    }
                    return true;
            }
        }
    }
}