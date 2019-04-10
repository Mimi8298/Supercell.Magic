namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicChangeAvatarNameCommand : LogicServerCommand
    {
        private string m_avatarName;
        private int m_nameChangeState;

        public override void Destruct()
        {
            base.Destruct();
            this.m_avatarName = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_avatarName = stream.ReadString(900000);
            this.m_nameChangeState = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteString(this.m_avatarName);
            encoder.WriteInt(this.m_nameChangeState);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.SetName(this.m_avatarName);
                playerAvatar.SetNameSetByUser(true);
                playerAvatar.SetNameChangeState(this.m_nameChangeState);

                level.GetGameListener().NameChanged(this.m_avatarName);

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_AVATAR_NAME;
        }

        public void SetAvatarName(string avatarName)
        {
            this.m_avatarName = avatarName;
        }

        public void SetAvatarNameChangeState(int state)
        {
            this.m_nameChangeState = state;
        }
    }
}