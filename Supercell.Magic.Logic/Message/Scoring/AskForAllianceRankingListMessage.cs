namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AskForAllianceRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14401;

        private LogicLong m_allianceId;

        private int m_villageType;
        private bool m_localRanking;

        public AskForAllianceRankingListMessage() : this(0)
        {
            // AskForAllianceRankingListMessage.
        }

        public AskForAllianceRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AskForAllianceRankingListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            if (this.m_stream.ReadBoolean())
            {
                this.m_allianceId = this.m_stream.ReadLong();
            }

            this.m_localRanking = this.m_stream.ReadBoolean();
            this.m_villageType = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_allianceId != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_allianceId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }

            this.m_localRanking = this.m_stream.ReadBoolean();
            this.m_villageType = this.m_stream.ReadInt();
        }

        public override short GetMessageType()
        {
            return AskForAllianceRankingListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 28;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong RemoveAllianceId()
        {
            LogicLong tmp = this.m_allianceId;
            this.m_allianceId = null;
            return tmp;
        }

        public void SetAllianceId(LogicLong id)
        {
            this.m_allianceId = id;
        }

        public bool LocalRanking()
        {
            return this.m_localRanking;
        }

        public void SetLocalRanking(bool value)
        {
            this.m_localRanking = value;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public void SetVillageType(int value)
        {
            this.m_villageType = value;
        }
    }
}