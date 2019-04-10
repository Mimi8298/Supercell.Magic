namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Titan.Message;

    public class AllianceWarDataFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24337;

        public const int WAR_DATA_ERROR_INTERNAL = 2;
        public const int WAR_DATA_ERROR_INVALID_WAR = 1;
        public const int WAR_DATA_ERROR_ERROR_NO_LONGER_AVAILABLE = 0;

        private int m_errorCode;

        public AllianceWarDataFailedMessage() : this(0)
        {
            // AllianceWarDataFailedMessage.
        }

        public AllianceWarDataFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceWarDataFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_errorCode = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteInt(this.m_errorCode);
        }

        public override short GetMessageType()
        {
            return AllianceWarDataFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public int GetErrorCode()
        {
            return this.m_errorCode;
        }

        public void SetErrorCode(int value)
        {
            this.m_errorCode = value;
        }
    }
}