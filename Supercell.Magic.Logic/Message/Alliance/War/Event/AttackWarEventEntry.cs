namespace Supercell.Magic.Logic.Message.Alliance.War.Event
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AttackWarEventEntry : WarEventEntry
    {
        private LogicLong m_accountId;
        private LogicLong m_avatarId;
        private LogicLong m_homeId;
        private LogicLong m_allianceId;

        private readonly string m_avatarName;
        private readonly string m_allianceName;

        private int m_stars;
        private int m_destructionPercentage;
        private int m_replayShardId;

        private LogicLong m_replayId;

        public AttackWarEventEntry()
        {
            this.m_avatarName = string.Empty;
            this.m_allianceName = string.Empty;
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteLong(this.m_allianceId);
            stream.WriteLong(this.m_accountId);
            stream.WriteLong(this.m_avatarId);
            stream.WriteLong(this.m_homeId);

            stream.WriteString(this.m_allianceName);
            stream.WriteString(this.m_avatarName);

            stream.WriteInt(1);
            stream.WriteInt(2);
            stream.WriteInt(3);
            stream.WriteInt(4);
            stream.WriteInt(5);
            stream.WriteInt(6);
            stream.WriteInt(7);

            if (this.m_replayId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteInt(this.m_replayShardId);
                stream.WriteLong(this.m_replayId);
            }
            else
            {
                stream.WriteBoolean(false);
            }

            stream.WriteInt(8);
        }

        public override int GetWarEventEntryType()
        {
            return WarEventEntry.WAR_EVENT_ENTRY_TYPE_ATTACK;
        }
    }
}