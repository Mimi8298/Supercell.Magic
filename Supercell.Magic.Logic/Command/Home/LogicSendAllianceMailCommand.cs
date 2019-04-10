namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSendAllianceMailCommand : LogicCommand
    {
        private string m_message;

        public LogicSendAllianceMailCommand()
        {
            // LogicSendAllianceMailCommand.
        }

        public LogicSendAllianceMailCommand(string message)
        {
            this.m_message = message;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_message = stream.ReadString(900000);
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteString(this.m_message);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SEND_ALLIANCE_MAIL;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_message = null;
        }

        public override int Execute(LogicLevel level)
        {
            LogicAvatarAllianceRole allianceRole = level.GetHomeOwnerAvatar().GetAllianceRole();

            if (allianceRole == LogicAvatarAllianceRole.LEADER || allianceRole == LogicAvatarAllianceRole.CO_LEADER)
            {
                LogicBuilding allianceCastle = level.GetGameObjectManagerAt(0).GetAllianceCastle();

                if (allianceCastle != null)
                {
                    LogicBunkerComponent bunkerComponent = allianceCastle.GetBunkerComponent();

                    if (bunkerComponent != null && bunkerComponent.GetClanMailCooldownTime() == 0)
                    {
                        bunkerComponent.StartClanMailCooldownTime();
                        level.GetHomeOwnerAvatar().GetChangeListener().SendClanMail(this.m_message);

                        return 0;
                    }
                }

                return -2;
            }

            return -1;
        }
    }
}