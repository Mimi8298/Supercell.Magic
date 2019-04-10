namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class ReportUserMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10117;

        private LogicLong m_reportedAvatarId;

        public ReportUserMessage() : this(0)
        {
            // ReportUserMessage.
        }

        public ReportUserMessage(short messageVersion) : base(messageVersion)
        {
            // ReportUserMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_reportedAvatarId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_reportedAvatarId);
        }

        public override short GetMessageType()
        {
            return ReportUserMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong RemoveReportedAvatarId()
        {
            LogicLong tmp = this.m_reportedAvatarId;
            this.m_reportedAvatarId = null;
            return tmp;
        }

        public void SetReportedAvatarId(LogicLong value)
        {
            this.m_reportedAvatarId = value;
        }
    }
}