namespace Supercell.Magic.Logic.Message.Google
{
    using Supercell.Magic.Titan.Message;

    public class BindGoogleServiceAccountMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14262;

        private bool m_force;
        private string m_googleServiceId;
        private string m_accessToken;

        public BindGoogleServiceAccountMessage() : this(0)
        {
            // BindGoogleServiceAccountMessage.
        }

        public BindGoogleServiceAccountMessage(short messageVersion) : base(messageVersion)
        {
            // BindGoogleServiceAccountMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_force = this.m_stream.ReadBoolean();
            this.m_googleServiceId = this.m_stream.ReadString(900000);
            this.m_accessToken = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteBoolean(this.m_force);
            this.m_stream.WriteString(this.m_googleServiceId);
            this.m_stream.WriteString(this.m_accessToken);
        }

        public override short GetMessageType()
        {
            return BindGoogleServiceAccountMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_googleServiceId = null;
            this.m_accessToken = null;
        }

        public string RemoveGoogleServiceId()
        {
            string tmp = this.m_googleServiceId;
            this.m_googleServiceId = null;
            return tmp;
        }

        public void SetGoogleServiceId(string value)
        {
            this.m_googleServiceId = value;
        }

        public string RemoveAccessToken()
        {
            string tmp = this.m_accessToken;
            this.m_accessToken = null;
            return tmp;
        }

        public void SetAccessToken(string value)
        {
            this.m_accessToken = value;
        }
    }
}