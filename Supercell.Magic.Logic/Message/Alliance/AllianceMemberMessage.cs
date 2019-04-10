namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceMemberMessage : PiranhaMessage
    {
        private AllianceMemberEntry m_allianceMemberEntry;

        public AllianceMemberMessage() : this(0)
        {
            // AllianceMemberMessage.
        }

        public AllianceMemberMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceMemberMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_allianceMemberEntry = new AllianceMemberEntry();
            this.m_allianceMemberEntry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_allianceMemberEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return 24308;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceMemberEntry = null;
        }

        public AllianceMemberEntry RemoveAllianceMemberEntry()
        {
            AllianceMemberEntry tmp = this.m_allianceMemberEntry;
            this.m_allianceMemberEntry = null;
            return tmp;
        }

        public void SetAllianceMemberEntry(AllianceMemberEntry value)
        {
            this.m_allianceMemberEntry = value;
        }
    }
}