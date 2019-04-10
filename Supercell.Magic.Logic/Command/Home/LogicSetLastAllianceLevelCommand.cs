namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSetLastAllianceLevelCommand : LogicCommand
    {
        private int m_allianceLevel;

        public override void Decode(ByteStream stream)
        {
            this.m_allianceLevel = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_allianceLevel);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SET_LAST_ALLIANCE_LEVEL;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            level.SetLastAllianceLevel(this.m_allianceLevel);
            return 0;
        }
    }
}