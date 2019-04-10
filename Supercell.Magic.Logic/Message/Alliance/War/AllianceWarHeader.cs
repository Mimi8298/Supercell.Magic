namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceWarHeader
    {
        private LogicLong m_allianceId;
        private string m_allianceName;
        private int m_allianceBadgeId;
        private int m_allianceLevel;

        public void Decode(ByteStream stream)
        {
            this.m_allianceId = stream.ReadLong();
            this.m_allianceName = stream.ReadString(900000);
            this.m_allianceBadgeId = stream.ReadInt();
            this.m_allianceLevel = stream.ReadInt();
        }

        public void Encode(ByteStream encoder)
        {
            encoder.WriteLong(this.m_allianceId);
            encoder.WriteString(this.m_allianceName);
            encoder.WriteInt(this.m_allianceBadgeId);
            encoder.WriteInt(this.m_allianceLevel);
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong value)
        {
            this.m_allianceId = value;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public int GetAllianceLevel()
        {
            return this.m_allianceLevel;
        }

        public void SetAllianceLevel(int value)
        {
            this.m_allianceLevel = value;
        }
    }
}