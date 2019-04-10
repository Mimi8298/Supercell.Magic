namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Message;

    public class SearchAlliancesMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14324;

        private string m_searchString;
        private bool m_openOnly;

        private int m_warFrequency;
        private int m_minMemberCount;
        private int m_maxMemberCount;
        private int m_requiredScore;
        private int m_requiredDuelScore;
        private int m_minExpLevel;

        private LogicData m_originData;

        public SearchAlliancesMessage() : this(0)
        {
            // SearchAlliancesMessage.
        }

        public SearchAlliancesMessage(short messageVersion) : base(messageVersion)
        {
            // SearchAlliancesMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_searchString = this.m_stream.ReadString(900000);
            this.m_warFrequency = this.m_stream.ReadInt();
            this.m_originData = ByteStreamHelper.ReadDataReference(this.m_stream, LogicDataType.REGION);
            this.m_minMemberCount = this.m_stream.ReadInt();
            this.m_maxMemberCount = this.m_stream.ReadInt();
            this.m_requiredScore = this.m_stream.ReadInt();
            this.m_requiredDuelScore = this.m_stream.ReadInt();
            this.m_openOnly = this.m_stream.ReadBoolean();

            this.m_stream.ReadInt();
            this.m_stream.ReadInt();

            this.m_minExpLevel = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_searchString);
            this.m_stream.WriteInt(this.m_warFrequency);
            ByteStreamHelper.WriteDataReference(this.m_stream, this.m_originData);
            this.m_stream.WriteInt(this.m_minMemberCount);
            this.m_stream.WriteInt(this.m_maxMemberCount);
            this.m_stream.WriteInt(this.m_requiredScore);
            this.m_stream.WriteInt(this.m_requiredDuelScore);
            this.m_stream.WriteBoolean(this.m_openOnly);
            this.m_stream.WriteInt(0);
            this.m_stream.WriteInt(0);
            this.m_stream.WriteInt(this.m_minExpLevel);
        }

        public override short GetMessageType()
        {
            return SearchAlliancesMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public string GetSearchString()
        {
            return this.m_searchString;
        }

        public void SetSearchString(string value)
        {
            this.m_searchString = value;
        }

        public int GetWarFrequency()
        {
            return this.m_warFrequency;
        }

        public void SetWarFrequency(int value)
        {
            this.m_warFrequency = value;
        }

        public LogicData GetOrigin()
        {
            return this.m_originData;
        }

        public void SetOrigin(LogicData data)
        {
            this.m_originData = data;
        }

        public int GetMinimumMembers()
        {
            return this.m_minMemberCount;
        }

        public void SetMinimumMembers(int value)
        {
            this.m_maxMemberCount = value;
        }

        public int GetMaximumMembers()
        {
            return this.m_maxMemberCount;
        }

        public void SetMaximumMembers(int value)
        {
            this.m_minMemberCount = value;
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

        public void SetRequireDuelScore(int value)
        {
            this.m_requiredDuelScore = value;
        }

        public int GetMinimumExpLevel()
        {
            return this.m_minExpLevel;
        }

        public void SetMinimumExpLevel(int value)
        {
            this.m_minExpLevel = value;
        }

        public bool IsJoinableOnly()
        {
            return this.m_openOnly;
        }

        public void SetOpenOnly(bool enabled)
        {
            this.m_openOnly = enabled;
        }
    }
}