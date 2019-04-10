namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LogicChangeAllianceRoleCommand : LogicServerCommand
    {
        private LogicLong m_allianceId;
        private LogicAvatarAllianceRole m_allianceRole;

        public void SetData(LogicLong allianceId, LogicAvatarAllianceRole allianceRole)
        {
            this.m_allianceId = allianceId;
            this.m_allianceRole = allianceRole;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceId = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_allianceId = stream.ReadLong();
            this.m_allianceRole = (LogicAvatarAllianceRole) stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_allianceId);
            encoder.WriteInt((int) this.m_allianceRole);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (playerAvatar.IsInAlliance())
                {
                    if (LogicLong.Equals(playerAvatar.GetAllianceId(), this.m_allianceId))
                    {
                        playerAvatar.SetAllianceRole(this.m_allianceRole);
                    }
                }

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_ALLIANCE_ROLE;
        }
    }
}