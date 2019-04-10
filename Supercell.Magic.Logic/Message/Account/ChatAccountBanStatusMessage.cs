namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class ChatAccountBanStatusMessage : PiranhaMessage
    {
        private int m_banSecs;

        public ChatAccountBanStatusMessage() : this(0)
        {
            // ChatAccountBanStatusMessage.
        }

        public ChatAccountBanStatusMessage(short messageVersion) : base(messageVersion)
        {
            // ChatAccountBanStatusMessage.
        }

        public override void Decode()
        {
            base.Decode();
        }

        public override void Encode()
        {
            base.Encode();
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetBanSeconds()
        {
            return this.m_banSecs;
        }

        public void SetBanSeconds(int value)
        {
            this.m_banSecs = value;
        }
    }
}