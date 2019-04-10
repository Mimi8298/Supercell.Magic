namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicEditModeShownCommand : LogicCommand
    {
        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.EDIT_MODE_SHOWN;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            level.SetEditModeShown();
            return 0;
        }
    }
}