namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Message;

    public class AvatarNameCheckResponseMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20300;
        
        private bool m_invalid;
        private ErrorCode m_errorCode;
        private string m_name;

        public AvatarNameCheckResponseMessage() : this(0)
        {
            // AvatarNameCheckResponseMessage.
        }

        public AvatarNameCheckResponseMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarNameCheckResponseMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_invalid = this.m_stream.ReadBoolean();
            this.m_errorCode = (ErrorCode) this.m_stream.ReadInt();
            this.m_name = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteBoolean(this.m_invalid);
            this.m_stream.WriteInt((int) this.m_errorCode);
            this.m_stream.WriteString(this.m_name);
        }

        public override short GetMessageType()
        {
            return AvatarNameCheckResponseMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_name = null;
        }

        public string GetName()
        {
            return this.m_name;
        }

        public void SetName(string name)
        {
            this.m_name = name;
        }

        public bool IsInvalid()
        {
            return this.m_invalid;
        }

        public void SetInvalid(bool invalid)
        {
            this.m_invalid = invalid;
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
            INVALID_NAME = 1,
            NAME_TOO_SHORT = 2,
            NAME_TOO_LONG = 3,
            NAME_ALREADY_CHANGED = 4,
            NAME_TH_LEVEL_TOO_LOW = 5
        }
    }
}