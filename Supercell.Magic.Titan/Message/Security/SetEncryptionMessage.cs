namespace Supercell.Magic.Titan.Message.Security
{
    public class SetEncryptionMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20000;

        private byte[] m_nonce;

        public SetEncryptionMessage() : this(0)
        {
            // SetEncryptionMessage.
        }

        public SetEncryptionMessage(short messageVersion) : base(messageVersion)
        {
            // SetEncryptionMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_nonce = this.m_stream.ReadBytes(this.m_stream.ReadBytesLength(), 900000);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteBytes(this.m_nonce, this.m_nonce.Length);
        }

        public override short GetMessageType()
        {
            return SetEncryptionMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_nonce = null;
        }

        public byte[] RemoveNonce()
        {
            byte[] tmp = this.m_nonce;
            this.m_nonce = null;
            return tmp;
        }

        public void SetNonce(byte[] value)
        {
            this.m_nonce = value;
        }
    }
}