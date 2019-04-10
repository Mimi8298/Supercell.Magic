namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class VisitFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24122;

        private int m_reason;

        public VisitFailedMessage() : this(0)
        {
            // VisitFailedMessage.
        }

        public VisitFailedMessage(short messageVersion) : base(messageVersion)
        {
            // VisitFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_reason = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_reason);
        }

        public override short GetMessageType()
        {
            return VisitFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetReason()
        {
            return this.m_reason;
        }

        public void SetReason(int value)
        {
            this.m_reason = value;
        }
    }
}