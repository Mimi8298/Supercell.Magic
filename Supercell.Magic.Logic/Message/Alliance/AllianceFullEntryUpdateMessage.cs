namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AllianceFullEntryUpdateMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24324;

        private int m_warState;
        private string m_description;

        private LogicLong m_currentWarId;
        private AllianceHeaderEntry m_headerEntry;

        public AllianceFullEntryUpdateMessage() : this(0)
        {
            // AllianceFullEntryUpdateMessage.
        }

        public AllianceFullEntryUpdateMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceFullEntryUpdateMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_description = this.m_stream.ReadString(1000);
            this.m_stream.ReadInt();
            this.m_stream.ReadInt();

            if (this.m_stream.ReadBoolean())
            {
                this.m_currentWarId = this.m_stream.ReadLong();
            }

            this.m_stream.ReadInt();

            if (this.m_stream.ReadBoolean())
            {
                this.m_stream.ReadLong();
            }

            this.m_headerEntry = new AllianceHeaderEntry();
            this.m_headerEntry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_description);
            this.m_stream.WriteInt(this.m_warState);
            this.m_stream.WriteInt(50);

            if (this.m_currentWarId != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_currentWarId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }

            this.m_stream.WriteInt(0);

            if (false)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(new LogicLong());
            }

            this.m_stream.WriteBoolean(false);

            this.m_headerEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return AllianceFullEntryUpdateMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_headerEntry = null;
        }

        public AllianceHeaderEntry RemoveAllianceHeaderEntry()
        {
            AllianceHeaderEntry tmp = this.m_headerEntry;
            this.m_headerEntry = null;
            return tmp;
        }

        public void SetAllianceHeaderEntry(AllianceHeaderEntry entry)
        {
            this.m_headerEntry = entry;
        }

        public string GetDescription()
        {
            return this.m_description;
        }

        public void SetDescription(string value)
        {
            this.m_description = value;
        }

        public LogicLong GetCurrentWarId()
        {
            return this.m_currentWarId;
        }

        public void SetCurrentWarId(LogicLong value)
        {
            this.m_currentWarId = value;
        }

        public int GetWarState()
        {
            return this.m_warState;
        }

        public void SetWarState(int value)
        {
            this.m_warState = value;
        }
    }
}