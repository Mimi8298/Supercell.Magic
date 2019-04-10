namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AllianceStreamEntryRemovedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24318;

        private LogicLong m_streamEntryId;

        public AllianceStreamEntryRemovedMessage() : this(0)
        {
            // AllianceStreamEntryRemovedMessage.
        }

        public AllianceStreamEntryRemovedMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceStreamEntryRemovedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_streamEntryId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_streamEntryId);
        }

        public override short GetMessageType()
        {
            return AllianceStreamEntryRemovedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_streamEntryId = null;
        }

        public LogicLong GetStreamEntryId()
        {
            return this.m_streamEntryId;
        }

        public void SetStreamEntryId(LogicLong value)
        {
            this.m_streamEntryId = value;
        }
    }
}