namespace Supercell.Magic.Logic.Message.Google
{
    using Supercell.Magic.Titan.Message;

    public class GoogleServiceAccountBoundMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20261;

        private int m_resultCode;

        public GoogleServiceAccountBoundMessage() : this(0)
        {
            // GoogleServiceAccountBoundMessage.
        }

        public GoogleServiceAccountBoundMessage(short messageVersion) : base(messageVersion)
        {
            // GoogleServiceAccountBoundMessage.
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
            return 24261;
        }

        public override int GetServiceNodeType()
        {
            return 1;
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