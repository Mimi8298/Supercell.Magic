namespace Supercell.Magic.Logic.Message.Facebook
{
    using Supercell.Magic.Titan.Message;

    public class BindFacebookAccountMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14201;

        private bool m_force;
        private string m_googleServiceId;
        private string m_authToken;

        public BindFacebookAccountMessage() : this(0)
        {
            // BindFacebookAccountMessage.
        }

        public BindFacebookAccountMessage(short messageVersion) : base(messageVersion)
        {
            // BindFacebookAccountMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_force = this.m_stream.ReadBoolean();
            this.m_googleServiceId = this.m_stream.ReadString(900000);
            this.m_authToken = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteBoolean(this.m_force);
            this.m_stream.WriteString(this.m_googleServiceId);
            this.m_stream.WriteString(this.m_authToken);
        }

        public override short GetMessageType()
        {
            return BindFacebookAccountMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_googleServiceId = null;
            this.m_authToken = null;
        }

        public string RemoveFacebookId()
        {
            string tmp = this.m_googleServiceId;
            this.m_googleServiceId = null;
            return tmp;
        }

        public void SetFacebookId(string value)
        {
            this.m_googleServiceId = value;
        }

        public string RemoveAuthentificationToken()
        {
            string tmp = this.m_authToken;
            this.m_authToken = null;
            return tmp;
        }

        public void SetAuthentificationToken(string value)
        {
            this.m_authToken = value;
        }
    }
}