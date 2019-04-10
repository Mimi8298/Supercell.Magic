namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSetPersistentBoolCommand : LogicCommand
    {
        private int m_index;
        private bool m_value;

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_index = stream.ReadInt();
            this.m_value = stream.ReadBoolean();
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);

            encoder.WriteInt(this.m_index);
            encoder.WriteBoolean(this.m_value);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SET_PERSISTENT_BOOL;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            switch (this.m_index)
            {
                case 0:
                    level.SetPersistentBool(0, this.m_value);
                    return 0;
                default:
                    return -1;
            }
        }
    }
}