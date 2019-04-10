namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicNewsSeenCommand : LogicCommand
    {
        private int m_lastSeenNews;

        public LogicNewsSeenCommand()
        {
            // LogicNewsSeenCommand.
        }

        public LogicNewsSeenCommand(int lastSeenNews)
        {
            this.m_lastSeenNews = lastSeenNews;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_lastSeenNews = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_lastSeenNews);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.NEWS_SEEN;
        }

        public override int Execute(LogicLevel level)
        {
            level.SetLastSeenNews(this.m_lastSeenNews);
            return 0;
        }
    }
}