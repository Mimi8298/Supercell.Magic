namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicBuyBuildingCommand : LogicCommand
    {
        private int m_x;
        private int m_y;
        private LogicBuildingData m_buildingData;

        public LogicBuyBuildingCommand()
        {
            // LogicBuyBuildingCommand.
        }

        public LogicBuyBuildingCommand(int x, int y, LogicBuildingData buildingData)
        {
            this.m_x = x;
            this.m_y = y;
            this.m_buildingData = buildingData;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_buildingData = (LogicBuildingData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.BUILDING);
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            ByteStreamHelper.WriteDataReference(encoder, this.m_buildingData);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BUY_BUILDING;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_x = 0;
            this.m_y = 0;
            this.m_buildingData = null;
        }

        public override int Execute(LogicLevel level)
        {
            int villageType = level.GetVillageType();

            if (this.m_buildingData.GetVillageType() == villageType)
            {
                if (this.m_buildingData.GetWallBlockCount() <= 1 && this.m_buildingData.GetBuildingClass().CanBuy())
                {
                    if (level.IsValidPlaceForBuilding(this.m_x, this.m_y, this.m_buildingData.GetWidth(), this.m_buildingData.GetHeight(), null) &&
                        !level.IsBuildingCapReached(this.m_buildingData, true))
                    {
                        if (level.GetCalendar().IsEnabled(this.m_buildingData))
                        {
                            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                            LogicResourceData buildResourceData = this.m_buildingData.GetBuildResource(0);

                            int buildResourceCost = this.m_buildingData.GetBuildCost(0, level);

                            if (playerAvatar.HasEnoughResources(buildResourceData, buildResourceCost, true, this, false))
                            {
                                if (this.m_buildingData.IsWorkerBuilding() ||
                                    this.m_buildingData.GetConstructionTime(0, level, 0) <= 0 && !LogicDataTables.GetGlobals().WorkerForZeroBuilTime() ||
                                    level.HasFreeWorkers(this, -1))
                                {
                                    if (buildResourceData.IsPremiumCurrency())
                                    {
                                        playerAvatar.UseDiamonds(buildResourceCost);
                                        playerAvatar.GetChangeListener().DiamondPurchaseMade(1, this.m_buildingData.GetGlobalID(), 0, buildResourceCost, villageType);
                                    }
                                    else
                                    {
                                        playerAvatar.CommodityCountChangeHelper(0, buildResourceData, -buildResourceCost);
                                    }

                                    LogicBuilding building = (LogicBuilding) LogicGameObjectFactory.CreateGameObject(this.m_buildingData, level, villageType);

                                    building.SetInitialPosition(this.m_x << 9, this.m_y << 9);
                                    level.GetGameObjectManager().AddGameObject(building, -1);
                                    building.StartConstructing(false);

                                    if (this.m_buildingData.IsWall() && level.IsBuildingCapReached(this.m_buildingData, false))
                                    {
                                        level.GetGameListener().BuildingCapReached(this.m_buildingData);
                                    }

                                    int width = building.GetWidthInTiles();
                                    int height = building.GetHeightInTiles();

                                    for (int i = 0; i < width; i++)
                                    {
                                        for (int j = 0; j < height; j++)
                                        {
                                            LogicObstacle tallGrass = level.GetTileMap().GetTile(this.m_x + i, this.m_y + j).GetTallGrass();

                                            if (tallGrass != null)
                                            {
                                                level.GetGameObjectManager().RemoveGameObject(tallGrass);
                                            }
                                        }
                                    }
                                }
                            }

                            return 0;
                        }
                    }
                }

                return -33;
            }

            return -32;
        }
    }
}