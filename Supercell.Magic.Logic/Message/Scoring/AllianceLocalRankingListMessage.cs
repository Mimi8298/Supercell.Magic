namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AllianceLocalRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24402;

        private int m_villageType;
        private LogicArrayList<AllianceRankingEntry> m_allianceRankingList;

        public AllianceLocalRankingListMessage() : this(0)
        {
            // AllianceLocalRankingListMessage.
        }

        public AllianceLocalRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceLocalRankingListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int count = this.m_stream.ReadInt();

            if (count > -1)
            {
                this.m_allianceRankingList = new LogicArrayList<AllianceRankingEntry>(count);

                for (int i = 0; i < count; i++)
                {
                    AllianceRankingEntry allianceRankingEntry = new AllianceRankingEntry();
                    allianceRankingEntry.Decode(this.m_stream);
                    this.m_allianceRankingList.Add(allianceRankingEntry);
                }
            }

            this.m_villageType = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_allianceRankingList != null)
            {
                this.m_stream.WriteInt(this.m_allianceRankingList.Size());

                for (int i = 0; i < this.m_allianceRankingList.Size(); i++)
                {
                    this.m_allianceRankingList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }

            this.m_stream.WriteInt(this.m_villageType);
        }

        public override short GetMessageType()
        {
            return AllianceLocalRankingListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 28;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_allianceRankingList = null;
        }

        public LogicArrayList<AllianceRankingEntry> RemoveAllianceRankingList()
        {
            LogicArrayList<AllianceRankingEntry> tmp = this.m_allianceRankingList;
            this.m_allianceRankingList = null;
            return tmp;
        }

        public void SetAllianceRankingList(LogicArrayList<AllianceRankingEntry> list)
        {
            this.m_allianceRankingList = list;
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