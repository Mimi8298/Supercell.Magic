namespace Supercell.Magic.Logic.Message.Google
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class GoogleServiceAccountAlreadyBoundMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20262;

        private LogicLong m_accountId;
        private LogicClientAvatar m_avatar;

        private string m_googleServiceId;
        private string m_passToken;

        public GoogleServiceAccountAlreadyBoundMessage() : this(0)
        {
            // GoogleServiceAccountAlreadyBoundMessage.
        }

        public GoogleServiceAccountAlreadyBoundMessage(short messageVersion) : base(messageVersion)
        {
            // GoogleServiceAccountAlreadyBoundMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_googleServiceId = this.m_stream.ReadString(900000);

            if (this.m_stream.ReadBoolean())
            {
                this.m_accountId = this.m_stream.ReadLong();
            }

            this.m_passToken = this.m_stream.ReadString(900000);
            this.m_avatar = new LogicClientAvatar();
            this.m_avatar.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_googleServiceId);

            if (!this.m_accountId.IsZero())
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_accountId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }

            this.m_stream.WriteString(this.m_passToken);
            this.m_avatar.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return 24262;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_googleServiceId = null;
            this.m_passToken = null;
            this.m_avatar = null;
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

        public string RemovePassToken()
        {
            string tmp = this.m_passToken;
            this.m_passToken = null;
            return tmp;
        }

        public void SetPassToken(string value)
        {
            this.m_passToken = value;
        }

        public LogicLong RemoveAccountId()
        {
            LogicLong tmp = this.m_accountId;
            this.m_accountId = null;
            return tmp;
        }

        public void SetAccountId(LogicLong value)
        {
            this.m_accountId = value;
        }

        public LogicClientAvatar RemoveLogicClientAvatar()
        {
            LogicClientAvatar tmp = this.m_avatar;
            this.m_avatar = null;
            return tmp;
        }

        public void SetAvatar(LogicClientAvatar avatar)
        {
            this.m_avatar = avatar;
        }
    }
}