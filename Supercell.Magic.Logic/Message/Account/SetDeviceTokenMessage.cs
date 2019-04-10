namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;

    public class SetDeviceTokenMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10113;

        private byte[] m_deviceToken;
        private int m_deviceTokenLength;

        public SetDeviceTokenMessage() : this(0)
        {
            // SetDeviceTokenMessage.
        }

        public SetDeviceTokenMessage(short messageVersion) : base(messageVersion)
        {
            // SetDeviceTokenMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_deviceTokenLength = this.m_stream.ReadBytesLength();

            if (this.m_deviceTokenLength > 1000)
            {
                Debugger.Error("Illegal byte array length encountered.");
            }

            this.m_deviceToken = this.m_stream.ReadBytes(this.m_deviceTokenLength, 900000);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteBytes(this.m_deviceToken, this.m_deviceTokenLength);
        }

        public override short GetMessageType()
        {
            return SetDeviceTokenMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_deviceToken = null;
        }

        public byte[] GetDeviceToken()
        {
            return this.m_deviceToken;
        }

        public int GetDeviceTokenLength()
        {
            return this.m_deviceTokenLength;
        }

        public void SetDeviceToken(byte[] value, int length)
        {
            this.m_deviceToken = value;
            this.m_deviceTokenLength = length;
        }
    }
}