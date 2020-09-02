using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands.BreakDoors
{
    public class All : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[] { "*" };

        public string Description { get; } = "Gives everyone the ability to break everything or just doors";

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
                response = "Usage: breakdoors all / * (doors, everything)";
                return false;
            }

            if (!Enum.TryParse(arguments.At(0), true, out BreakType Type))
            {
                response = $"Invalid breaking type: {arguments.At(0)}";
                return false;
            }

            foreach (Player Ply in Player.List)
            {
                if (!Ply.ReferenceHub.TryGetComponent(out BreakDoorComponent BdComponent))
                {
                    Ply.GameObject.AddComponent<BreakDoorComponent>();
                    switch (Type)
                    {
                        case BreakType.Doors:
                            Ply.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = false;
                            Ply.IsBypassModeEnabled = false;
                            break;
                        case BreakType.All:
                            Ply.ReferenceHub.GetComponent<BreakDoorComponent>().breakAll = true;
                            Ply.IsBypassModeEnabled = true;
                            break;
                    }
                }
                else
                {
                    switch (Type)
                    {
                        case BreakType.Doors:
                            BdComponent.breakAll = false;
                            Ply.IsBypassModeEnabled = false;
                            break;
                        case BreakType.All:
                            BdComponent.breakAll = true;
                            Ply.IsBypassModeEnabled = true;
                            break;
                    }
                }
            }

            response = $"Breaking {((Type == BreakType.Doors) ? "doors" : "everything")} is on for everyone now";
            return true;
        }
    }
}
