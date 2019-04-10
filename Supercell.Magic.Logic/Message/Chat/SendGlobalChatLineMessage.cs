namespace Supercell.Magic.Logic.Message.Chat
{
    using Supercell.Magic.Titan.Message;

    public class SendGlobalChatLineMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14715;

        private string m_message;

        public SendGlobalChatLineMessage() : this(0)
        {
            // SendGlobalChatLineMessage.
        }

        public SendGlobalChatLineMessage(short messageVersion) : base(messageVersion)
        {
            // SendGlobalChatLineMessage.
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
            return SendGlobalChatLineMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 6;
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