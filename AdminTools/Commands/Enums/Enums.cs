using CommandSystem;
using NorthwoodLib.Pools;
using System;
using System.Text;

namespace AdminTools.Commands.Enums
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Enums : ParentCommand
    {
        public Enums() => LoadGeneratedCommands();

        public override string Command { get; } = "enums";

        public override string[] Aliases { get; } = new string[] { "enum" };

        public override string Description { get; } = "Lists all enums AdminTools uses";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            EventHandlers.LogCommandUsed((CommandSender)sender, EventHandlers.FormatArguments(arguments, 0));
            StringBuilder ListBuilder = StringBuilderPool.Shared.Rent();
            ListBuilder.Append("Here are the following enums you can use in commands:\n\nBreakType: ");
            foreach (BreakType Bt in Enum.GetValues(typeof(BreakType)))
            {
                ListBuilder.Append(Bt.ToString());
                ListBuilder.Append(" ");
            }
            ListBuilder.AppendLine();
            ListBuilder.Append("MoveType: ");
            foreach (MoveType Mt in Enum.GetValues(typeof(MoveType)))
            {
                ListBuilder.Append(Mt.ToString());
                ListBuilder.Append(" ");
            }
            ListBuilder.AppendLine();
            ListBuilder.Append("GrenadeType: ");
            foreach (GrenadeType Gt in Enum.GetValues(typeof(GrenadeType)))
            {
                ListBuilder.Append(Gt.ToString());
                ListBuilder.Append(" ");
            }
            ListBuilder.AppendLine();
            ListBuilder.Append("VectorAxis: ");
            foreach (VectorAxis Va in Enum.GetValues(typeof(VectorAxis)))
            {
                ListBuilder.Append(Va.ToString());
                ListBuilder.Append(" ");
            }
            ListBuilder.AppendLine();
            ListBuilder.Append("PositionModifier: ");
            foreach (PositionModifier Pm in Enum.GetValues(typeof(PositionModifier)))
            {
                ListBuilder.Append(Pm.ToString());
                ListBuilder.Append(" ");
            }
            string message = ListBuilder.ToString();
            StringBuilderPool.Shared.Return(ListBuilder);
            response = message;
            return true;
        }
    }
}
