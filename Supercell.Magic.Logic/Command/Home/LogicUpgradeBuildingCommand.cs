namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicUpgradeBuildingCommand : LogicCommand
    {
        private int m_gameObjectId;
        private bool m_useAltResource;

        public LogicUpgradeBuildingCommand()
        {
            // LogicUpgradeBuildingCommand.
        }

        public LogicUpgradeBuildingCommand(int gameObjectId, bool useAltResource)
        {
            this.m_gameObjectId = gameObjectId;
            this.m_useAltResource = useAltResource;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_useAltResource = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteBoolean(this.m_useAltResource);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.UPGRADE_BUILDING;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null)
            {
                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                {
                    LogicBuilding building = (LogicBuilding) gameObject;
                    LogicBuildingData buildingData = building.GetBuildingData();

                    if (buildingData.IsTownHallVillage2())
                    {
                        if (!LogicUpgradeBuildingCommand.CanUpgradeTHV2(level))
                        {
                            return -76;
                        }
                    }

                    if (buildingData.GetVillageType() == level.GetVillageType())
                    {
                        if (level.GetGameObjectManager().GetAvailableBuildingUpgradeCount(building) <= 0)
                        {
                            return -34;
                        }

                        if (building.GetWallIndex() == 0)
                        {
                            if (building.CanUpgrade(true))
                            {
                                int nextUpgradeLevel = building.GetUpgradeLevel() + 1;
                                int buildCost = buildingData.GetBuildCost(nextUpgradeLevel, level);

                                LogicResourceData buildResourceData = this.m_useAltResource
                                    ? buildingData.GetAltBuildResource(nextUpgradeLevel)
                                    : buildingData.GetBuildResource(nextUpgradeLevel);

                                if (buildResourceData != null)
                                {
                                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                                    if (playerAvatar.HasEnoughResources(buildResourceData, buildCost, true, this, false))
                                    {
                                        if (buildingData.GetConstructionTime(nextUpgradeLevel, level, 0) != 0 || LogicDataTables.GetGlobals().WorkerForZeroBuilTime())
                                        {
                                            if (!level.HasFreeWorkers(this, -1))
                                            {
                                                return -1;
                                            }
                                        }

                                        playerAvatar.CommodityCountChangeHelper(0, buildResourceData, -buildCost);
                                        building.StartUpgrading(true, false);

                                        return 0;
                                    }
                                }
                            }

                            return -1;
                        }

                        return -35;
                    }

                    return -32;
                }

                if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                {
                    LogicTrap trap = (LogicTrap) gameObject;

                    if (trap.CanUpgrade(true))
                    {
                        LogicTrapData data = trap.GetTrapData();
                        LogicResourceData buildResourceData = data.GetBuildResource();
                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                        int buildCost = data.GetBuildCost(trap.GetUpgradeLevel() + 1);

                        if (playerAvatar.HasEnoughResources(buildResourceData, buildCost, true, this, false))
                        {
                            if (data.GetBuildTime(trap.GetUpgradeLevel() + 1) != 0 || LogicDataTables.GetGlobals().WorkerForZeroBuilTime())
                            {
                                if (!level.HasFreeWorkers(this, -1))
                                {
                                    return -1;
                                }
                            }

                            playerAvatar.CommodityCountChangeHelper(0, buildResourceData, -buildCost);
                            trap.StartUpgrading();

                            return 0;
                        }
                    }
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.VILLAGE_OBJECT)
                {
                    if (!this.m_useAltResource)
                    {
                        LogicVillageObject villageObject = (LogicVillageObject) gameObject;

                        if (villageObject.CanUpgrade(true))
                        {
                            LogicVillageObjectData data = villageObject.GetVillageObjectData();
                            LogicResourceData buildResourceData = data.GetBuildResource();

                            int buildCost = data.GetBuildCost(villageObject.GetUpgradeLevel() + 1);

                            if (buildResourceData != null)
                            {
                                LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                                if (playerAvatar.HasEnoughResources(buildResourceData, buildCost, true, this, false))
                                {
                                    if (data.GetBuildTime(villageObject.GetUpgradeLevel() + 1) != 0 || LogicDataTables.GetGlobals().WorkerForZeroBuilTime())
                                    {
                                        if (!level.HasFreeWorkers(this, -1))
                                        {
                                            return -1;
                                        }
                                    }

                                    playerAvatar.CommodityCountChangeHelper(0, buildResourceData, -buildCost);
                                    villageObject.StartUpgrading(true);

                                    return 0;
                                }
                            }
                        }

                        return -1;
                    }

                    return -31;
                }
            }

            return -1;
        }

        private static bool CanUpgradeTHV2(LogicLevel level)
        {
            if (level.GetVillageType() == 1)
            {
                LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);

                for (int i = 0; i < buildingTable.GetItemCount(); i++)
                {
                    LogicBuildingData buildingData = (LogicBuildingData) buildingTable.GetItemAt(i);

                    if (buildingData.GetVillageType() == 1 &&
                        !level.IsBuildingCapReached(buildingData, false))
                    {
                        return false;
                    }
                }

                LogicDataTable trapTable = LogicDataTables.GetTable(LogicDataType.TRAP);

                for (int i = 0; i < trapTable.GetItemCount(); i++)
                {
                    LogicTrapData trapData = (LogicTrapData) trapTable.GetItemAt(i);

                    if (trapData.GetVillageType() == 1 &&
                        !level.IsTrapCapReached(trapData, false))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}