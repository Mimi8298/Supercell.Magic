namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AccountSwitchedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10118;

        private LogicLong m_switchedToAccountId;

        public AccountSwitchedMessage() : this(0)
        {
            // AccountSwitchedMessage.
        }

        public AccountSwitchedMessage(short messageVersion) : base(messageVersion)
        {
            // AccountSwitchedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_switchedToAccountId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_switchedToAccountId);
        }

        public override short GetMessageType()
        {
            return AccountSwitchedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong RemoveSwitchedToAccountId()
        {
            LogicLong tmp = this.m_switchedToAccountId;
            this.m_switchedToAccountId = null;
            return tmp;
        }

        public void SetSwitchedToAccountId(LogicLong id)
        {
            this.m_switchedToAccountId = id;
        }
    }
}