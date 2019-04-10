namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LogicJoinAllianceCommand : LogicServerCommand
    {
        private LogicLong m_allianceId;
        private string m_allianceName;
        private int m_allianceBadgeId;
        private int m_allianceExpLevel;
        private bool m_allianceCreate;

        public override void Destruct()
        {
            base.Destruct();

            this.m_allianceId = null;
            this.m_allianceName = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_allianceId = stream.ReadLong();
            this.m_allianceName = stream.ReadString(900000);
            this.m_allianceBadgeId = stream.ReadInt();
            this.m_allianceCreate = stream.ReadBoolean();
            this.m_allianceExpLevel = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_allianceId);
            encoder.WriteString(this.m_allianceName);
            encoder.WriteInt(this.m_allianceBadgeId);
            encoder.WriteBoolean(this.m_allianceCreate);
            encoder.WriteInt(this.m_allianceExpLevel);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (this.m_allianceCreate)
                {
                    LogicGlobals globals = LogicDataTables.GetGlobals();
                    LogicResourceData resource = globals.GetAllianceCreateResourceData();

                    int removeCount = LogicMath.Min(globals.GetAllianceCreateCost(), playerAvatar.GetResourceCount(resource));

                    playerAvatar.CommodityCountChangeHelper(0, resource, -removeCount);
                }

                playerAvatar.SetAllianceId(this.m_allianceId.Clone());
                playerAvatar.SetAllianceName(this.m_allianceName);
                playerAvatar.SetAllianceBadgeId(this.m_allianceBadgeId);
                playerAvatar.SetAllianceLevel(this.m_allianceExpLevel);
                playerAvatar.SetAllianceRole(this.m_allianceCreate ? LogicAvatarAllianceRole.LEADER : LogicAvatarAllianceRole.MEMBER);
                playerAvatar.GetChangeListener().AllianceJoined(playerAvatar.GetAllianceId(), this.m_allianceName, this.m_allianceBadgeId, this.m_allianceExpLevel,
                                                                playerAvatar.GetAllianceRole());

                LogicGameListener gameListener = level.GetGameListener();

                if (gameListener != null)
                {
                    if (this.m_allianceCreate)
                    {
                        gameListener.AllianceCreated();
                    }
                    else
                    {
                        gameListener.AllianceJoined();
                    }
                }

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.JOIN_ALLIANCE;
        }

        public void SetAllianceData(LogicLong allianceId, string allianceName, int allianceBadgeId, int allianceExpLevel, bool isNewAlliance)
        {
            this.m_allianceId = allianceId;
            this.m_allianceName = allianceName;
            this.m_allianceBadgeId = allianceBadgeId;
            this.m_allianceExpLevel = allianceExpLevel;
            this.m_allianceCreate = isNewAlliance;
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }
    }
}