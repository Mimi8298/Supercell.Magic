namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceCreateFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24332;
        
        private Reason m_reason;

        public AllianceCreateFailedMessage() : this(0)
        {
            // AllianceCreateFailedMessage.
        }

        public AllianceCreateFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceCreateFailedMessage.
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
            return AllianceCreateFailedMessage.MESSAGE_TYPE;
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

        public void SetReason(Reason value)
        {
            this.m_reason = value;
        }

        public enum Reason
        {
            GENERIC,
            INVALID_NAME = 1,
            INVALID_DESCRIPTION = 2,
            NAME_TOO_SHORT = 3,
            NAME_TOO_LONG = 4
        }
    }
}