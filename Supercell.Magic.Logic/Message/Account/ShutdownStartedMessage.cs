namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class ShutdownStartedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20161;

        private int m_secondsUntilShutdown;

        public ShutdownStartedMessage() : this(0)
        {
            // ShutdownStartedMessage.
        }

        public ShutdownStartedMessage(short messageVersion) : base(messageVersion)
        {
            // ShutdownStartedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_secondsUntilShutdown = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_secondsUntilShutdown);
        }

        public override short GetMessageType()
        {
            return ShutdownStartedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetSecondsUntilShutdown()
        {
            return this.m_secondsUntilShutdown;
        }

        public void SetSecondsUntilShutdown(int value)
        {
            this.m_secondsUntilShutdown = value;
        }
    }
}