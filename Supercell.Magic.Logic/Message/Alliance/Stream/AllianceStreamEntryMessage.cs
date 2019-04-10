namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.Message;

    public class AllianceStreamEntryMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24312;

        private StreamEntry m_streamEntry;

        public AllianceStreamEntryMessage() : this(0)
        {
            // AllianceStreamEntryMessage.
        }

        public AllianceStreamEntryMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceStreamEntryMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_streamEntry = StreamEntryFactory.CreateStreamEntryByType((StreamEntryType) this.m_stream.ReadInt());
            this.m_streamEntry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt((int) this.m_streamEntry.GetStreamEntryType());
            this.m_streamEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return AllianceStreamEntryMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_streamEntry = null;
        }

        public StreamEntry GetStreamEntryId()
        {
            return this.m_streamEntry;
        }

        public void SetStreamEntry(StreamEntry value)
        {
            this.m_streamEntry = value;
        }
    }
}