namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class PersonalBreakStartedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20171;

        private int m_secondsUntilBreak;

        public PersonalBreakStartedMessage() : this(0)
        {
            // PersonalBreakStartedMessage.
        }

        public PersonalBreakStartedMessage(short messageVersion) : base(messageVersion)
        {
            // PersonalBreakStartedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_secondsUntilBreak = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_secondsUntilBreak);
        }

        public override short GetMessageType()
        {
            return PersonalBreakStartedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetSecondsUntilBreak()
        {
            return this.m_secondsUntilBreak;
        }

        public void SetSecondsUntilBreak(int value)
        {
            this.m_secondsUntilBreak = value;
        }
    }
}