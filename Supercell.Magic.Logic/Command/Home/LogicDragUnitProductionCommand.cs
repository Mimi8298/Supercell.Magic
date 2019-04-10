namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicDragUnitProductionCommand : LogicCommand
    {
        private bool m_spellProduction;
        private int m_slotIdx;
        private int m_dragIdx;

        public LogicDragUnitProductionCommand()
        {
            // LogicDragUnitProductionCommand.
        }

        public LogicDragUnitProductionCommand(bool spellProduction, int slotIdx, int dragIdx)
        {
            this.m_spellProduction = spellProduction;
            this.m_slotIdx = slotIdx;
            this.m_dragIdx = dragIdx;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_spellProduction = stream.ReadBoolean();
            this.m_slotIdx = stream.ReadInt();
            this.m_dragIdx = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteBoolean(this.m_spellProduction);
            encoder.WriteInt(this.m_slotIdx);
            encoder.WriteInt(this.m_dragIdx);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.DRAG_UNIT_PRODUCTION;
        }

        public override int Execute(LogicLevel level)
        {
            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                if (!LogicDataTables.GetGlobals().UseDragInTraining() &&
                    !LogicDataTables.GetGlobals().UseDragInTrainingFix() &&
                    !LogicDataTables.GetGlobals().UseDragInTrainingFix2())
                {
                    return -51;
                }

                LogicUnitProduction unitProduction = this.m_spellProduction ? level.GetGameObjectManager().GetSpellProduction() : level.GetGameObjectManager().GetUnitProduction();

                if (unitProduction.GetSlotCount() > this.m_slotIdx)
                {
                    if (unitProduction.GetSlotCount() >= this.m_dragIdx)
                    {
                        if (this.m_slotIdx >= 0)
                        {
                            if (this.m_dragIdx >= 0)
                            {
                                return unitProduction.DragSlot(this.m_slotIdx, this.m_dragIdx) ? 0 : -5;
                            }

                            return -4;
                        }

                        return -3;
                    }

                    return -2;
                }

                return -1;
            }

            return -50;
        }
    }
}