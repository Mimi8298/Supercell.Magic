namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class ResetAccountMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10116;

        private int m_accountPreset;

        public ResetAccountMessage() : this(0)
        {
            // ResetAccountMessage.
        }

        public ResetAccountMessage(short messageVersion) : base(messageVersion)
        {
            // ResetAccountMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_accountPreset = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_accountPreset);
        }

        public override short GetMessageType()
        {
            return ResetAccountMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetAccountPreset()
        {
            return this.m_accountPreset;
        }

        public void SetAccountPreset(int value)
        {
            this.m_accountPreset = value;
        }
    }
}