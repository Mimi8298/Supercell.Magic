namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class JoinableAllianceListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24304;

        private LogicArrayList<LogicLong> m_bookmarkList;
        private LogicArrayList<AllianceHeaderEntry> m_allianceList;

        public JoinableAllianceListMessage() : this(0)
        {
            // JoinableAllianceListMessage.
        }

        public JoinableAllianceListMessage(short messageVersion) : base(messageVersion)
        {
            // JoinableAllianceListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int arraySize = this.m_stream.ReadInt();

            if (arraySize <= 10000)
            {
                if (arraySize > -1)
                {
                    this.m_allianceList = new LogicArrayList<AllianceHeaderEntry>(arraySize);

                    for (int i = 0; i < arraySize; i++)
                    {
                        AllianceHeaderEntry allianceHeaderEntry = new AllianceHeaderEntry();
                        allianceHeaderEntry.Decode(this.m_stream);
                        this.m_allianceList.Add(allianceHeaderEntry);
                    }
                }
            }

            int array2Size = this.m_stream.ReadInt();

            if (array2Size <= 10000)
            {
                if (array2Size > -1)
                {
                    this.m_bookmarkList = new LogicArrayList<LogicLong>(array2Size);

                    for (int i = 0; i < array2Size; i++)
                    {
                        this.m_bookmarkList.Add(this.m_stream.ReadLong());
                    }
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_allianceList != null)
            {
                this.m_stream.WriteInt(this.m_allianceList.Size());

                for (int i = 0; i < this.m_allianceList.Size(); i++)
                {
                    this.m_allianceList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }

            if (this.m_bookmarkList != null)
            {
                this.m_stream.WriteInt(this.m_bookmarkList.Size());

                for (int i = 0; i < this.m_bookmarkList.Size(); i++)
                {
                    this.m_stream.WriteLong(this.m_bookmarkList[i]);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return JoinableAllianceListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_allianceList = null;
            this.m_bookmarkList = null;
        }

        public LogicArrayList<AllianceHeaderEntry> RemoveAlliances()
        {
            LogicArrayList<AllianceHeaderEntry> tmp = this.m_allianceList;
            this.m_allianceList = null;
            return tmp;
        }

        public void SetAlliances(LogicArrayList<AllianceHeaderEntry> alliances)
        {
            this.m_allianceList = alliances;
        }

        public void SetBookmarkList(LogicArrayList<LogicLong> list)
        {
            this.m_bookmarkList = list;
        }
    }
}