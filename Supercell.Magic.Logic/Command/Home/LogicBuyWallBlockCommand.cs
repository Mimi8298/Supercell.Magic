namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicBuyWallBlockCommand : LogicCommand
    {
        private LogicBuildingData m_buildingData;
        private LogicArrayList<LogicVector2> m_position;

        public LogicBuyWallBlockCommand()
        {
            this.m_position = new LogicArrayList<LogicVector2>();
        }

        public override void Decode(ByteStream stream)
        {
            int count = LogicMath.Min(stream.ReadInt(), 10);

            for (int i = 0; i < count; i++)
            {
                this.m_position.Add(new LogicVector2(stream.ReadInt(), stream.ReadInt()));
            }

            this.m_buildingData = (LogicBuildingData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.BUILDING);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            int count = LogicMath.Min(this.m_position.Size(), 10);

            encoder.WriteInt(count);

            for (int i = 0; i < count; i++)
            {
                encoder.WriteInt(this.m_position[i].m_x);
                encoder.WriteInt(this.m_position[i].m_y);
            }

            ByteStreamHelper.WriteDataReference(encoder, this.m_buildingData);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BUY_WALL_BLOCK;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_position != null)
            {
                for (int i = this.m_position.Size() - 1; i >= 0; i--)
                {
                    this.m_position[i].Destruct();
                    this.m_position.Remove(i);
                }

                this.m_position = null;
            }

            this.m_buildingData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 1)
            {
                if (this.m_buildingData != null && this.m_buildingData.GetBuildingClass().CanBuy() && this.m_buildingData.IsWall())
                {
                    if (this.m_buildingData.GetWallBlockCount() != 0)
                    {
                        if (this.m_buildingData.GetWallBlockCount() == this.m_position.Size())
                        {
                            if (this.m_position.Size() <= 10)
                            {
                                for (int i = 0, nIdx = -1; i < this.m_position.Size(); i++)
                                {
                                    LogicVector2 firstPosition = this.m_position[0];
                                    LogicVector2 position = this.m_position[i];

                                    if (i > 0)
                                    {
                                        int shapeIdx = this.m_buildingData.GetWallBlockIndex(position.m_x - firstPosition.m_x, position.m_y - firstPosition.m_y, i);

                                        if (nIdx == -1)
                                        {
                                            nIdx = shapeIdx;
                                        }

                                        if (shapeIdx == -1 || shapeIdx != nIdx)
                                        {
                                            Debugger.Error("LogicBuyWallBlockCommand shape incorrect");
                                            return -4;
                                        }

                                        nIdx = shapeIdx;
                                    }

                                    if (!level.IsValidPlaceForBuilding(position.m_x, position.m_y, this.m_buildingData.GetWidth(), this.m_buildingData.GetHeight(), null))
                                    {
                                        Debugger.Error("LogicBuyWallBlockCommand invalid place.");
                                        return -5;
                                    }
                                }

                                if (!level.IsBuildingCapReached(this.m_buildingData, true))
                                {
                                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                                    LogicResourceData buildResource = this.m_buildingData.GetBuildResource(0);

                                    int buildCost = this.m_buildingData.GetBuildCost(0, level);

                                    if (playerAvatar.HasEnoughResources(buildResource, buildCost, true, this, false))
                                    {
                                        if (this.m_buildingData.IsWorkerBuilding() ||
                                            this.m_buildingData.GetConstructionTime(0, level, 0) <= 0 && !LogicDataTables.GetGlobals().WorkerForZeroBuilTime() ||
                                            level.HasFreeWorkers(this, -1))
                                        {
                                            if (buildResource.IsPremiumCurrency())
                                            {
                                                playerAvatar.UseDiamonds(buildCost);
                                                playerAvatar.GetChangeListener().DiamondPurchaseMade(1, this.m_buildingData.GetGlobalID(), 0, buildCost, level.GetVillageType());
                                            }
                                            else
                                            {
                                                playerAvatar.CommodityCountChangeHelper(0, buildResource, -buildCost);
                                            }

                                            LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
                                            int wallIndex = gameObjectManager.GetHighestWallIndex(this.m_buildingData);

                                            for (int i = 0; i < this.m_position.Size(); i++)
                                            {
                                                LogicVector2 position = this.m_position[i];
                                                LogicBuilding building =
                                                    (LogicBuilding) LogicGameObjectFactory.CreateGameObject(this.m_buildingData, level, level.GetVillageType());

                                                building.StartConstructing(false);
                                                building.SetInitialPosition(position.m_x << 9, position.m_y << 9);
                                                building.SetWallObjectId(wallIndex, i, i == 0);

                                                gameObjectManager.AddGameObject(building, -1);

                                                int width = building.GetWidthInTiles();
                                                int height = building.GetHeightInTiles();

                                                for (int j = 0; j < width; j++)
                                                {
                                                    for (int k = 0; k < height; k++)
                                                    {
                                                        LogicObstacle tallGrass = level.GetTileMap().GetTile(position.m_x + j, position.m_y + k).GetTallGrass();

                                                        if (tallGrass != null)
                                                        {
                                                            level.GetGameObjectManager().RemoveGameObject(tallGrass);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                return 0;
                            }

                            return -3;
                        }

                        return -2;
                    }

                    return -1;
                }

                return 0;
            }

            return -32;
        }
    }
}