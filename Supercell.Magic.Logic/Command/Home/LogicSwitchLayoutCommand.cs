namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicSwitchLayoutCommand : LogicCommand
    {
        private int m_layoutId;
        private int m_layoutType;

        public LogicSwitchLayoutCommand()
        {
            // LogicSwitchLayoutCommand.
        }

        public LogicSwitchLayoutCommand(int layoutId, int layoutType)
        {
            this.m_layoutId = layoutId;
            this.m_layoutType = layoutType;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_layoutId = stream.ReadInt();
            this.m_layoutType = stream.ReadInt();

            if (this.m_layoutId == 6 || this.m_layoutId == 7)
            {
                this.m_layoutId = -1;
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_layoutId);
            encoder.WriteInt(this.m_layoutType);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_layoutId != -1)
            {
                if (level.GetTownHallLevel(level.GetVillageType()) >= level.GetRequiredTownHallLevelForLayout(this.m_layoutId, -1))
                {
                    if ((this.m_layoutId & 0xFFFFFFFE) != 6)
                    {
                        if (this.m_layoutType != 0)
                        {
                            if (this.m_layoutId != 1 && this.m_layoutId != 4 && this.m_layoutId != 5)
                            {
                                return -5;
                            }
                        }
                        else
                        {
                            if (this.m_layoutId != 0 && this.m_layoutId != 2 && this.m_layoutId != 3)
                            {
                                return -4;
                            }
                        }
                    }

                    LogicGameObjectFilter filter = new LogicGameObjectFilter();
                    LogicArrayList<LogicGameObject> gameObjects = new LogicArrayList<LogicGameObject>(500);

                    filter.AddGameObjectType(LogicGameObjectType.BUILDING);
                    filter.AddGameObjectType(LogicGameObjectType.TRAP);
                    filter.AddGameObjectType(LogicGameObjectType.DECO);

                    level.GetGameObjectManager().GetGameObjects(gameObjects, filter);

                    for (int i = 0; i < gameObjects.Size(); i++)
                    {
                        LogicGameObject gameObject = gameObjects[i];
                        LogicVector2 position = gameObject.GetPositionLayout(this.m_layoutId, false);

                        if ((this.m_layoutId & 0xFFFFFFFE) != 6 && (position.m_x == -1 || position.m_y == -1))
                        {
                            return -2;
                        }
                    }

                    LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
                    LogicArrayList<LogicGameObject> buildings = gameObjectManager.GetGameObjects(LogicGameObjectType.BUILDING);
                    LogicArrayList<LogicGameObject> obstacles = gameObjectManager.GetGameObjects(LogicGameObjectType.OBSTACLE);

                    for (int i = 0; i < gameObjects.Size(); i++)
                    {
                        LogicGameObject gameObject = gameObjects[i];
                        LogicVector2 position = gameObject.GetPositionLayout(this.m_layoutId, false);

                        int minX = position.m_x;
                        int minY = position.m_y;
                        int maxX = minX + gameObject.GetWidthInTiles();
                        int maxY = minY + gameObject.GetHeightInTiles();

                        for (int j = 0; j < obstacles.Size(); j++)
                        {
                            LogicObstacle obstacle = (LogicObstacle) obstacles[j];

                            int minX2 = obstacle.GetTileX();
                            int minY2 = obstacle.GetTileY();
                            int maxX2 = minX2 + obstacle.GetWidthInTiles();
                            int maxY2 = minY2 + obstacle.GetHeightInTiles();

                            if (maxX > minX2 && maxY > minY2 && minX < maxX2 && minY < maxY2)
                            {
                                if ((this.m_layoutId & 0xFFFFFFFE) != 6)
                                {
                                    return -2;
                                }

                                gameObjectManager.RemoveGameObject(obstacle);
                                j -= 1;
                            }
                        }
                    }

                    for (int i = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding baseWallBlock = (LogicBuilding) buildings[i];

                        if (baseWallBlock.GetWallIndex() != 0)
                        {
                            int x = baseWallBlock.GetTileX();
                            int y = baseWallBlock.GetTileY();
                            int minX = 0;
                            int minY = 0;
                            int maxX = 0;
                            int maxY = 0;
                            int minWallBlockX = 0;
                            int minWallBlockY = 0;
                            int maxWallBlockX = 0;
                            int maxWallBlockY = 0;
                            int wallBlockCnt = 0;

                            for (int j = 0; j < buildings.Size(); j++)
                            {
                                LogicBuilding wallBlock = (LogicBuilding) buildings[j];

                                if (wallBlock.GetWallIndex() == baseWallBlock.GetWallIndex())
                                {
                                    int tmp1 = x - wallBlock.GetTileX();
                                    int tmp2 = y - wallBlock.GetTileY();

                                    minX = LogicMath.Min(minX, tmp1);
                                    minY = LogicMath.Min(minY, tmp2);
                                    maxX = LogicMath.Max(maxX, tmp1);
                                    maxY = LogicMath.Max(maxY, tmp2);

                                    int wallBlockX = wallBlock.GetBuildingData().GetWallBlockX(wallBlockCnt);
                                    int wallBlockY = wallBlock.GetBuildingData().GetWallBlockY(wallBlockCnt);

                                    minWallBlockX = LogicMath.Min(minWallBlockX, wallBlockX);
                                    minWallBlockY = LogicMath.Min(minWallBlockY, wallBlockY);
                                    maxWallBlockX = LogicMath.Max(maxWallBlockX, wallBlockX);
                                    maxWallBlockY = LogicMath.Max(maxWallBlockY, wallBlockY);

                                    ++wallBlockCnt;
                                }
                            }

                            if (baseWallBlock.GetBuildingData().GetWallBlockCount() != wallBlockCnt)
                            {
                                return -24;
                            }

                            int wallBlockSizeX = maxWallBlockX - minWallBlockX;
                            int wallBlockSizeY = maxWallBlockY - minWallBlockY;
                            int lengthX = maxX - minX;
                            int lengthY = maxY - minY;

                            if (wallBlockSizeX != lengthX || wallBlockSizeY != lengthY)
                            {
                                if (wallBlockSizeX != lengthX != (wallBlockSizeY != lengthY))
                                {
                                    return -25;
                                }
                            }
                        }
                    }

                    if (this.m_layoutType != 0)
                    {
                        if (level.IsWarBase())
                        {
                            level.SetWarBase(true);
                        }

                        level.SetActiveWarLayout(this.m_layoutId);
                    }
                    else
                    {
                        level.SetActiveLayout(this.m_layoutId, level.GetVillageType());

                        for (int i = 0; i < gameObjects.Size(); i++)
                        {
                            LogicGameObject gameObject = gameObjects[i];
                            LogicVector2 position = gameObject.GetPositionLayout(this.m_layoutId, false);

                            gameObject.SetPositionXY(position.m_x << 9, position.m_y << 9);
                        }
                    }

                    return 0;
                }
            }

            return -10;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_LAYOUT;
        }
    }
}