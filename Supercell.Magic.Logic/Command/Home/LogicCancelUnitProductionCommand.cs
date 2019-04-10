namespace Supercell.Magic.Logic.Command.Home
{
    using System;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicCancelUnitProductionCommand : LogicCommand
    {
        private LogicCombatItemData m_unitData;

        private int m_unitType;
        private int m_unitCount;
        private int m_gameObjectId;
        private int m_slotId;

        public LogicCancelUnitProductionCommand()
        {
            // LogicCancelConstructionCommand.
        }

        public LogicCancelUnitProductionCommand(int count, LogicCombatItemData combatItemData, int gameObjectId, int slotId)
        {
            this.m_unitCount = count;
            this.m_unitData = combatItemData;
            this.m_gameObjectId = gameObjectId;
            this.m_slotId = slotId;

            this.m_unitType = this.m_unitData.GetCombatItemType();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_unitType = stream.ReadInt();
            this.m_unitData = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream, this.m_unitType != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);
            this.m_unitCount = stream.ReadInt();
            this.m_slotId = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_unitType);
            ByteStreamHelper.WriteDataReference(encoder, this.m_unitData);
            encoder.WriteInt(this.m_unitCount);
            encoder.WriteInt(this.m_slotId);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CANCEL_UNIT_PRODUCTION;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_gameObjectId = 0;
            this.m_unitType = 0;
            this.m_slotId = 0;
            this.m_unitCount = 0;
            this.m_unitData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (!LogicDataTables.GetGlobals().UseNewTraining())
            {
                throw new NotImplementedException(); // TODO: Implement this.
            }

            return this.NewTrainingUnit(level);
        }

        public int NewTrainingUnit(LogicLevel level)
        {
            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                if (this.m_unitData != null)
                {
                    LogicUnitProduction unitProduction = this.m_unitData.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_SPELL
                        ? level.GetGameObjectManager().GetSpellProduction()
                        : level.GetGameObjectManager().GetUnitProduction();

                    if (!unitProduction.IsLocked())
                    {
                        if (this.m_unitCount > 0)
                        {
                            if (this.m_unitData.GetDataType() == unitProduction.GetUnitProductionType())
                            {
                                LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                                LogicResourceData trainingResourceData = this.m_unitData.GetTrainingResource();
                                int trainingCost = level.GetGameMode().GetCalendar().GetTrainingCost(this.m_unitData, playerAvatar.GetUnitUpgradeLevel(this.m_unitData));
                                int refundCount = LogicMath.Max(trainingCost * (this.m_unitData.GetDataType() != LogicDataType.CHARACTER
                                                                    ? LogicDataTables.GetGlobals().GetSpellCancelMultiplier()
                                                                    : LogicDataTables.GetGlobals().GetTrainCancelMultiplier()) / 100, 0);

                                while (unitProduction.RemoveUnit(this.m_unitData, this.m_slotId))
                                {
                                    playerAvatar.CommodityCountChangeHelper(0, trainingResourceData, refundCount);

                                    if (--this.m_unitCount <= 0)
                                    {
                                        break;
                                    }
                                }

                                return 0;
                            }
                        }

                        return -1;
                    }

                    return -23;
                }

                return -1;
            }

            return -99;
        }
    }
}