namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicMoveMultipleBuildingsCommand : LogicCommand
    {
        private LogicArrayList<int> m_xPositions;
        private LogicArrayList<int> m_yPositions;
        private LogicArrayList<int> m_gameObjectIds;

        public LogicMoveMultipleBuildingsCommand()
        {
            this.m_xPositions = new LogicArrayList<int>(500);
            this.m_yPositions = new LogicArrayList<int>(500);
            this.m_gameObjectIds = new LogicArrayList<int>(500);
        }

        public override void Decode(ByteStream stream)
        {
            int size = LogicMath.Min(stream.ReadInt(), 500);

            for (int i = size; i > 0; i--)
            {
                this.m_xPositions.Add(stream.ReadInt());
                this.m_yPositions.Add(stream.ReadInt());
                this.m_gameObjectIds.Add(stream.ReadInt());
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            int size = LogicMath.Min(this.m_gameObjectIds.Size(), 500);

            encoder.WriteInt(size);

            for (int i = 0; i < size; i++)
            {
                encoder.WriteInt(this.m_xPositions[i]);
                encoder.WriteInt(this.m_yPositions[i]);
                encoder.WriteInt(this.m_gameObjectIds[i]);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.MOVE_MULTIPLE_BUILDING;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_xPositions = null;
            this.m_yPositions = null;
            this.m_gameObjectIds = null;
        }

        public override int Execute(LogicLevel level)
        {
            int count = this.m_gameObjectIds.Size();

            if (count > 0)
            {
                bool validGameObjectType = true;

                if (this.m_xPositions.Size() == count && this.m_xPositions.Size() == count && count <= 500)
                {
                    LogicGameObject[] gameObjects = new LogicGameObject[count];

                    for (int i = 0; i < count; i++)
                    {
                        LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectIds[i]);

                        if (gameObject != null)
                        {
                            LogicGameObjectType gameObjectType = gameObject.GetGameObjectType();

                            if (gameObjectType != LogicGameObjectType.BUILDING &&
                                gameObjectType != LogicGameObjectType.TRAP &&
                                gameObjectType != LogicGameObjectType.DECO)
                            {
                                validGameObjectType = false;
                            }

                            gameObjects[i] = gameObject;
                        }
                        else
                        {
                            validGameObjectType = false;
                        }
                    }

                    if (validGameObjectType)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            LogicGameObject gameObject = gameObjects[i];

                            if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING && validGameObjectType)
                            {
                                LogicBuilding baseWallBlock = (LogicBuilding) gameObject;

                                if (baseWallBlock.GetWallIndex() != 0)
                                {
                                    int x = this.m_xPositions[i];
                                    int y = this.m_yPositions[i];
                                    int minX = 0;
                                    int minY = 0;
                                    int maxX = 0;
                                    int maxY = 0;
                                    int minWallBlockX = 0;
                                    int minWallBlockY = 0;
                                    int maxWallBlockX = 0;
                                    int maxWallBlockY = 0;
                                    int wallBlockCnt = 0;

                                    bool success = true;

                                    for (int j = 0; j < count; j++)
                                    {
                                        LogicGameObject obj = gameObjects[j];

                                        if (obj.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                        {
                                            LogicBuilding wallBlock = (LogicBuilding) obj;

                                            if (wallBlock.GetWallIndex() == baseWallBlock.GetWallIndex())
                                            {
                                                int tmp1 = x - this.m_xPositions[j];
                                                int tmp2 = y - this.m_yPositions[j];

                                                if ((x & this.m_xPositions[j]) != -1)
                                                {
                                                    success = false;
                                                }

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
                                    }

                                    if (baseWallBlock.GetBuildingData().GetWallBlockCount() == wallBlockCnt)
                                    {
                                        int wallBlockSizeX = maxWallBlockX - minWallBlockX;
                                        int wallBlockSizeY = maxWallBlockY - minWallBlockY;
                                        int lengthX = maxX - minX;
                                        int lengthY = maxY - minY;

                                        if (wallBlockSizeX != lengthX || wallBlockSizeY != lengthY)
                                        {
                                            if (!success && wallBlockSizeX != lengthX != (wallBlockSizeY != lengthY))
                                            {
                                                validGameObjectType = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debugger.Warning("EditModeInvalidGameObjectType");
                        return -1;
                    }

                    if (validGameObjectType)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int x = this.m_xPositions[i];
                            int y = this.m_yPositions[i];

                            LogicGameObject gameObject = gameObjects[i];

                            int maxX = x + gameObject.GetWidthInTiles();
                            int maxY = y + gameObject.GetHeightInTiles();

                            for (int j = 0; j < count; j++)
                            {
                                LogicGameObject gameObject2 = gameObjects[j];

                                if (gameObject2 != gameObject)
                                {
                                    int x2 = this.m_xPositions[j];
                                    int y2 = this.m_yPositions[j];
                                    int maxX2 = x2 + gameObject2.GetWidthInTiles();
                                    int maxY2 = y2 + gameObject2.GetHeightInTiles();

                                    if (maxX > x2 && maxY > y2 && x < maxX2 && y < maxY2)
                                    {
                                        Debugger.Warning("EditModeObjectsOverlap");
                                        return -1;
                                    }
                                }
                            }
                        }
                    }

                    if (validGameObjectType)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int x = this.m_xPositions[i];
                            int y = this.m_yPositions[i];

                            LogicGameObject gameObject = gameObjects[i];

                            int width = gameObject.GetWidthInTiles();
                            int height = gameObject.GetHeightInTiles();

                            if (!level.IsValidPlaceForBuildingWithIgnoreList(x, y, width, height, gameObjects, count))
                            {
                                Debugger.Warning("EditModeInvalidPosition");
                                return -1;
                            }
                        }
                    }

                    if (validGameObjectType)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int x = this.m_xPositions[i];
                            int y = this.m_yPositions[i];

                            LogicGameObject gameObject = gameObjects[i];

                            gameObject.SetPositionXY(x << 9, y << 9);
                        }

                        for (int i = 0; i < count; i++)
                        {
                            int x = this.m_xPositions[i];
                            int y = this.m_yPositions[i];

                            LogicGameObject gameObject = gameObjects[i];

                            int width = gameObject.GetWidthInTiles();
                            int height = gameObject.GetHeightInTiles();

                            for (int j = 0; j < width; j++)
                            {
                                for (int k = 0; k < height; k++)
                                {
                                    LogicObstacle tallGrass = level.GetTileMap().GetTile(x + j, y + k).GetTallGrass();

                                    if (tallGrass != null)
                                    {
                                        level.GetGameObjectManager().RemoveGameObject(tallGrass);
                                    }
                                }
                            }
                        }

                        if (level.GetHomeOwnerAvatar() != null)
                        {
                            LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                            if (homeOwnerAvatar.GetTownHallLevel() >= LogicDataTables.GetGlobals().GetChallengeBaseCooldownEnabledTownHall())
                            {
                                level.SetLayoutCooldownSecs(level.GetActiveLayout(level.GetVillageType()), LogicDataTables.GetGlobals().GetChallengeBaseSaveCooldown());
                            }
                        }

                        return 0;
                    }
                }
                else
                {
                    Debugger.Warning("EditModeSizeMismatch");
                }

                return -1;
            }

            return -92;
        }

        public void AddNewMove(int gameObjectId, int x, int y)
        {
            if (this.m_gameObjectIds.Size() < 500)
            {
                this.m_gameObjectIds.Add(gameObjectId);
                this.m_xPositions.Add(x);
                this.m_yPositions.Add(y);
            }
        }
    }
}