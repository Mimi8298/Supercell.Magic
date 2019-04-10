namespace Supercell.Magic.Titan.Message.Account
{
    public class TitanDisconnectedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 25892;

        private int m_reason;

        public TitanDisconnectedMessage() : this(0)
        {
            // TitanDisconnectedMessage.
        }

        public TitanDisconnectedMessage(short messageVersion) : base(messageVersion)
        {
            // TitanDisconnectedMessage.
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
            return TitanDisconnectedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
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