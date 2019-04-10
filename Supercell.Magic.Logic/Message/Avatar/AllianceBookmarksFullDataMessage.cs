namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AllianceBookmarksFullDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24341;
        private LogicArrayList<AllianceHeaderEntry> m_allianceList;

        public AllianceBookmarksFullDataMessage() : this(0)
        {
            // AllianceBookmarksFullDataMessage.
        }

        public AllianceBookmarksFullDataMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceBookmarksFullDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int count = this.m_stream.ReadInt();

            if (count >= 0)
            {
                this.m_allianceList = new LogicArrayList<AllianceHeaderEntry>(count);

                for (int i = 0; i < count; i++)
                {
                    AllianceHeaderEntry headerEntry = new AllianceHeaderEntry();
                    headerEntry.Decode(this.m_stream);
                    this.m_allianceList.Add(headerEntry);
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
        }

        public override short GetMessageType()
        {
            return AllianceBookmarksFullDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceList = null;
        }

        public LogicArrayList<AllianceHeaderEntry> GetAllianceList()
        {
            return this.m_allianceList;
        }

        public void SetAlliances(LogicArrayList<AllianceHeaderEntry> list)
        {
            this.m_allianceList = list;
        }
    }
}