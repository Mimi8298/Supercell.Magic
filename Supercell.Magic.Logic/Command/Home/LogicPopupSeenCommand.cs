namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicPopupSeenCommand : LogicCommand
    {
        private int m_popupType;
        private bool m_seen;

        public override void Decode(ByteStream stream)
        {
            this.m_popupType = stream.ReadInt();
            this.m_seen = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_popupType);
            encoder.WriteBoolean(this.m_seen);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.POPUP_SEEN;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            switch (this.m_popupType)
            {
                case 0:
                    level.SetHelpOpened(this.m_seen);
                    break;
                case 1:
                    level.SetEditModeShown();
                    break;
                case 2:
                    level.SetShieldCostPopupShown(this.m_seen);
                    break;
                case 3:
                    break;
                default:
                    return -1;
            }

            return 0;
        }
    }
}