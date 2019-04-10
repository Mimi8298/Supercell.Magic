namespace Supercell.Magic.Titan.Message.Security
{
    public class CryptoErrorMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 29997;

        private int m_errorCode;

        public CryptoErrorMessage() : this(0)
        {
            // CryptoErrorMessage.
        }

        public CryptoErrorMessage(short messageVersion) : base(messageVersion)
        {
            // CryptoErrorMessage.
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteVInt(this.m_errorCode);
        }

        public override void Decode()
        {
            base.Decode();
            this.m_errorCode = this.m_stream.ReadVInt();
        }

        public override short GetMessageType()
        {
            return CryptoErrorMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}