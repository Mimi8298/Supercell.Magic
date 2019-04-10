namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LogicLeaveAllianceCommand : LogicServerCommand
    {
        private LogicLong m_allianceId;

        public void SetAllianceData(LogicLong value)
        {
            this.m_allianceId = value;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceId = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_allianceId = stream.ReadLong();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_allianceId);
            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (playerAvatar.IsInAlliance())
                {
                    if (playerAvatar.GetAllianceId().Equals(this.m_allianceId))
                    {
                        playerAvatar.SetAllianceId(null);
                        playerAvatar.SetAllianceName(null);
                        playerAvatar.SetAllianceBadgeId(-1);
                        playerAvatar.SetAllianceLevel(-1);
                        playerAvatar.GetChangeListener().AllianceLeft();
                    }
                }

                level.GetGameListener().AllianceLeft();

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.LEAVE_ALLIANCE;
        }
    }
}