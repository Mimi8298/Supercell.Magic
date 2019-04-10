namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicLeagueNotificationSeenCommand : LogicCommand
    {
        private int m_lastLeagueRank;
        private int m_lastSeasonSeen;

        public override void Decode(ByteStream stream)
        {
            this.m_lastLeagueRank = stream.ReadInt();
            this.m_lastSeasonSeen = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_lastLeagueRank);
            encoder.WriteInt(this.m_lastSeasonSeen);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.LEAGUE_NOTIFICATION_SEEN;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            level.SetLastLeagueRank(this.m_lastLeagueRank);
            level.SetLastLeagueShuffle(false);
            level.SetLastSeasonSeen(this.m_lastSeasonSeen);

            return 0;
        }
    }
}