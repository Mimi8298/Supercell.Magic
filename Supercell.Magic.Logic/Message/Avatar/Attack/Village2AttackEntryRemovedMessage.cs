namespace Supercell.Magic.Logic.Message.Avatar.Attack
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class Village2AttackEntryRemovedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24373;
        private LogicLong m_streamId;

        public Village2AttackEntryRemovedMessage() : this(0)
        {
            // Village2AttackEntryRemovedMessage.
        }

        public Village2AttackEntryRemovedMessage(short messageVersion) : base(messageVersion)
        {
            // Village2AttackEntryRemovedMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_streamId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_streamId);
        }

        public override short GetMessageType()
        {
            return Village2AttackEntryRemovedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong GetStreamId()
        {
            return this.m_streamId;
        }

        public void SetStreamId(LogicLong id)
        {
            this.m_streamId = id;
        }
    }
}