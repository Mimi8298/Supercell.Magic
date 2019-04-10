namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AvatarRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24403;

        private int m_nextEndTimeSeconds;
        private int m_seasonYear;
        private int m_seasonMonth;
        private int m_lastSeasonYear;
        private int m_lastSeasonMonth;

        private LogicArrayList<AvatarRankingEntry> m_avatarRankingList;
        private LogicArrayList<AvatarRankingEntry> m_lastSeasonAvatarRankingList;

        public AvatarRankingListMessage() : this(0)
        {
            // AvatarRankingListMessage.
        }

        public AvatarRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarRankingListMessage.
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

            int count2 = this.m_stream.ReadInt();

            if (count2 > -1)
            {
                this.m_lastSeasonAvatarRankingList = new LogicArrayList<AvatarRankingEntry>(count2);

                for (int i = 0; i < count2; i++)
                {
                    AvatarRankingEntry avatarRankingEntry = new AvatarRankingEntry();
                    avatarRankingEntry.Decode(this.m_stream);
                    this.m_lastSeasonAvatarRankingList.Add(avatarRankingEntry);
                }
            }

            this.m_nextEndTimeSeconds = this.m_stream.ReadInt();
            this.m_seasonYear = this.m_stream.ReadInt();
            this.m_seasonMonth = this.m_stream.ReadInt();
            this.m_lastSeasonYear = this.m_stream.ReadInt();
            this.m_lastSeasonMonth = this.m_stream.ReadInt();
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

            if (this.m_lastSeasonAvatarRankingList != null)
            {
                this.m_stream.WriteInt(this.m_lastSeasonAvatarRankingList.Size());

                for (int i = 0; i < this.m_lastSeasonAvatarRankingList.Size(); i++)
                {
                    this.m_lastSeasonAvatarRankingList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }

            this.m_stream.WriteInt(this.m_nextEndTimeSeconds);
            this.m_stream.WriteInt(this.m_seasonYear);
            this.m_stream.WriteInt(this.m_seasonMonth);
            this.m_stream.WriteInt(this.m_lastSeasonYear);
            this.m_stream.WriteInt(this.m_lastSeasonMonth);
        }

        public override short GetMessageType()
        {
            return AvatarRankingListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 28;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_avatarRankingList = null;
            this.m_lastSeasonAvatarRankingList = null;
        }

        public LogicArrayList<AvatarRankingEntry> RemoveAvatarRankingList()
        {
            LogicArrayList<AvatarRankingEntry> tmp = this.m_avatarRankingList;
            this.m_avatarRankingList = null;
            return tmp;
        }

        public LogicArrayList<AvatarRankingEntry> RemoveLastSeasonAvatarRankingList()
        {
            LogicArrayList<AvatarRankingEntry> tmp = this.m_lastSeasonAvatarRankingList;
            this.m_lastSeasonAvatarRankingList = null;
            return tmp;
        }

        public void SetAvatarRankingList(LogicArrayList<AvatarRankingEntry> list)
        {
            this.m_avatarRankingList = list;
        }

        public void SetLastSeasonAvatarRankingList(LogicArrayList<AvatarRankingEntry> list)
        {
            this.m_lastSeasonAvatarRankingList = list;
        }

        public int GetNextEndTimeSeconds()
        {
            return this.m_nextEndTimeSeconds;
        }

        public void SetNextEndTimeSeconds(int value)
        {
            this.m_nextEndTimeSeconds = value;
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

        public int GetLastSeasonYear()
        {
            return this.m_lastSeasonYear;
        }

        public void SetLastSeasonYear(int value)
        {
            this.m_lastSeasonYear = value;
        }

        public int GetLastSeasonMonth()
        {
            return this.m_lastSeasonMonth;
        }

        public void SetLastSeasonMonth(int value)
        {
            this.m_lastSeasonMonth = value;
        }
    }
}