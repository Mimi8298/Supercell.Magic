namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;

    public sealed class LogicTrainUnitCommand : LogicCommand
    {
        private LogicCombatItemData m_unitData;

        private int m_unitType;
        private int m_trainCount;
        private int m_gameObjectId;
        private int m_slotId;

        public LogicTrainUnitCommand()
        {
            // LogicTrainUnitCommand.
        }

        public LogicTrainUnitCommand(int count, LogicCombatItemData combatItemData, int gameObjectId, int slotId)
        {
            this.m_trainCount = count;
            this.m_unitData = combatItemData;
            this.m_gameObjectId = gameObjectId;
            this.m_slotId = slotId;
            this.m_unitType = this.m_unitData.GetDataType() == LogicDataType.SPELL ? 1 : 0;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_unitType = stream.ReadInt();
            this.m_unitData = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream, this.m_unitType != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);
            this.m_trainCount = stream.ReadInt();
            this.m_slotId = stream.ReadInt();

            LogicGlobals globals = LogicDataTables.GetGlobals();

            if (!globals.UseDragInTraining() && !globals.UseDragInTrainingFix() && !globals.UseDragInTrainingFix2())
            {
                this.m_slotId = -1;
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_unitType);
            ByteStreamHelper.WriteDataReference(encoder, this.m_unitData);
            encoder.WriteInt(this.m_trainCount);
            encoder.WriteInt(this.m_slotId);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TRAIN_UNIT;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_gameObjectId = 0;
            this.m_unitType = 0;
            this.m_slotId = 0;
            this.m_trainCount = 0;
            this.m_unitData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 0)
            {
                if (!LogicDataTables.GetGlobals().UseNewTraining())
                {
                    if (this.m_gameObjectId == 0)
                    {
                        return -1;
                    }

                    // TODO: Implement this.
                }
                else
                {
                    return this.NewTrainingUnit(level);
                }
            }

            return -32;
        }

        public int NewTrainingUnit(LogicLevel level)
        {
            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                if (this.m_trainCount <= 100)
                {
                    LogicUnitProduction unitProduction = this.m_unitType == 1
                        ? level.GetGameObjectManagerAt(0).GetSpellProduction()
                        : level.GetGameObjectManagerAt(0).GetUnitProduction();

                    if (this.m_trainCount > 0)
                    {
                        if (this.m_unitData != null)
                        {
                            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                            bool firstLoopExecuted = false;
                            int trainingCost = level.GetGameMode().GetCalendar().GetTrainingCost(this.m_unitData, playerAvatar.GetUnitUpgradeLevel(this.m_unitData));

                            for (int i = 0; i < this.m_trainCount; i++)
                            {
                                if (!unitProduction.CanAddUnitToQueue(this.m_unitData, false))
                                {
                                    if (firstLoopExecuted)
                                    {
                                        break;
                                    }

                                    return -40;
                                }

                                if (!playerAvatar.HasEnoughResources(this.m_unitData.GetTrainingResource(), trainingCost, true, this, false))
                                {
                                    if (firstLoopExecuted)
                                    {
                                        break;
                                    }

                                    return -30;
                                }

                                playerAvatar.CommodityCountChangeHelper(0, this.m_unitData.GetTrainingResource(), -trainingCost);

                                if (this.m_slotId == -1)
                                {
                                    this.m_slotId = unitProduction.GetSlotCount();
                                }

                                unitProduction.AddUnitToQueue(this.m_unitData, this.m_slotId, false);
                                firstLoopExecuted = true;
                            }

                            return 0;
                        }
                    }

                    return -50;
                }

                Debugger.Error("LogicTraingUnitCommand - Count is too high");

                return -20;
            }

            return -99;
        }
    }
}