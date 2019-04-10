namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AvatarLastSeasonRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24405;

        private int m_seasonYear;
        private int m_seasonMonth;

        private LogicArrayList<AvatarRankingEntry> m_avatarRankingList;

        public AvatarLastSeasonRankingListMessage() : this(0)
        {
            // AvatarLastSeasonRankingListMessage.
        }

        public AvatarLastSeasonRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarLastSeasonRankingListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int count = this.m_stream.ReadInt();

            if (count > -1)
            {
                this.m_avatarRankingList = new LogicArrayList<AvatarRankingEntry>(count);

                for (int i = 0; i < count; i++)
                {
                    AvatarRankingEntry avatarRankingEntry = new AvatarRankingEntry();
                    avatarRankingEntry.Decode(this.m_stream);
                    this.m_avatarRankingList.Add(avatarRankingEntry);
                }
            }

            this.m_seasonMonth = this.m_stream.ReadInt();
            this.m_seasonYear = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_avatarRankingList != null)
            {
                this.m_stream.WriteInt(this.m_avatarRankingList.Size());

                for (int i = 0; i < this.m_avatarRankingList.Size(); i++)
                {
                    this.m_avatarRankingList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }

            this.m_stream.WriteInt(this.m_seasonMonth);
            this.m_stream.WriteInt(this.m_seasonYear);
        }

        public override short GetMessageType()
        {
            return AvatarLastSeasonRankingListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 28;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_avatarRankingList = null;
        }

        public LogicArrayList<AvatarRankingEntry> RemoveAvatarRankingList()
        {
            LogicArrayList<AvatarRankingEntry> tmp = this.m_avatarRankingList;
            this.m_avatarRankingList = null;
            return tmp;
        }

        public void SetAvatarRankingList(LogicArrayList<AvatarRankingEntry> list)
        {
            this.m_avatarRankingList = list;
        }

        public int GetSeasonYear()
        {
            return this.m_seasonYear;
        }

        public void SetSeasonYear(int value)
        {
            this.m_seasonYear = value;
        }

        public int GetSeasonMonth()
        {
            return this.m_seasonMonth;
        }

        public void SetSeasonMonth(int value)
        {
            this.m_seasonMonth = value;
        }
    }
}