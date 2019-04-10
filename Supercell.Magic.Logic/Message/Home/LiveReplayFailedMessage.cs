namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class LiveReplayFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24117;
        
        private Reason m_reason;

        public LiveReplayFailedMessage() : this(0)
        {
            // LiveReplayFailedMessage.
        }

        public LiveReplayFailedMessage(short messageVersion) : base(messageVersion)
        {
            // LiveReplayFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_reason = (Reason) this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt((int) this.m_reason);
        }

        public override short GetMessageType()
        {
            return LiveReplayFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public Reason GetReason()
        {
            return this.m_reason;
        }

        public void SetReason(Reason value)
        {
            this.m_reason = value;
        }

        public enum Reason
        {
            GENERIC,
            NO_DATA_FOUND,
            NO_FREE_SLOTS
        }
    }
}