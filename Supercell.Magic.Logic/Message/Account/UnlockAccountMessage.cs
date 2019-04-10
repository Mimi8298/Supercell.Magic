namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class UnlockAccountMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10121;

        private LogicLong m_accountId;

        private string m_passToken;
        private string m_unlockCode;

        public UnlockAccountMessage() : this(0)
        {
            // UnlockAccountMessage.
        }

        public UnlockAccountMessage(short messageVersion) : base(messageVersion)
        {
            // UnlockAccountMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_accountId = this.m_stream.ReadLong();
            this.m_passToken = this.m_stream.ReadString(900000);
            this.m_unlockCode = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_accountId);
            this.m_stream.WriteString(this.m_passToken);
            this.m_stream.WriteString(this.m_unlockCode);
        }

        public override short GetMessageType()
        {
            return UnlockAccountMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_accountId = null;
            this.m_passToken = null;
            this.m_unlockCode = null;
        }

        public LogicLong GetAccountId()
        {
            return this.m_accountId;
        }

        public void SetAccountId(LogicLong id)
        {
            this.m_accountId = id;
        }

        public string GetPassToken()
        {
            return this.m_passToken;
        }

        public void SetPassToken(string value)
        {
            this.m_passToken = value;
        }

        public string GetUnlockCode()
        {
            return this.m_unlockCode;
        }

        public void SetUnlockCode(string value)
        {
            this.m_unlockCode = value;
        }
    }
}