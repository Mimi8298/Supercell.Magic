namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AvatarDuelRankingEntry : RankingEntry
    {
        private int m_expLevel;
        private int m_duelWinCount;
        private int m_duelDrawCount;
        private int m_duelLoseCount;
        private int m_allianceBadgeId;

        private string m_country;
        private string m_allianceName;

        private LogicLong m_homeId;
        private LogicLong m_allianceId;

        public AvatarDuelRankingEntry()
        {
            this.m_allianceBadgeId = -1;
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt(this.m_expLevel);
            stream.WriteInt(this.m_duelWinCount);
            stream.WriteInt(this.m_duelDrawCount);
            stream.WriteInt(this.m_duelLoseCount);
            stream.WriteString(this.m_country);
            stream.WriteLong(this.m_homeId);
            stream.WriteInt(0);
            stream.WriteInt(0);

            if (this.m_allianceId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_allianceId);
                stream.WriteString(this.m_allianceName);
                stream.WriteInt(this.m_allianceBadgeId);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_expLevel = stream.ReadInt();
            this.m_duelWinCount = stream.ReadInt();
            this.m_duelDrawCount = stream.ReadInt();
            this.m_duelLoseCount = stream.ReadInt();
            this.m_country = stream.ReadString(900000);
            this.m_homeId = stream.ReadLong();

            stream.ReadInt();
            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_allianceId = stream.ReadLong();
                this.m_allianceName = stream.ReadString(900000);
                this.m_allianceBadgeId = stream.ReadInt();
            }
        }

        public int GetExpLevel()
        {
            return this.m_expLevel;
        }

        public void SetExpLevel(int value)
        {
            this.m_expLevel = value;
        }

        public int GetDuelWinCount()
        {
            return this.m_duelWinCount;
        }

        public void SetDuelWinCount(int value)
        {
            this.m_duelWinCount = value;
        }

        public int GetDuelLoseCount()
        {
            return this.m_duelLoseCount;
        }

        public void SetDuelLoseCount(int value)
        {
            this.m_duelLoseCount = value;
        }

        public int GetDuelDrawCount()
        {
            return this.m_duelDrawCount;
        }

        public void SetDuelDrawCount(int value)
        {
            this.m_duelDrawCount = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public string GetCountry()
        {
            return this.m_country;
        }

        public void SetCountry(string value)
        {
            this.m_country = value;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong value)
        {
            this.m_homeId = value;
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong value)
        {
            this.m_allianceId = value;
        }
    }
}