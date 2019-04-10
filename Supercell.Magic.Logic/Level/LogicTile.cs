namespace Supercell.Magic.Logic.Level
{
    using System.Runtime.CompilerServices;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicTile
    {
        private byte m_passableFlag;
        private readonly byte m_tileX;
        private readonly byte m_tileY;

        private short m_roomIndex;
        private int m_pathFinderCost;

        private LogicArrayList<LogicGameObject> m_gameObjects;

        public LogicTile(byte tileX, byte tileY)
        {
            this.m_gameObjects = new LogicArrayList<LogicGameObject>(4);
            this.m_tileX = tileX;
            this.m_tileY = tileY;
            this.m_passableFlag = 16;
            this.m_roomIndex = -1;
        }

        public void Destruct()
        {
            this.m_gameObjects = null;
            this.m_passableFlag = 16;
        }

        public void AddGameObject(LogicGameObject gameObject)
        {
            this.m_gameObjects.Add(gameObject);

            if (!gameObject.IsPassable())
            {
                this.m_passableFlag &= 0xEF;
            }

            this.RefreshSubTiles();
        }

        public bool IsPassablePathFinder(int x, int y)
        {
            return ((uint) x | (uint) y) <= 1 && ((1 << (x + 2 * y)) & this.m_passableFlag) == 0;
        }

        public bool IsPassablePathFinder(int pos)
        {
            return ((1 << pos) & this.m_passableFlag) == 0;
        }

        public bool IsBuildable(LogicGameObject gameObject)
        {
            for (int i = 0; i < this.m_gameObjects.Size(); i++)
            {
                LogicGameObject go = this.m_gameObjects[i];

                if (go != gameObject)
                {
                    if (!go.IsPassable() || go.IsUnbuildable())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsBuildableWithIgnoreList(LogicGameObject[] gameObjects, int count)
        {
            for (int i = 0, index = -1; i < this.m_gameObjects.Size(); i++, index = -1)
            {
                LogicGameObject go = this.m_gameObjects[i];

                for (int j = 0; j < count; j++)
                {
                    if (gameObjects[j] == go)
                    {
                        index = j;
                        break;
                    }
                }

                if (index == -1)
                {
                    if (!this.m_gameObjects[i].IsPassable() || this.m_gameObjects[i].IsUnbuildable())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RemoveGameObject(LogicGameObject gameObject)
        {
            int index = -1;

            for (int i = 0; i < this.m_gameObjects.Size(); i++)
            {
                if (this.m_gameObjects[i] == gameObject)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                this.m_gameObjects.Remove(index);
                this.RefreshPassableFlag();
            }
        }

        public LogicObstacle GetTallGrass()
        {
            for (int i = 0; i < this.m_gameObjects.Size(); i++)
            {
                LogicGameObject gameObject = this.m_gameObjects[i];

                if (gameObject.GetGameObjectType() == LogicGameObjectType.OBSTACLE)
                {
                    LogicObstacle obstacle = (LogicObstacle) this.m_gameObjects[i];

                    if (obstacle.GetObstacleData().IsTallGrass())
                    {
                        return obstacle;
                    }
                }
            }

            return null;
        }

        public LogicGameObject GetGameObject(int idx)
        {
            return this.m_gameObjects[idx];
        }

        public int GetGameObjectCount()
        {
            return this.m_gameObjects.Size();
        }

        public bool IsFullyNotPassable()
        {
            return (this.m_passableFlag & 0xF) == 0xF;
        }

        public short GetRoomIdx()
        {
            return this.m_roomIndex;
        }

        public void SetRoomIdx(short index)
        {
            this.m_roomIndex = index;
        }

        public int GetPathFinderCost(int x, int y)
        {
            if (this.m_pathFinderCost <= 0)
            {
                if (((uint) x | (uint) y) <= 1)
                {
                    if ((this.m_passableFlag & (1 << (x + 2 * y))) == 0)
                        return 0;
                }

                return 0x7FFFFFFF;
            }

            return this.m_pathFinderCost;
        }

        public int GetPathFinderCostIgnorePos(int x, int y)
        {
            if (this.m_pathFinderCost <= 0)
            {
                if ((this.m_passableFlag & (1 << (x + 2 * y))) == 0)
                    return 0;
                return 0x7FFFFFFF;
            }

            return this.m_pathFinderCost;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPathFinderCost()
        {
            if (this.m_pathFinderCost <= 0)
            {
                if (!this.IsFullyNotPassable())
                    return 0;
                return 0x7FFFFFFF;
            }

            return this.m_pathFinderCost;
        }

        public void RefreshSubTiles()
        {
            this.m_passableFlag &= 0xF0;
            this.m_pathFinderCost = 0;

            for (int i = 0; i < this.m_gameObjects.Size(); i++)
            {
                LogicGameObject gameObject = this.m_gameObjects[i];

                this.m_pathFinderCost = LogicMath.Max(this.m_pathFinderCost, gameObject.PathFinderCost());

                if (!gameObject.IsPassable())
                {
                    int width = gameObject.GetWidthInTiles();
                    int height = gameObject.GetWidthInTiles();

                    if (width == 1 || height == 1)
                    {
                        this.m_passableFlag |= 0xF;
                    }
                    else
                    {
                        int edge = gameObject.PassableSubtilesAtEdge();

                        int startX = 2 * (this.m_tileX - gameObject.GetTileX());
                        int startY = 2 * (this.m_tileY - gameObject.GetTileY());
                        int endX = 2 * width - edge;
                        int endY = 2 * height - edge;

                        for (int j = 0; j < 2; j++)
                        {
                            int offset = j;
                            int x = startX + j;

                            for (int k = 0; k < 2; k++)
                            {
                                int y = startY + k;

                                if (y < endY && x < endX && x >= edge && y >= edge)
                                {
                                    this.m_passableFlag |= (byte) (1 << offset);
                                }

                                offset += 2;
                            }
                        }
                    }
                }
            }
        }

        public void RefreshPassableFlag()
        {
            byte passableFlag = (byte) (this.m_passableFlag | 0x10);

            for (int i = 0; i < this.m_gameObjects.Size(); i++)
            {
                if (!this.m_gameObjects[i].IsPassable())
                {
                    passableFlag = (byte) (this.m_passableFlag & 0xEF);
                    break;
                }
            }

            this.m_passableFlag = passableFlag;
            this.RefreshSubTiles();
        }

        public byte GetPassableFlag()
        {
            return (byte) ((this.m_passableFlag >> 4) & 1);
        }

        public int GetX()
        {
            return this.m_tileX;
        }

        public int GetY()
        {
            return this.m_tileY;
        }

        public bool IsPassableFlag()
        {
            return (this.m_passableFlag & 16) >> 4 != 0;
        }

        public bool HasWall()
        {
            for (int i = 0; i < this.m_gameObjects.Size(); i++)
            {
                LogicGameObject gameObject = this.m_gameObjects[i];

                if (gameObject.IsWall() && gameObject.IsAlive())
                {
                    return true;
                }
            }

            return false;
        }
    }
}