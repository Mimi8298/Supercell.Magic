namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AskForAvatarDuelLastSeasonRankingListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14408;

        private LogicLong m_avatarId;

        public AskForAvatarDuelLastSeasonRankingListMessage() : this(0)
        {
            // AskForAvatarDuelLastSeasonRankingListMessage.
        }

        public AskForAvatarDuelLastSeasonRankingListMessage(short messageVersion) : base(messageVersion)
        {
            // AskForAvatarDuelLastSeasonRankingListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            if (this.m_stream.ReadBoolean())
            {
                this.m_avatarId = this.m_stream.ReadLong();
            }
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
        }

        public override short GetMessageType()
        {
            return AskForAvatarDuelLastSeasonRankingListMessage.MESSAGE_TYPE;
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
    }
}