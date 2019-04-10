namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.League.Entry;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicLegendSeasonScoreCommand : LogicServerCommand
    {
        private int m_lastSeasonState;
        private int m_lastSeasonYear;
        private int m_lastSeasonMonth;
        private int m_lastSeasonRank;
        private int m_lastSeasonScore;

        private int m_scoreChange;
        private int m_villageType;

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_lastSeasonState = stream.ReadInt();
            this.m_lastSeasonYear = stream.ReadInt();
            this.m_lastSeasonMonth = stream.ReadInt();
            this.m_lastSeasonScore = stream.ReadInt();
            this.m_lastSeasonRank = stream.ReadInt();
            this.m_scoreChange = stream.ReadInt();
            this.m_villageType = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_lastSeasonState);
            encoder.WriteInt(this.m_lastSeasonYear);
            encoder.WriteInt(this.m_lastSeasonMonth);
            encoder.WriteInt(this.m_lastSeasonScore);
            encoder.WriteInt(this.m_lastSeasonRank);
            encoder.WriteInt(this.m_scoreChange);
            encoder.WriteInt(this.m_villageType);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                LogicLegendSeasonEntry legendSeasonEntry;

                if (this.m_villageType == 1)
                {
                    legendSeasonEntry = playerAvatar.GetLegendSeasonEntryVillage2();
                }
                else
                {
                    if (this.m_villageType != 0)
                    {
                        return -2;
                    }

                    legendSeasonEntry = playerAvatar.GetLegendSeasonEntry();
                }

                if (legendSeasonEntry.GetLastSeasonState() != this.m_lastSeasonState)
                {
                    if (this.m_villageType == 1)
                    {
                        playerAvatar.SetDuelScore(playerAvatar.GetDuelScore() - this.m_scoreChange);
                        playerAvatar.SetLegendaryScore(playerAvatar.GetLegendaryScoreVillage2() + this.m_scoreChange);
                    }
                    else
                    {
                        playerAvatar.SetScore(playerAvatar.GetScore() - this.m_scoreChange);
                        playerAvatar.SetLegendaryScore(playerAvatar.GetLegendaryScore() + this.m_scoreChange);
                    }

                    legendSeasonEntry.SetLastSeasonState(this.m_lastSeasonState);
                    legendSeasonEntry.SetLastSeasonDate(this.m_lastSeasonYear, this.m_lastSeasonMonth);
                    legendSeasonEntry.SetLastSeasonRank(this.m_lastSeasonRank);
                    legendSeasonEntry.SetLastSeasonScore(this.m_lastSeasonScore);

                    bool bestSeason = false;

                    if (legendSeasonEntry.GetBestSeasonState() == 0 ||
                        this.m_lastSeasonRank < legendSeasonEntry.GetBestSeasonRank() ||
                        this.m_lastSeasonRank == legendSeasonEntry.GetBestSeasonRank() &&
                        this.m_lastSeasonScore > legendSeasonEntry.GetBestSeasonScore())
                    {
                        legendSeasonEntry.SetBestSeasonState(this.m_lastSeasonState);
                        legendSeasonEntry.SetBestSeasonDate(this.m_lastSeasonYear, this.m_lastSeasonMonth);
                        legendSeasonEntry.SetBestSeasonRank(this.m_lastSeasonRank);
                        legendSeasonEntry.SetBestSeasonScore(this.m_lastSeasonScore);

                        bestSeason = true;
                    }

                    playerAvatar.GetChangeListener().LegendSeasonScoreChanged(this.m_lastSeasonState, this.m_lastSeasonScore, this.m_scoreChange, bestSeason, this.m_villageType);
                    level.GetGameListener().LegendSeasonScoreChanged(this.m_lastSeasonState, this.m_lastSeasonScore, this.m_scoreChange, bestSeason, this.m_villageType);

                    return 0;
                }
            }

            return -1;
        }

        public void SetDatas(int seasonState, int seasonYear, int seasonMonth, int seasonRank, int seasonScore, int scoreChange, int villageType)
        {
            this.m_lastSeasonState = seasonState;
            this.m_lastSeasonYear = seasonYear;
            this.m_lastSeasonMonth = seasonMonth;
            this.m_lastSeasonRank = seasonRank;
            this.m_lastSeasonScore = seasonScore;
            this.m_scoreChange = scoreChange;
            this.m_villageType = villageType;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.LEGEND_SEASON_SCORE;
        }
    }
}