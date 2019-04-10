namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Message;

    public class OutOfSyncMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24104;

        private int m_subtick;
        private int m_clientChecksum;
        private int m_serverChecksum;

        private LogicJSONObject m_debugJSON;

        public OutOfSyncMessage() : this(0)
        {
            // OutOfSyncMessage.
        }

        public OutOfSyncMessage(short messageVersion) : base(messageVersion)
        {
            // OutOfSyncMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_serverChecksum = this.m_stream.ReadInt();
            this.m_clientChecksum = this.m_stream.ReadInt();
            this.m_subtick = this.m_stream.ReadInt();

            if (this.m_stream.ReadBoolean())
            {
                string json = this.m_stream.ReadString(900000);

                if (json != null)
                {
                    this.m_debugJSON = LogicJSONParser.ParseObject(json);
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_serverChecksum);
            this.m_stream.WriteInt(this.m_clientChecksum);
            this.m_stream.WriteInt(this.m_subtick);

            if (this.m_debugJSON != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteString(LogicJSONParser.CreateJSONString(this.m_debugJSON, 1024));
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }
        }

        public override short GetMessageType()
        {
            return OutOfSyncMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_debugJSON = null;
        }

        public int GetServerChecksum()
        {
            return this.m_serverChecksum;
        }

        public void SetServerChecksum(int value)
        {
            this.m_serverChecksum = value;
        }

        public int GetClientChecksum()
        {
            return this.m_clientChecksum;
        }

        public void SetClientChecksum(int value)
        {
            this.m_clientChecksum = value;
        }

        public int GetSubTick()
        {
            return this.m_subtick;
        }

        public void SetSubTick(int value)
        {
            this.m_subtick = value;
        }

        public LogicJSONObject RemoveDebugJSON()
        {
            LogicJSONObject tmp = this.m_debugJSON;
            this.m_debugJSON = null;
            return tmp;
        }

        public void SetDebugJSON(LogicJSONObject json)
        {
            this.m_debugJSON = json;
        }
    }
}