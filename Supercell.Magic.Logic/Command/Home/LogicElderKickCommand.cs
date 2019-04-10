namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicElderKickCommand : LogicCommand
    {
        private LogicLong m_playerId;
        private string m_message;

        public override void Decode(ByteStream stream)
        {
            this.m_playerId = stream.ReadLong();

            if (stream.ReadBoolean())
            {
                this.m_message = stream.ReadString(900000);
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_playerId);

            if (this.m_message != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteString(this.m_message);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.ELDER_KICK;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_playerId = null;
            this.m_message = null;
        }

        public override int Execute(LogicLevel level)
        {
            LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();
            LogicAvatarAllianceRole allianceRole = homeOwnerAvatar.GetAllianceRole();

            if (allianceRole != LogicAvatarAllianceRole.MEMBER)
            {
                if (allianceRole == LogicAvatarAllianceRole.LEADER || allianceRole == LogicAvatarAllianceRole.CO_LEADER)
                {
                    level.GetHomeOwnerAvatar().GetChangeListener().KickPlayer(this.m_playerId, this.m_message);
                    return 0;
                }

                LogicBuilding allianceCastle = level.GetGameObjectManagerAt(0).GetAllianceCastle();

                if (allianceCastle != null)
                {
                    LogicBunkerComponent bunkerComponent = allianceCastle.GetBunkerComponent();

                    if (bunkerComponent != null)
                    {
                        if (bunkerComponent.GetElderCooldownTime() == 0)
                        {
                            bunkerComponent.StartElderKickCooldownTime();
                            level.GetHomeOwnerAvatar().GetChangeListener().KickPlayer(this.m_playerId, this.m_message);
                            return 0;
                        }
                    }
                }

                return -2;
            }

            return -1;
        }
    }
}