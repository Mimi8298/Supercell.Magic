namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicChangeWarPreferenceCommand : LogicCommand
    {
        private int m_warPreference;

        public LogicChangeWarPreferenceCommand()
        {
            // LogicChangeWarPreferenceCommand.
        }

        public LogicChangeWarPreferenceCommand(int preference)
        {
            this.m_warPreference = preference;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_warPreference = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_warPreference);
            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            level.GetPlayerAvatar().GetChangeListener().WarPreferenceChanged(this.m_warPreference);
            return 0;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_WAR_PREFERENCE;
        }
    }
}