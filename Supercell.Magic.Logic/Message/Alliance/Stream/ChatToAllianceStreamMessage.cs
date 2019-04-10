namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.Message;

    public class ChatToAllianceStreamMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14315;

        private string m_message;

        public ChatToAllianceStreamMessage() : this(0)
        {
            // ChatToAllianceStreamMessage.
        }

        public ChatToAllianceStreamMessage(short messageVersion) : base(messageVersion)
        {
            // ChatToAllianceStreamMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_message = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteString(this.m_message);
        }

        public override short GetMessageType()
        {
            return ChatToAllianceStreamMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_message = null;
        }

        public string RemoveMessage()
        {
            string tmp = this.m_message;
            this.m_message = null;
            return tmp;
        }

        public void SetMessage(string message)
        {
            this.m_message = message;
        }
    }
}