namespace Supercell.Magic.Logic.Message.Alliance.War
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceWarHistoryEntry
    {
        private LogicLong m_allianceId;
        private LogicLong m_allianceEnemyId;

        private string m_allianceName;
        private string m_allianceEnemyName;

        private int m_allianceBadgeId;
        private int m_allianceLevel;
        private int m_allianceEnemyBadgeId;
        private int m_allianceEnemyLevel;
        private int m_expEarned;

        private bool m_removed;

        private int m_ageSeconds;
        private int m_villageType;

        public void Decode(ByteStream stream)
        {
            this.m_allianceId = stream.ReadLong();
            this.m_allianceName = stream.ReadString(900000);
            this.m_allianceBadgeId = stream.ReadInt();
            this.m_allianceLevel = stream.ReadInt();

            this.m_allianceEnemyId = stream.ReadLong();
            this.m_allianceEnemyName = stream.ReadString(900000);
            this.m_allianceEnemyBadgeId = stream.ReadInt();
            this.m_allianceEnemyLevel = stream.ReadInt();

            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadInt();
            stream.ReadLong();
            stream.ReadInt();
            this.m_expEarned = stream.ReadInt();
            this.m_removed = stream.ReadBoolean();
            this.m_ageSeconds = stream.ReadInt();
            stream.ReadLong();
            this.m_villageType = stream.ReadInt();
        }

        public void Encode(ByteStream encoder)
        {
            encoder.WriteLong(this.m_allianceId);
            encoder.WriteString(this.m_allianceName);
            encoder.WriteInt(this.m_allianceBadgeId);
            encoder.WriteInt(this.m_allianceLevel);

            encoder.WriteLong(this.m_allianceEnemyId);
            encoder.WriteString(this.m_allianceEnemyName);
            encoder.WriteInt(this.m_allianceEnemyBadgeId);
            encoder.WriteInt(this.m_allianceEnemyLevel);

            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteInt(0);
            encoder.WriteLong(0);
            encoder.WriteInt(0);
            encoder.WriteInt(this.m_expEarned);
            encoder.WriteBoolean(this.m_removed);
            encoder.WriteInt(this.m_ageSeconds);
            encoder.WriteLong(0);
            encoder.WriteInt(this.m_villageType);
        }
    }
}