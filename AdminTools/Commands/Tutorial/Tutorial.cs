using CommandSystem;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdminTools.Commands.Tutorial
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Tutorial : ParentCommand
    {
        Player Ply;

        public Tutorial() => LoadGeneratedCommands();

        public override string Command { get; } = "tutorial";

        public override string[] Aliases { get; } = new string[] { "tut" };

        public override string Description { get; } = "Sets a player as a tutorial conveniently";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            MoveType Value = MoveType.Move;
            if (!((CommandSender)sender).CheckPermission("at.tut"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            switch (arguments.Count)
            {
                case 0:
                case 1:
                    if (arguments.Count == 0)
                        Ply = Player.Get(((CommandSender)sender).Nickname);
                    else
                    {
                        if (String.IsNullOrWhiteSpace(arguments.At(0)))
                        {
                            response = "Please do not try to put a space as tutorial";
                            return false;
                        }

                        Ply = Player.Get(arguments.At(0));
                        if (Ply == null)
                        {
                            response = $"Player not found: {arguments.At(0)}";
                            return false;
                        }
                    }

                    Value = MoveType.Move;
                    DoTutorialFunction(Ply, Value, out response);
                    return true;
                case 2:
                    if (String.IsNullOrWhiteSpace(arguments.At(0)))
                    {
                        response = "Please do not try to put a space as tutorial";
                        return false;
                    }

                    Ply = Player.Get(arguments.At(0));
                    if (Ply == null)
                    {
                        response = $"Player not found: {arguments.At(0)}";
                        return false;
                    }

                    if (!Enum.TryParse(arguments.At(1), true, out MoveType Type))
                    {
                        response = $"Invalid value for move type: {arguments.At(1)}";
                        return false;
                    }
                    Value = Type;
                    DoTutorialFunction(Ply, Value, out response);
                    return true;
                default:
                    response = "Usage: tutorial (optional: id / name) (optional: stay / move)";
                    return false;
            }
        }

        private IEnumerator<float> SetClassAsTutorial(Player Ply) 
        {
            Vector3 OldPos = Ply.Position;
            Ply.Role = RoleType.Tutorial;
            yield return Timing.WaitForSeconds(0.5f);
            Ply.Position = OldPos;
        }

        private void DoTutorialFunction(Player Ply, MoveType Value, out string response)
        {
            if (Ply.Role != RoleType.Tutorial)
            {
                if (Value == MoveType.Move)
                    Timing.RunCoroutine(EventHandlers.DoTut(Ply));
                else
                    Timing.RunCoroutine(SetClassAsTutorial(Ply));
                response = $"Player {Ply.Nickname} is now set to tutorial";
            }
            else
            {
                Ply.Role = RoleType.Spectator;
                response = $"Player {Ply.Nickname} is now set to spectator";
            }
        }
    }
}
