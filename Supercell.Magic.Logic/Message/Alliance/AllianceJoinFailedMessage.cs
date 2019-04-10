namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceJoinFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24302;

        private Reason m_reason;

        public AllianceJoinFailedMessage() : this(0)
        {
            // AllianceJoinFailedMessage.
        }

        public AllianceJoinFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceJoinFailedMessage.
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
            return AllianceJoinFailedMessage.MESSAGE_TYPE;
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
            FULL,
            CLOSED,
            ALREADY_IN_ALLIANCE,
            SCORE,
            BANNED,
        }
    }
}