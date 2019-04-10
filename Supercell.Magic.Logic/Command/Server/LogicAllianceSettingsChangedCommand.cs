namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LogicAllianceSettingsChangedCommand : LogicServerCommand
    {
        private LogicLong m_allianceId;
        private int m_allianceBadgeId;

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceId = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_allianceId = stream.ReadLong();
            this.m_allianceBadgeId = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_allianceId);
            encoder.WriteInt(this.m_allianceBadgeId);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (LogicLong.Equals(playerAvatar.GetAllianceId(), this.m_allianceId))
                {
                    playerAvatar.SetAllianceBadgeId(this.m_allianceBadgeId);
                    level.GetGameListener().AllianceSettingsChanged();
                }

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.ALLIANCE_SETTINGS_CHANGED;
        }

        public void SetAllianceData(LogicLong allianceId, int allianceBadgeId)
        {
            this.m_allianceId = allianceId;
            this.m_allianceBadgeId = allianceBadgeId;
        }
    }
}