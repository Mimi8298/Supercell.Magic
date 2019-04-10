namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Message;

    public class CreateAllianceMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14301;

        private LogicData m_originData;

        private string m_allianceName;
        private string m_allianceDescription;

        private int m_allianceType;
        private int m_allianceBadgeId;
        private int m_requiredScore;
        private int m_requiredDuelScore;
        private int m_warFrequency;

        private bool m_publicWarLog;
        private bool m_amicalWarEnabled;

        public CreateAllianceMessage() : this(0)
        {
            // CreateAllianceMessage.
        }

        public CreateAllianceMessage(short messageVersion) : base(messageVersion)
        {
            // CreateAllianceMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_allianceName = this.m_stream.ReadString(900000);
            this.m_allianceDescription = this.m_stream.ReadString(900000);
            this.m_allianceBadgeId = this.m_stream.ReadInt();
            this.m_allianceType = this.m_stream.ReadInt();
            this.m_requiredScore = this.m_stream.ReadInt();
            this.m_requiredDuelScore = this.m_stream.ReadInt();
            this.m_warFrequency = this.m_stream.ReadInt();
            this.m_originData = ByteStreamHelper.ReadDataReference(this.m_stream);
            this.m_publicWarLog = this.m_stream.ReadBoolean();
            this.m_amicalWarEnabled = this.m_stream.ReadBoolean();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_allianceName);
            this.m_stream.WriteString(this.m_allianceDescription);
            this.m_stream.WriteInt(this.m_allianceBadgeId);
            this.m_stream.WriteInt(this.m_allianceType);
            this.m_stream.WriteInt(this.m_requiredScore);
            this.m_stream.WriteInt(this.m_requiredDuelScore);
            this.m_stream.WriteInt(this.m_warFrequency);
            ByteStreamHelper.WriteDataReference(this.m_stream, this.m_originData);
            this.m_stream.WriteBoolean(this.m_publicWarLog);
            this.m_stream.WriteBoolean(this.m_amicalWarEnabled);
        }

        public override short GetMessageType()
        {
            return CreateAllianceMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
        }

        public string GetAllianceDescription()
        {
            return this.m_allianceDescription;
        }

        public void SetAllianceDescription(string value)
        {
            this.m_allianceDescription = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public int GetAllianceType()
        {
            return this.m_allianceType;
        }

        public void SetAllianceType(int value)
        {
            this.m_allianceType = value;
        }

        public int GetRequiredScore()
        {
            return this.m_requiredScore;
        }

        public void SetRequiredScore(int value)
        {
            this.m_requiredScore = value;
        }

        public int GetRequiredDuelScore()
        {
            return this.m_requiredDuelScore;
        }

        public void SetRequiredDuelScore(int value)
        {
            this.m_requiredDuelScore = value;
        }

        public LogicData GetOriginData()
        {
            return this.m_originData;
        }

        public void SetOriginData(LogicData data)
        {
            this.m_originData = data;
        }

        public int GetWarFrequency()
        {
            return this.m_warFrequency;
        }

        public bool GetArrangedWarEnabled()
        {
            return this.m_amicalWarEnabled;
        }

        public void SetAmicalWarEnabled(bool enabled)
        {
            this.m_amicalWarEnabled = enabled;
        }

        public bool GetPublicWarLog()
        {
            return this.m_publicWarLog;
        }

        public void SetPublicWarLog(bool enabled)
        {
            this.m_publicWarLog = enabled;
        }
    }
}