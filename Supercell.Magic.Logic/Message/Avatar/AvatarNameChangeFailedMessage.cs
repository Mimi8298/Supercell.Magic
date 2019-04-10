namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Message;

    public class AvatarNameChangeFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20205;
        
        private ErrorCode m_errorCode;

        public AvatarNameChangeFailedMessage() : this(0)
        {
            // AvatarNameChangeFailedMessage.
        }

        public AvatarNameChangeFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarNameChangeFailedMessage.
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
            return AvatarNameChangeFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
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
            TOO_LONG = 1,
            TOO_SHORT = 2,
            BAD_WORD = 3,
            ALREADY_CHANGED = 4,
            TH_LEVEL_TOO_LOW = 5
        }
    }
}