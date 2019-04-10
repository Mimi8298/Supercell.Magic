namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class ServerErrorMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24115;

        private string m_errorMessage;

        public ServerErrorMessage() : this(0)
        {
            // ServerErrorMessage.
        }

        public ServerErrorMessage(short messageVersion) : base(messageVersion)
        {
            // ServerErrorMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_errorMessage = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteString(this.m_errorMessage);
        }

        public override short GetMessageType()
        {
            return ServerErrorMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_errorMessage = null;
        }

        public string GetErrorMessage()
        {
            return this.m_errorMessage;
        }

        public void SetErrorMessage(string value)
        {
            this.m_errorMessage = value;
        }
    }
}