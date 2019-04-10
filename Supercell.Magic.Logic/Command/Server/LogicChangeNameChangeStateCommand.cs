namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicChangeNameChangeStateCommand : LogicServerCommand
    {
        private int m_state;

        public LogicChangeNameChangeStateCommand()
        {
            // LogicChangeNameStateCommand.
        }

        public LogicChangeNameChangeStateCommand(int state)
        {
            this.m_state = state;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.m_state = stream.ReadInt();
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);
            encoder.WriteInt(this.m_state);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                level.GetPlayerAvatar().SetNameChangeState(this.m_state);
                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_NAME_CHANGE_STATE;
        }
    }
}