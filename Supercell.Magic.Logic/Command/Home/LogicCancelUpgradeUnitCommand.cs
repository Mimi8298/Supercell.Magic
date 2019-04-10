namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicCancelUpgradeUnitCommand : LogicCommand
    {
        private int m_gameObjectId;

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CANCEL_UPGRADE_UNIT;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_gameObjectId = 0;
        }

        public override int Execute(LogicLevel level)
        {
            return -1;
        }
    }
}