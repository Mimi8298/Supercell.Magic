namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicChangeAllianceChatFilterCommand : LogicCommand
    {
        private bool m_enabled;

        public LogicChangeAllianceChatFilterCommand()
        {
            // LogicChangeAllianceChatFilterCommand.
        }

        public LogicChangeAllianceChatFilterCommand(bool enabled)
        {
            this.m_enabled = enabled;
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.m_enabled = stream.ReadBoolean();
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);
            encoder.WriteBoolean(this.m_enabled);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_ALLIANCE_CHAT_FILTER;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (this.m_enabled != playerAvatar.GetAllianceChatFilterEnabled())
                {
                    playerAvatar.SetAllianceChatFilterEnabled(true);
                    level.GetHomeOwnerAvatar().GetChangeListener().AllianceChatFilterChanged(this.m_enabled);
                }

                return 0;
            }

            return -1;
        }
    }
}