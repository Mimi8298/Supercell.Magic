namespace Supercell.Magic.Logic.Message.Friend
{
    using Supercell.Magic.Titan.Message;

    public class AddFriendErrorMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20112;
        private ErrorCode m_errorCode;

        public AddFriendErrorMessage() : this(0)
        {
            // AddFriendErrorMessage.
        }

        public AddFriendErrorMessage(short messageVersion) : base(messageVersion)
        {
            // AddFriendErrorMessage.
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
            return AddFriendErrorMessage.MESSAGE_TYPE;
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
            GENERIC = 0,
            TOO_MANY_REQUESTS_YOU = 1,
            TOO_MANY_REQUESTS_OTHER = 2,
            OWN_AVATAR = 4,
            DOES_NOT_EXIST = 5,
            TOO_MANY_FRIENDS_YOU = 7,
            TOO_MANY_FRIENDS_OTHERS = 8
        }
    }
}