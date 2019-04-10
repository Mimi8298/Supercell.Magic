namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceInvitationSendFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24321;
        private Reason m_reason;

        public AllianceInvitationSendFailedMessage() : this(0)
        {
            // AllianceInvitationSendFailedMessage.
        }

        public AllianceInvitationSendFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceInvitationSendFailedMessage.
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
            return AllianceInvitationSendFailedMessage.MESSAGE_TYPE;
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
            GENERIC,
            NO_RIGHTS,
            NO_CASTLE,
            ALREADY_IN_ALLIANCE,
            ALREADY_HAS_AN_INVITE,
            HAS_TOO_MANY_INVITES,
            USER_BANNED
        }
    }
}