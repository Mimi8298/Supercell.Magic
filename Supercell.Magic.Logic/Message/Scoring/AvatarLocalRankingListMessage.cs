namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AvatarLocalRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24404;

        private LogicArrayList<AvatarRankingEntry> m_avatarRankingList;

        public AvatarLocalRankingListMessage() : this(0)
        {
            // AvatarLocalRankingListMessage.
        }

        public AvatarLocalRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarLocalRankingListMessage.
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
            return AvatarLocalRankingListMessage.MESSAGE_TYPE;
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
    }
}