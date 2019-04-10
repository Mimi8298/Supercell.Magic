namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AskForAvatarRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14403;

        private LogicLong m_avatarId;
        private int m_villageType;

        public AskForAvatarRankingListMessage() : this(0)
        {
            // AskForAvatarRankingListMessage.
        }

        public AskForAvatarRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AskForAvatarRankingListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            if (this.m_stream.ReadBoolean())
            {
                this.m_avatarId = this.m_stream.ReadLong();
            }

            this.m_villageType = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_avatarId != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_avatarId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }

            this.m_villageType = this.m_stream.ReadInt();
        }

        public override short GetMessageType()
        {
            return AskForAvatarRankingListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 28;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_avatarId = null;
        }

        public LogicLong RemoveAvatarId()
        {
            LogicLong tmp = this.m_avatarId;
            this.m_avatarId = null;
            return tmp;
        }

        public void SetAvatarId(LogicLong id)
        {
            this.m_avatarId = id;
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