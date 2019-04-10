namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AllianceRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24401;

        private int m_villageType;
        private int m_nextEndTimeSeconds;

        private LogicArrayList<int> m_diamondPrizes;
        private LogicArrayList<AllianceRankingEntry> m_allianceRankingList;

        public AllianceRankingListMessage() : this(0)
        {
            // AllianceRankingListMessage.
        }

        public AllianceRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceRankingListMessage.
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

            this.m_nextEndTimeSeconds = this.m_stream.ReadInt();
            this.m_diamondPrizes = new LogicArrayList<int>();

            for (int i = 0, size = this.m_stream.ReadInt(); i < size; i++)
            {
                this.m_diamondPrizes.Add(this.m_stream.ReadInt());
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

            this.m_stream.WriteInt(this.m_nextEndTimeSeconds);
            this.m_stream.WriteInt(this.m_diamondPrizes.Size());

            for (int i = 0; i < this.m_diamondPrizes.Size(); i++)
            {
                this.m_stream.WriteInt(this.m_diamondPrizes[i]);
            }

            this.m_stream.WriteInt(this.m_villageType);
        }

        public override short GetMessageType()
        {
            return AllianceRankingListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 28;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_allianceRankingList = null;
            this.m_diamondPrizes = null;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public void SetVillageType(int value)
        {
            this.m_villageType = value;
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

        public LogicArrayList<int> RemoveDiamondPrizes()
        {
            LogicArrayList<int> tmp = this.m_diamondPrizes;
            this.m_diamondPrizes = null;
            return tmp;
        }

        public void SetDiamondPrizes(LogicArrayList<int> list)
        {
            this.m_diamondPrizes = list;
        }

        public int GetNextEndTimeSeconds()
        {
            return this.m_nextEndTimeSeconds;
        }

        public void SetNextEndTimeSeconds(int value)
        {
            this.m_nextEndTimeSeconds = value;
        }
    }
}