namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicLockUnitProductionCommand : LogicCommand
    {
        private bool m_disabled;
        private int m_index;

        public LogicLockUnitProductionCommand()
        {
            // LogicLockUnitProductionCommand.
        }

        public LogicLockUnitProductionCommand(bool disabled, int index)
        {
            this.m_disabled = disabled;
            this.m_index = index;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_disabled = stream.ReadBoolean();
            this.m_index = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteBoolean(this.m_disabled);
            encoder.WriteInt(this.m_index);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.LOCK_UNIT_PRODUCTION;
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 0)
            {
                LogicUnitProduction unitProduction = null;

                switch (this.m_index)
                {
                    case 1:
                        unitProduction = level.GetGameObjectManagerAt(0).GetUnitProduction();
                        break;
                    case 2:
                        unitProduction = level.GetGameObjectManagerAt(0).GetSpellProduction();
                        break;
                }

                if (unitProduction == null)
                {
                    return -1;
                }

                unitProduction.SetLocked(this.m_disabled);

                return 0;
            }

            return -32;
        }
    }
}