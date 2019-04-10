namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AvatarDuelLocalRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24407;

        private LogicArrayList<AvatarDuelRankingEntry> m_avatarRankingList;

        public AvatarDuelLocalRankingListMessage() : this(0)
        {
            // AvatarDuelLocalRankingListMessage.
        }

        public AvatarDuelLocalRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarDuelLocalRankingListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int count = this.m_stream.ReadInt();

            if (count > -1)
            {
                this.m_avatarRankingList = new LogicArrayList<AvatarDuelRankingEntry>(count);

                for (int i = 0; i < count; i++)
                {
                    AvatarDuelRankingEntry avatarDuelRankingEntry = new AvatarDuelRankingEntry();
                    avatarDuelRankingEntry.Decode(this.m_stream);
                    this.m_avatarRankingList.Add(avatarDuelRankingEntry);
                }
            }
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
        }

        public override short GetMessageType()
        {
            return AvatarDuelLocalRankingListMessage.MESSAGE_TYPE;
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

        public LogicArrayList<AvatarDuelRankingEntry> RemoveAvatarRankingList()
        {
            LogicArrayList<AvatarDuelRankingEntry> tmp = this.m_avatarRankingList;
            this.m_avatarRankingList = null;
            return tmp;
        }

        public void SetAvatarRankingList(LogicArrayList<AvatarDuelRankingEntry> list)
        {
            this.m_avatarRankingList = list;
        }
    }
}