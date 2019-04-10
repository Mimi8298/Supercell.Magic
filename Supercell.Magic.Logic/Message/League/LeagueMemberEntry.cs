namespace Supercell.Magic.Logic.Message.League
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LeagueMemberEntry
    {
        private LogicLong m_accountId;
        private LogicLong m_avatarId;
        private LogicLong m_homeId;

        private string m_name;
        private string m_allianceName;

        private int m_score;
        private int m_order;
        private int m_previousOrder;
        private int m_attackWinCount;
        private int m_attackLoseCount;
        private int m_defenseWinCount;
        private int m_defenseLoseCount;
        private int m_allianceBadgeId;

        private LogicLong m_allianceId;

        public LeagueMemberEntry()
        {
            this.m_allianceBadgeId = -1;
        }

        public void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_accountId);
            encoder.WriteString(this.m_name);
            encoder.WriteInt(this.m_order);
            encoder.WriteInt(this.m_score);
            encoder.WriteInt(this.m_previousOrder);
            encoder.WriteInt(0);
            encoder.WriteInt(this.m_attackWinCount);
            encoder.WriteInt(this.m_attackLoseCount);
            encoder.WriteInt(this.m_defenseWinCount);
            encoder.WriteInt(this.m_defenseLoseCount);
            encoder.WriteLong(this.m_avatarId);
            encoder.WriteLong(this.m_homeId);

            if (this.m_allianceId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_allianceId);
                encoder.WriteString(this.m_allianceName);
                encoder.WriteInt(this.m_allianceBadgeId);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            encoder.WriteLong(new LogicLong(0, 0));
        }

        public void Decode(ByteStream stream)
        {
            this.m_accountId = stream.ReadLong();
            this.m_name = stream.ReadString(900000);
            this.m_order = stream.ReadInt();
            this.m_score = stream.ReadInt();
            this.m_previousOrder = stream.ReadInt();

            stream.ReadInt();

            this.m_attackWinCount = stream.ReadInt();
            this.m_attackLoseCount = stream.ReadInt();
            this.m_defenseWinCount = stream.ReadInt();
            this.m_defenseLoseCount = stream.ReadInt();
            this.m_avatarId = stream.ReadLong();
            this.m_homeId = stream.ReadLong();

            if (stream.ReadBoolean())
            {
                this.m_allianceId = stream.ReadLong();
                this.m_allianceName = stream.ReadString(900000);
                this.m_allianceBadgeId = stream.ReadInt();
            }

            stream.ReadLong();
        }

        public LogicLong GetAccountId()
        {
            return this.m_accountId;
        }

        public void SetAccountId(LogicLong value)
        {
            this.m_accountId = value;
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong value)
        {
            this.m_avatarId = value;
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong value)
        {
            this.m_homeId = value;
        }

        public string GetName()
        {
            return this.m_name;
        }

        public void SetName(string value)
        {
            this.m_name = value;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
        }

        public int GetScore()
        {
            return this.m_score;
        }

        public void SetScore(int value)
        {
            this.m_score = value;
        }

        public int GetOrder()
        {
            return this.m_order;
        }

        public void SetOrder(int value)
        {
            this.m_order = value;
        }

        public int GetPreviousOrder()
        {
            return this.m_previousOrder;
        }

        public void SetPreviousOrder(int value)
        {
            this.m_previousOrder = value;
        }

        public int GetAttackWinCount()
        {
            return this.m_attackWinCount;
        }

        public void SetAttackWinCount(int value)
        {
            this.m_attackWinCount = value;
        }

        public int GetAttackLoseCount()
        {
            return this.m_attackLoseCount;
        }

        public void SetAttackLoseCount(int value)
        {
            this.m_attackLoseCount = value;
        }

        public int GetDefenseWinCount()
        {
            return this.m_defenseWinCount;
        }

        public void SetDefenseWinCount(int value)
        {
            this.m_defenseWinCount = value;
        }

        public int GetDefenseLoseCount()
        {
            return this.m_defenseLoseCount;
        }

        public void SetDefenseLoseCount(int value)
        {
            this.m_defenseLoseCount = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
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