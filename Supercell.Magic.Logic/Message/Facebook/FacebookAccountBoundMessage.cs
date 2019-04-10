namespace Supercell.Magic.Logic.Message.Facebook
{
    using Supercell.Magic.Titan.Message;

    public class FacebookAccountBoundMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24201;

        private int m_resultCode;

        public FacebookAccountBoundMessage() : this(0)
        {
            // FacebookAccountBoundMessage.
        }

        public FacebookAccountBoundMessage(short messageVersion) : base(messageVersion)
        {
            // FacebookAccountBoundMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_resultCode = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_resultCode);
        }

        public override short GetMessageType()
        {
            return FacebookAccountBoundMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetResultCode()
        {
            return this.m_resultCode;
        }

        public void SetResultCode(int value)
        {
            this.m_resultCode = value;
        }
    }
}