namespace Supercell.Magic.Titan.Message.Security
{
    public class ServerHelloMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20100;

        private byte[] m_serverNonce;

        public ServerHelloMessage() : this(0)
        {
            // ServerHelloMessage.
        }

        public ServerHelloMessage(short messageVersion) : base(messageVersion)
        {
            // ServerHelloMessage.
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteBytes(this.m_serverNonce, this.m_serverNonce.Length);
        }

        public override void Decode()
        {
            base.Decode();
            this.m_serverNonce = this.m_stream.ReadBytes(this.m_stream.ReadBytesLength(), 24);
        }

        public byte[] RemoveServerNonce()
        {
            byte[] tmp = this.m_serverNonce;
            this.m_serverNonce = null;
            return tmp;
        }

        public void SetServerNonce(byte[] value)
        {
            this.m_serverNonce = value;
        }

        public override short GetMessageType()
        {
            return ServerHelloMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            this.m_serverNonce = null;
        }
    }
}