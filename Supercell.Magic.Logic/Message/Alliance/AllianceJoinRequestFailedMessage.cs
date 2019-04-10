namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceJoinRequestFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24320;

        private Reason m_reason;

        public AllianceJoinRequestFailedMessage() : this(0)
        {
            // AllianceJoinRequestFailedMessage.
        }

        public AllianceJoinRequestFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceJoinRequestFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_reason = (Reason) this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt((int) this.m_reason);
        }

        public override short GetMessageType()
        {
            return AllianceJoinRequestFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public Reason GetReason()
        {
            return this.m_reason;
        }

        public void SetReason(Reason reason)
        {
            this.m_reason = reason;
        }

        public enum Reason
        {
            GENERIC,
            CLOSED,
            ALREADY_SENT,
            NO_SCORE,
            BANNED,
            TOO_MANY_PENDING_REQUESTS,
            NO_DUEL_SCORE,
        }
    }
}