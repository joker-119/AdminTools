using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdminTools.Commands.HintBroadcast
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class HintBroadcast : ParentCommand
    {
        public HintBroadcast() => LoadGeneratedCommands();

        public override string Command { get; } = "hbc";

        public override string[] Aliases { get; } = new string[] { "broadcasthint" };

        public override string Description { get; } = "Broadcasts a message to either a user, a group, a role, all staff, or everyone";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            if (!CommandProcessor.CheckPermissions(((CommandSender)sender), "hints", PlayerPermissions.Broadcasting, "AdminTools", false))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage:\nhint (time) (message)" +
                    "\nhbc user (player id / name) (time) (message)" +
                    "\nhbc users (player id / name group (i.e.: 1,2,3 or hello,there,hehe)) (time) (message)" +
                    "\nhbc group (group name) (time) (message)" +
                    "\nhbc groups (list of groups (i.e.: owner,admin,moderator)) (time) (message)" +
                    "\nhbc role (RoleType) (time) (message)" +
                    "\nhbc roles (RoleType group (i.e.: ClassD,Scientist,NtfCadet)) (time) (message)" +
                    "\nhbc (random / someone) (time) (message)" +
                    "\nhbc (staff / admin) (time) (message)" +
                    "\nhbc clearall";
                return false;
            }

            switch (arguments.At(0))
            {
                case "user":
                    if (arguments.Count < 4)
                    {
                        response = "Usage: hbc user (player id / name) (time) (message)";
                        return false;
                    }

                    Player Ply = Player.Get(arguments.At(1));
                    if (Ply == null)
                    {
                        response = $"Player not found: {arguments.At(1)}";
                        return false;
                    }

                    if (!ushort.TryParse(arguments.At(2), out ushort time) && time <= 0)
                    {
                        response = $"Invalid value for duration: {arguments.At(2)}";
                        return false;
                    }

                    Ply.ShowHint(EventHandlers.FormatArguments(arguments, 3), time);
                    response = $"Hint sent to {Ply.Nickname}";
                    return true;
                case "users":
                    if (arguments.Count < 4)
                    {
                        response = "Usage: hbc users (player id / name group (i.e.: 1,2,3 or hello,there,hehe)) (time) (message)";
                        return false;
                    }

                    string[] Users = arguments.At(1).Split(',');
                    List<Player> PlyList = new List<Player>();
                    foreach (string s in Users)
                    {
                        if (int.TryParse(s, out int id) && Player.Get(id) != null)
                            PlyList.Add(Player.Get(id));
                        else if (Player.Get(s) != null)
                            PlyList.Add(Player.Get(s));
                    }

                    if (!ushort.TryParse(arguments.At(2), out ushort tme) && tme <= 0)
                    {
                        response = $"Invalid value for duration: {arguments.At(2)}";
                        return false;
                    }

                    foreach (Player P in PlyList)
                        P.ShowHint(EventHandlers.FormatArguments(arguments, 3), tme);


                    StringBuilder Builder = new StringBuilder("Hint sent to players: ");
                    foreach (Player P in PlyList)
                    {
                        Builder.Append("\"");
                        Builder.Append(P.Nickname);
                        Builder.Append("\"");
                        Builder.Append(" ");
                    }
                    string message = Builder.ToString();
                    Builder.Clear();
                    response = message;
                    return true;
                case "group":
                    if (arguments.Count < 4)
                    {
                        response = "Usage: hbc group (group) (time) (message)";
                        return false;
                    }

                    UserGroup BroadcastGroup = ServerStatic.PermissionsHandler.GetGroup(arguments.At(1));
                    if (BroadcastGroup == null)
                    {
                        response = $"Invalid group: {arguments.At(1)}";
                        return false;
                    }

                    if (!ushort.TryParse(arguments.At(2), out ushort tim) && tim <= 0)
                    {
                        response = $"Invalid value for duration: {arguments.At(2)}";
                        return false;
                    }

                    foreach (Player player in Player.List)
                    {
                        if (player.Group.BadgeText.Equals(BroadcastGroup.BadgeText))
                            player.ShowHint(EventHandlers.FormatArguments(arguments, 3), tim);
                    }

                    response = $"Hint sent to all members of \"{arguments.At(1)}\"";
                    return true;
                case "groups":
                    if (arguments.Count < 4)
                    {
                        response = "Usage: hbc groups (list of groups (i.e.: owner,admin,moderator)) (time) (message)";
                        return false;
                    }

                    string[] Groups = arguments.At(1).Split(',');
                    List<string> GroupList = new List<string>();
                    foreach (string s in Groups)
                    {
                        UserGroup BroadGroup = ServerStatic.PermissionsHandler.GetGroup(s);
                        if (BroadGroup != null)
                            GroupList.Add(BroadGroup.BadgeText);

                    }

                    if (!ushort.TryParse(arguments.At(2), out ushort e) && e <= 0)
                    {
                        response = $"Invalid value for duration: {arguments.At(2)}";
                        return false;
                    }

                    foreach (Player P in Player.List)
                        if (GroupList.Contains(P.Group.BadgeText))
                            P.ShowHint(EventHandlers.FormatArguments(arguments, 3), e);


                    StringBuilder Bdr = new StringBuilder("Hint sent to groups with badge text: ");
                    foreach (string P in GroupList)
                    {
                        Bdr.Append("\"");
                        Bdr.Append(P);
                        Bdr.Append("\"");
                        Bdr.Append(" ");
                    }
                    string ms = Bdr.ToString();
                    Bdr.Clear();
                    response = ms;
                    return true;
                case "role":
                    if (arguments.Count < 4)
                    {
                        response = "Usage: hbc role (RoleType) (time) (message)";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out RoleType Role))
                    {
                        response = $"Invalid value for RoleType: {arguments.At(1)}";
                        return false;
                    }

                    if (!ushort.TryParse(arguments.At(2), out ushort te) && te <= 0)
                    {
                        response = $"Invalid value for duration: {arguments.At(2)}";
                        return false;
                    }

                    foreach (Player Player in Player.List)
                    {
                        if (Player.Role == Role)
                            Player.ShowHint(EventHandlers.FormatArguments(arguments, 3), te);
                    }

                    response = $"Hint sent to all members of \"{arguments.At(1)}\"";
                    return true;
                case "roles":
                    if (arguments.Count < 4)
                    {
                        response = "Usage: hbc roles (RoleType group (i.e.: ClassD, Scientist, NtfCadet)) (time) (message)";
                        return false;
                    }

                    string[] Roles = arguments.At(1).Split(',');
                    List<RoleType> RoleList = new List<RoleType>();
                    foreach (string s in Roles)
                    {
                        if (Enum.TryParse(s, true, out RoleType R))
                            RoleList.Add(R);
                    }

                    if (!ushort.TryParse(arguments.At(2), out ushort ti) && ti <= 0)
                    {
                        response = $"Invalid value for duration: {arguments.At(2)}";
                        return false;
                    }

                    foreach (Player P in Player.List)
                        if (RoleList.Contains(P.Role))
                            P.ShowHint(EventHandlers.FormatArguments(arguments, 3), ti);

                    StringBuilder Build = new StringBuilder("Hint sent to roles: ");
                    foreach (RoleType Ro in RoleList)
                    {
                        Build.Append("\"");
                        Build.Append(Ro.ToString());
                        Build.Append("\"");
                        Build.Append(" ");
                    }
                    string msg = Build.ToString();
                    Build.Clear();
                    response = msg;
                    return true;
                case "random":
                case "someone":
                    if (arguments.Count < 3)
                    {
                        response = "Usage: hbc (random / someone) (time) (message)";
                        return false;
                    }

                    if (!ushort.TryParse(arguments.At(1), out ushort me) && me <= 0)
                    {
                        response = $"Invalid value for duration: {arguments.At(1)}";
                        return false;
                    }

                    Player Plyr = Player.List.ToList()[Plugin.NumGen.Next(0, Player.List.Count())];
                    Plyr.ShowHint(EventHandlers.FormatArguments(arguments, 2), me);
                    response = $"Hint sent to {Plyr.Nickname}";
                    return true;
                case "staff":
                case "admin":
                    if (arguments.Count < 3)
                    {
                        response = "Usage: hbc (staff / admin) (time) (message)";
                        return false;
                    }

                    if (!ushort.TryParse(arguments.At(1), out ushort t))
                    {
                        response = $"Invalid value for hint broadcast time: {arguments.At(1)}";
                        return false;
                    }

                    foreach (Player Pl in Player.List)
                    {
                        if (Pl.ReferenceHub.serverRoles.RemoteAdmin)
                            Pl.ShowHint($"<color=orange>[Admin Hint]</color> <color=green>{EventHandlers.FormatArguments(arguments, 2)} - {((CommandSender)sender).Nickname}</color>", t);
                    }

                    response = $"Hint sent to all currently online staff";
                    return true;
                case "clearall":
                    if (arguments.Count != 1)
                    {
                        response = "Usage: hbc clearall";
                        return false;
                    }

                    foreach (Player Py in Player.List)
                        Py.ShowHint(" ");
                    response = "All hints have been cleared";
                    return true;
                default:
                    if (arguments.Count < 3)
                    {
                        response = "Usage: hbc (time) (message)";
                        return false;
                    }

                    if (!ushort.TryParse(arguments.At(1), out ushort tm))
                    {
                        response = $"Invalid value for hint broadcast time: {arguments.At(0)}";
                        return false;
                    }

                    foreach (Player Py in Player.List)
                        Py.ShowHint(EventHandlers.FormatArguments(arguments, 2), tm);
                    break;
            }
            response = "";
            return false;
        }
    }
}
