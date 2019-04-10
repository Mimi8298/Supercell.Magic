namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AvatarProfileFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24339;
        
        private ErrorType m_errorType;
        private LogicLong m_avatarId;

        public AvatarProfileFailedMessage() : this(0)
        {
            // AvatarProfileFailedMessage.
        }

        public AvatarProfileFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarProfileFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_errorType = (ErrorType) this.m_stream.ReadInt();
            this.m_avatarId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt((int) this.m_errorType);
            this.m_stream.WriteLong(this.m_avatarId);
        }

        public override short GetMessageType()
        {
            return AvatarProfileFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_avatarId = null;
        }

        public ErrorType GetErrorType()
        {
            return this.m_errorType;
        }

        public void SetErrorType(ErrorType value)
        {
            this.m_errorType = value;
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong value)
        {
            this.m_avatarId = value;
        }

        public enum ErrorType
        {
            GENERIC,
            INTERNAL_ERROR,
            NOT_FOUND
        }
    }
}