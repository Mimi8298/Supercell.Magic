namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class RespondToAllianceJoinRequestMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14321;

        private LogicLong m_streamEntryId;
        private bool m_accepted;

        public RespondToAllianceJoinRequestMessage() : this(0)
        {
            // RespondToAllianceJoinRequestMessage.
        }

        public RespondToAllianceJoinRequestMessage(short messageVersion) : base(messageVersion)
        {
            // RespondToAllianceJoinRequestMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_streamEntryId = this.m_stream.ReadLong();
            this.m_accepted = this.m_stream.ReadBoolean();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_streamEntryId);
            this.m_stream.WriteBoolean(this.m_accepted);
        }

        public override short GetMessageType()
        {
            return RespondToAllianceJoinRequestMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public LogicLong GetStreamEntryId()
        {
            return this.m_streamEntryId;
        }

        public void SetStreamEntryId(LogicLong value)
        {
            this.m_streamEntryId = value;
        }

        public bool IsAccepted()
        {
            return this.m_accepted;
        }

        public void SetAccepted(bool value)
        {
            this.m_accepted = value;
        }
    }
}