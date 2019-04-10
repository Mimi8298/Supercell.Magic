namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AllianceStreamMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24311;

        private LogicArrayList<StreamEntry> m_streamEntryList;

        public AllianceStreamMessage() : this(0)
        {
            // AllianceStreamMessage.
        }

        public AllianceStreamMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceStreamMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int arraySize = this.m_stream.ReadInt();

            if (arraySize > 0)
            {
                this.m_streamEntryList = new LogicArrayList<StreamEntry>(arraySize);

                do
                {
                    StreamEntry streamEntry = StreamEntryFactory.CreateStreamEntryByType((StreamEntryType) this.m_stream.ReadInt());
                    streamEntry.Decode(this.m_stream);
                    this.m_streamEntryList.Add(streamEntry);
                } while (--arraySize > 0);
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(0);

            if (this.m_streamEntryList != null)
            {
                this.m_stream.WriteInt(this.m_streamEntryList.Size());

                for (int i = 0; i < this.m_streamEntryList.Size(); i++)
                {
                    this.m_stream.WriteInt((int) this.m_streamEntryList[i].GetStreamEntryType());
                    this.m_streamEntryList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return AllianceStreamMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_streamEntryList = null;
        }

        public LogicArrayList<StreamEntry> RemovestreamEntries()
        {
            LogicArrayList<StreamEntry> tmp = this.m_streamEntryList;
            this.m_streamEntryList = null;
            return tmp;
        }

        public void SetStreamEntries(LogicArrayList<StreamEntry> array)
        {
            this.m_streamEntryList = array;
        }
    }
}