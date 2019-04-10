namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class UnlockAccountFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20133;
        
        private ErrorCode m_errorCode;

        public UnlockAccountFailedMessage() : this(0)
        {
            // UnlockAccountFailedMessage.
        }

        public UnlockAccountFailedMessage(short messageVersion) : base(messageVersion)
        {
            // UnlockAccountFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_errorCode = (ErrorCode) this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt((int) this.m_errorCode);
        }

        public override short GetMessageType()
        {
            return UnlockAccountFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public ErrorCode GetErrorCode()
        {
            return this.m_errorCode;
        }

        public void SetErrorCode(ErrorCode errorCode)
        {
            this.m_errorCode = errorCode;
        }

        public enum ErrorCode
        {
            UNLOCK_FAILED = 4,
            UNLOCK_UNAVAILABLE = 5,
            SERVER_MAINTENANCE = 10
        }
    }
}