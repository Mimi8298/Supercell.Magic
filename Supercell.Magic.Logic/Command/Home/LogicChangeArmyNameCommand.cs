namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicChangeArmyNameCommand : LogicCommand
    {
        private int m_armyId;
        private string m_name;

        public LogicChangeArmyNameCommand()
        {
            // LogicChangeArmyNameCommand.
        }

        public LogicChangeArmyNameCommand(int id, string name)
        {
            this.m_armyId = id;
            this.m_name = name;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_armyId = stream.ReadInt();
            this.m_name = stream.ReadString(900000);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_armyId);
            encoder.WriteString(this.m_name);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_ARMY_NAME;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_name = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_armyId > -1)
            {
                if (this.m_armyId <= 3)
                {
                    if (this.m_name.Length <= 16)
                    {
                        level.SetArmyName(this.m_armyId, this.m_name);
                        return 0;
                    }

                    return -4;
                }

                return -2;
            }

            return -1;
        }
    }
}