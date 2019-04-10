namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class JoinAllianceMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14305;

        private LogicLong m_allianceId;

        public JoinAllianceMessage() : this(0)
        {
            // JoinAllianceMessage.
        }

        public JoinAllianceMessage(short messageVersion) : base(messageVersion)
        {
            // JoinAllianceMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_allianceId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_allianceId);
        }

        public override short GetMessageType()
        {
            return JoinAllianceMessage.MESSAGE_TYPE;
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
    }
}