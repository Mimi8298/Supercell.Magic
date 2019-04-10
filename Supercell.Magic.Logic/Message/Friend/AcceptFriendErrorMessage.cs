namespace Supercell.Magic.Logic.Message.Friend
{
    using Supercell.Magic.Titan.Message;

    public class AcceptFriendErrorMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20501;

        private ErrorCode m_errorCode;

        public AcceptFriendErrorMessage() : this(0)
        {
            // AcceptFriendErrorMessage.
        }

        public AcceptFriendErrorMessage(short messageVersion) : base(messageVersion)
        {
            // AcceptFriendErrorMessage.
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
            return AcceptFriendErrorMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 3;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public ErrorCode GetErrorCode()
        {
            return this.m_errorCode;
        }

        public void SetErrorCode(ErrorCode value)
        {
            this.m_errorCode = value;
        }

        public enum ErrorCode
        {
            GENERIC,
            BANNED,
            TOO_MANY_FRIENDS
        }
    }
}