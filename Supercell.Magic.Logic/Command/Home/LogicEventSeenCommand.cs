namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicEventSeenCommand : LogicCommand
    {
        private int m_timestamp;

        public override void Decode(ByteStream stream)
        {
            this.m_timestamp = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_timestamp);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.EVENT_SEEN;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            level.GetGameMode().GetCalendar().SetEventSeenTime(this.m_timestamp);
            return 0;
        }
    }
}