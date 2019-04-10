namespace Supercell.Magic.Titan.Message.Security
{
    public class ClientHelloMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10100;

        private int m_protocol;
        private int m_keyVersion;
        private int m_majorVersion;
        private int m_minorVersion;
        private int m_buildVersion;
        private int m_deviceType;
        private int m_appStore;

        private string m_contentHash;

        public ClientHelloMessage() : this(0)
        {
            // ClientHelloMessage.
        }

        public ClientHelloMessage(short messageVersion) : base(messageVersion)
        {
            this.m_contentHash = string.Empty;
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_protocol);
            this.m_stream.WriteInt(this.m_keyVersion);
            this.m_stream.WriteInt(this.m_majorVersion);
            this.m_stream.WriteInt(this.m_minorVersion);
            this.m_stream.WriteInt(this.m_buildVersion);
            this.m_stream.WriteStringReference(this.m_contentHash);
            this.m_stream.WriteInt(this.m_deviceType);
            this.m_stream.WriteInt(this.m_appStore);
        }

        public override void Decode()
        {
            base.Decode();

            this.m_protocol = this.m_stream.ReadInt();
            this.m_keyVersion = this.m_stream.ReadInt();
            this.m_majorVersion = this.m_stream.ReadInt();
            this.m_minorVersion = this.m_stream.ReadInt();
            this.m_buildVersion = this.m_stream.ReadInt();
            this.m_contentHash = this.m_stream.ReadStringReference(900000);
            this.m_deviceType = this.m_stream.ReadInt();
            this.m_appStore = this.m_stream.ReadInt();
        }

        public override short GetMessageType()
        {
            return ClientHelloMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_contentHash = null;
        }

        public int GetProtocol()
        {
            return this.m_protocol;
        }

        public void SetProtocol(int value)
        {
            this.m_protocol = value;
        }

        public int GetKeyVersion()
        {
            return this.m_keyVersion;
        }

        public void SetKeyVersion(int value)
        {
            this.m_keyVersion = value;
        }

        public int GetMajorVersion()
        {
            return this.m_majorVersion;
        }

        public void SetMajorVersion(int value)
        {
            this.m_majorVersion = value;
        }

        public int GetMinorVersion()
        {
            return this.m_minorVersion;
        }

        public void SetMinorVersion(int value)
        {
            this.m_minorVersion = value;
        }

        public int GetBuildVersion()
        {
            return this.m_buildVersion;
        }

        public void SetBuildVersion(int value)
        {
            this.m_buildVersion = value;
        }

        public string GetContentHash()
        {
            return this.m_contentHash;
        }

        public void SetContentHash(string value)
        {
            this.m_contentHash = value;
        }

        public int GetDeviceType()
        {
            return this.m_deviceType;
        }

        public void SetDeviceType(int value)
        {
            this.m_deviceType = value;
        }

        public int GetAppStore()
        {
            return this.m_appStore;
        }

        public void SetAppStore(int value)
        {
            this.m_appStore = value;
        }
    }
}