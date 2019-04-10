namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class ChallengeFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24121;
        
        private Reason m_reason;

        public ChallengeFailedMessage() : this(0)
        {
            // ChallengeFailedMessage.
        }

        public ChallengeFailedMessage(short messageVersion) : base(messageVersion)
        {
            // ChallengeFailedMessage.
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
            return ChallengeFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public Reason GetReason()
        {
            return this.m_reason;
        }

        public void SetReason(Reason value)
        {
            this.m_reason = value;
        }

        public enum Reason
        {
            GENERIC = 0,
            ALREADY_CLOSED = 3,
            PERSONAL_BREAK_ATTACK_DISABLED = 5
        }
    }
}