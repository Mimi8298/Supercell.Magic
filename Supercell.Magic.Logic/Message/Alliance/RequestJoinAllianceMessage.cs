namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class RequestJoinAllianceMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14317;

        private LogicLong m_allianceId;
        private string m_message;

        public RequestJoinAllianceMessage() : this(0)
        {
            // RequestJoinAllianceMessage.
        }

        public RequestJoinAllianceMessage(short messageVersion) : base(messageVersion)
        {
            // RequestJoinAllianceMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_allianceId = this.m_stream.ReadLong();
            this.m_message = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_allianceId);
            this.m_stream.WriteString(this.m_message);
        }

        public override short GetMessageType()
        {
            return RequestJoinAllianceMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public LogicLong RemoveAllianceId()
        {
            LogicLong tmp = this.m_allianceId;
            this.m_allianceId = null;
            return tmp;
        }

        public void SetAllianceId(LogicLong id)
        {
            this.m_allianceId = id;
        }

        public string GetMessage()
        {
            return this.m_message;
        }

        public void SetMessage(string value)
        {
            this.m_message = value;
        }
    }
}