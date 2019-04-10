namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicTrainUnitVillage2Command : LogicCommand
    {
        private LogicCombatItemData m_unitData;

        private int m_unitType;
        private int m_gameObjectId;

        public LogicTrainUnitVillage2Command()
        {
            // LogicTrainUnitVillage2Command.
        }

        public LogicTrainUnitVillage2Command(int gameObjectId, LogicCombatItemData combatItemData)
        {
            this.m_gameObjectId = gameObjectId;
            this.m_unitData = combatItemData;
            this.m_unitType = this.m_unitData.GetDataType() == LogicDataType.SPELL ? 1 : 0;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_unitType = stream.ReadInt();
            this.m_unitData = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream, this.m_unitType != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_unitType);
            ByteStreamHelper.WriteDataReference(encoder, this.m_unitData);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TRAIN_UNIT_VILLAGE2;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_gameObjectId = 0;
            this.m_unitType = 0;
            this.m_unitData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 1)
            {
                if (this.m_gameObjectId != 0)
                {
                    LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(1);
                    LogicGameObject gameObject = gameObjectManager.GetGameObjectByID(this.m_gameObjectId);

                    if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        LogicBuilding building = (LogicBuilding) gameObject;

                        if (this.m_unitData != null && level.GetGameMode().GetCalendar().IsProductionEnabled(this.m_unitData))
                        {
                            if (this.m_unitData.GetVillageType() == 1)
                            {
                                LogicVillage2UnitComponent village2UnitComponent = building.GetVillage2UnitComponent();

                                if (village2UnitComponent != null)
                                {
                                    if (this.m_unitData.IsUnlockedForProductionHouseLevel(gameObjectManager.GetHighestBuildingLevel(this.m_unitData.GetProductionHouseData(), true))
                                    )
                                    {
                                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                                        LogicResourceData trainResource = this.m_unitData.GetTrainingResource();
                                        int trainCost = this.m_unitData.GetTrainingCost(playerAvatar.GetUnitUpgradeLevel(this.m_unitData));

                                        if (playerAvatar.HasEnoughResources(trainResource, trainCost, true, this, false))
                                        {
                                            village2UnitComponent.TrainUnit(this.m_unitData);
                                            playerAvatar.CommodityCountChangeHelper(0, trainResource, -trainCost);
                                        }

                                        return 0;
                                    }

                                    return -7;
                                }

                                return -4;
                            }

                            return -8;
                        }
                    }

                    return -5;
                }

                return -1;
            }

            return -10;
        }
    }
}