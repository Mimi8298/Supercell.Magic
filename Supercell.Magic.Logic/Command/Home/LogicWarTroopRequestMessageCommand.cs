namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicWarTroopRequestMessageCommand : LogicCommand
    {
        private string m_message;

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
            return LogicCommandType.WAR_TROOP_REQUEST_MESSAGE;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_message = null;
        }

        public override int Execute(LogicLevel level)
        {
            level.SetWarTroopRequestMessage(this.m_message);

            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.GetChangeListener().WarTroopRequestMessageChanged(this.m_message);
            }

            return 0;
        }
    }
}