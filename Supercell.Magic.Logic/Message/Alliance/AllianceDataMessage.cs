namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24301;

        private AllianceFullEntry m_fullEntry;

        public AllianceDataMessage() : this(0)
        {
            // AllianceDataMessage.
        }

        public AllianceDataMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_fullEntry = new AllianceFullEntry();
            this.m_fullEntry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_fullEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return AllianceDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_fullEntry = null;
        }

        public AllianceFullEntry RemoveAllianceFullEntry()
        {
            AllianceFullEntry tmp = this.m_fullEntry;
            this.m_fullEntry = null;
            return tmp;
        }

        public void SetAllianceFullEntry(AllianceFullEntry entry)
        {
            this.m_fullEntry = entry;
        }
    }
}