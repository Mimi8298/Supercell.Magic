namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class StartFriendlyChallengeSpectateMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14110;

        private LogicLong m_streamId;
        private LogicLong m_attackerId;

        public StartFriendlyChallengeSpectateMessage() : this(0)
        {
            // StartFriendlyChallengeSpectateMessage.
        }

        public StartFriendlyChallengeSpectateMessage(short messageVersion) : base(messageVersion)
        {
            // StartFriendlyChallengeSpectateMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_stream.ReadInt();

            if (this.m_stream.ReadBoolean())
            {
                this.m_streamId = this.m_stream.ReadLong();
            }

            if (this.m_stream.ReadBoolean())
            {
                this.m_attackerId = this.m_stream.ReadLong();
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(0);
            this.m_stream.WriteBoolean(this.m_streamId != null);

            if (this.m_streamId != null)
            {
                this.m_stream.WriteLong(this.m_streamId);
            }

            this.m_stream.WriteBoolean(this.m_attackerId != null);

            if (this.m_attackerId != null)
            {
                this.m_stream.WriteLong(this.m_attackerId);
            }
        }

        public override short GetMessageType()
        {
            return StartFriendlyChallengeSpectateMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong GetStreamId()
        {
            return this.m_streamId;
        }

        public void SetStreamId(LogicLong value)
        {
            this.m_streamId = value;
        }

        public LogicLong GetAttackerId()
        {
            return this.m_attackerId;
        }

        public void SetAttackerId(LogicLong value)
        {
            this.m_attackerId = value;
        }
    }
}