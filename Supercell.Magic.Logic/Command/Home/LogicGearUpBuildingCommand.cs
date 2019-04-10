namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicGearUpBuildingCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicGearUpBuildingCommand()
        {
            // LogicGearUpBuildingCommand.
        }

        public LogicGearUpBuildingCommand(int gameObjectId)
        {
            this.m_gameObjectId = gameObjectId;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.GEAR_UP_BUILDING;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;
                LogicBuildingData buildingData = building.GetBuildingData();

                if (buildingData.GetVillageType() == level.GetVillageType())
                {
                    if (!level.IsBuildingGearUpCapReached(buildingData, true))
                    {
                        if (building.GetGearLevel() == 0)
                        {
                            int upgLevel = building.GetUpgradeLevel();
                            int gearUpCost = buildingData.GetGearUpCost(upgLevel);

                            if (gearUpCost > 0)
                            {
                                if (upgLevel >= buildingData.GetMinUpgradeLevelForGearUp())
                                {
                                    if (level.GetGameObjectManagerAt(1).GetHighestBuildingLevel(buildingData.GetGearUpBuildingData()) <
                                        buildingData.GetGearUpLevelRequirement())
                                    {
                                        return -1;
                                    }

                                    LogicResourceData gearUpResource = buildingData.GetGearUpResource();

                                    if (gearUpResource != null)
                                    {
                                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                                        if (playerAvatar.HasEnoughResources(gearUpResource, gearUpCost, true, this, false))
                                        {
                                            if (level.HasFreeWorkers(this, 1))
                                            {
                                                playerAvatar.CommodityCountChangeHelper(0, gearUpResource, -gearUpCost);
                                                building.StartUpgrading(true, true);

                                                return 0;
                                            }
                                        }
                                    }

                                    return -1;
                                }

                                return -37;
                            }

                            return -36;
                        }

                        return -35;
                    }

                    return -31;
                }

                return -32;
            }

            return -1;
        }
    }
}