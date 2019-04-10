namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class WaitingToGoHomeMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24112;

        private int m_estimatedTimeSeconds;

        public WaitingToGoHomeMessage() : this(0)
        {
            // WaitingToGoHomeMessage.
        }

        public WaitingToGoHomeMessage(short messageVersion) : base(messageVersion)
        {
            // WaitingToGoHomeMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_estimatedTimeSeconds = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_estimatedTimeSeconds);
        }

        public override short GetMessageType()
        {
            return WaitingToGoHomeMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetEstimatedTimeSeconds()
        {
            return this.m_estimatedTimeSeconds;
        }

        public void SetEstimatedTimeSeconds(int value)
        {
            this.m_estimatedTimeSeconds = value;
        }
    }
}