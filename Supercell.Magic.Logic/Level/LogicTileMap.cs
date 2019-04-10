namespace Supercell.Magic.Logic.Level
{
    using System.Runtime.CompilerServices;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicTileMap
    {
        private bool m_roomEnabled;

        private readonly int m_sizeX;
        private readonly int m_sizeY;

        private LogicTile[] m_tiles;
        private LogicPathFinder m_pathFinder;

        private int[] m_openNodes;

        public LogicTileMap(int x, int y)
        {
            this.m_sizeX = x;
            this.m_sizeY = y;
            this.m_tiles = new LogicTile[x * y];

            for (int i = 0; i < this.m_tiles.Length; i++)
            {
                this.m_tiles[i] = new LogicTile((byte) (i % x), (byte) (i / x));
            }
        }

        public void Destruct()
        {
            if (this.m_tiles != null)
            {
                for (int i = 0; i < this.m_tiles.Length; i++)
                {
                    if (this.m_tiles[i] != null)
                    {
                        this.m_tiles[i].Destruct();
                        this.m_tiles[i] = null;
                    }
                }

                this.m_tiles = null;
            }

            this.m_openNodes = null;
        }

        public int GetSizeX()
        {
            return this.m_sizeX;
        }

        public int GetSizeY()
        {
            return this.m_sizeY;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogicTile GetTile(int x, int y)
        {
            if (this.m_sizeX > (uint) x && this.m_sizeY > (uint) y)
                return this.m_tiles[x + this.m_sizeX * y];
            return null;
        }

        public LogicPathFinder GetPathFinder()
        {
            if (this.m_pathFinder == null)
            {
                if (LogicDataTables.GetGlobals().UseNewPathFinder())
                {
                    this.m_pathFinder = new LogicPathFinderNew(this);
                }
                else
                {
                    this.m_pathFinder = new LogicPathFinderOld(this);
                }
            }

            return this.m_pathFinder;
        }

        public int GetPathFinderCost(int x, int y)
        {
            LogicTile tile = this.GetTile(x / 2, y / 2);

            if (tile != null)
                return tile.GetPathFinderCost(x % 2, y % 2);
            return 0x7FFFFFFF;
        }
        
        public int GetTilePathFinderCost(int tileX, int tileY)
        {
            LogicTile tile = this.GetTile(tileX, tileY);

            if (tile != null)
                return tile.GetPathFinderCost();
            return 0x7FFFFFFF;
        }

        public void AddGameObject(LogicGameObject gameObject)
        {
            if (gameObject.IsStaticObject())
            {
                int tileX = gameObject.GetTileX();
                int tileY = gameObject.GetTileY();

                if (tileX >= 0 && tileY >= 0)
                {
                    int sizeX = gameObject.GetWidthInTiles();
                    int sizeY = gameObject.GetHeightInTiles();

                    for (int i = 0; i < sizeY; i++)
                    {
                        for (int j = 0; j < sizeX; j++)
                        {
                            this.m_tiles[(tileX + j) + this.m_sizeX * (tileY + i)].AddGameObject(gameObject);
                        }
                    }

                    if (!gameObject.IsPassable())
                    {
                        this.UpdateRoomIndices();
                    }
                }
            }
        }

        public void GameObjectMoved(LogicGameObject gameObject, int prevTileX, int prevTileY)
        {
            if (gameObject.IsStaticObject())
            {
                int tileX = gameObject.GetTileX();
                int tileY = gameObject.GetTileY();

                if (tileX >= 0 && tileY >= 0)
                {
                    int sizeX = gameObject.GetWidthInTiles();
                    int sizeY = gameObject.GetHeightInTiles();

                    for (int i = 0; i < sizeY; i++)
                    {
                        for (int j = 0; j < sizeX; j++)
                        {
                            this.m_tiles[(prevTileX + j) + this.m_sizeX * (prevTileY + i)].RemoveGameObject(gameObject);
                        }
                    }

                    for (int i = 0; i < sizeY; i++)
                    {
                        for (int j = 0; j < sizeX; j++)
                        {
                            this.m_tiles[(tileX + j) + this.m_sizeX * (tileY + i)].AddGameObject(gameObject);
                        }
                    }

                    if (!gameObject.IsPassable())
                    {
                        this.UpdateRoomIndices();
                    }
                }
            }
        }

        public void RemoveGameObject(LogicGameObject gameObject)
        {
            if (gameObject.IsStaticObject())
            {
                int tileX = gameObject.GetTileX();
                int tileY = gameObject.GetTileY();

                if (tileX >= 0 && tileY >= 0)
                {
                    int sizeX = gameObject.GetWidthInTiles();
                    int sizeY = gameObject.GetHeightInTiles();

                    for (int i = 0; i < sizeY; i++)
                    {
                        for (int j = 0; j < sizeX; j++)
                        {
                            this.m_tiles[(tileX + j) + this.m_sizeX * (tileY + i)].RemoveGameObject(gameObject);
                        }
                    }

                    if (!gameObject.IsPassable())
                    {
                        this.UpdateRoomIndices();
                    }
                }
            }
        }

        public void EnableRoomIndices(bool enabled)
        {
            if (enabled && !this.m_roomEnabled)
            {
                this.m_roomEnabled = true;
                this.UpdateRoomIndices();
            }

            this.m_roomEnabled = enabled;
        }

        public void RefreshPassable(LogicGameObject gameObject)
        {
            if (gameObject.IsStaticObject())
            {
                int tileX = gameObject.GetTileX();
                int tileY = gameObject.GetTileY();

                if (tileX >= 0 && tileY >= 0)
                {
                    int sizeX = gameObject.GetWidthInTiles();
                    int sizeY = gameObject.GetHeightInTiles();

                    for (int i = 0; i < sizeY; i++)
                    {
                        for (int j = 0; j < sizeX; j++)
                        {
                            LogicTile tile = this.GetTile(tileX + j, tileY + i);
                            Debugger.DoAssert(tile != null, "illegal tile index");
                            tile.RefreshPassableFlag();
                        }
                    }

                    this.UpdateRoomIndices();
                }
            }
        }

        public void UpdateRoomIndices()
        {
            if (this.m_roomEnabled)
            {
                int tileCount = this.m_tiles.Length;

                for (int i = 0; i < tileCount; i++)
                {
                    this.m_tiles[i].SetRoomIdx(this.m_tiles[i].IsFullyNotPassable() ? (short) -1 : (short) 0);
                }

                short roomIdx = 1;

                for (int i = 0; i < tileCount; i++)
                {
                    if (this.m_tiles[i].GetRoomIdx() == 0)
                    {
                        this.FillRoom(i, roomIdx++);
                    }
                }
            }
        }

        public void FillRoom(int tileIndex, short roomIdx)
        {
            if (this.m_tiles[tileIndex].GetRoomIdx() == 0)
            {
                int fillCount = 1;
                int tileCount = this.m_sizeX * this.m_sizeY;

                if (this.m_openNodes == null)
                {
                    this.m_openNodes = new int[tileCount];
                }

                this.m_openNodes[0] = tileIndex;

                for (int i = 0; i < fillCount; i++)
                {
                    int openNode = this.m_openNodes[i];

                    if (this.m_tiles[openNode].GetRoomIdx() == 0)
                    {
                        int openNodeY = openNode / this.m_sizeX;
                        int openNodeX = openNode % this.m_sizeX;
                        int openNodeIdx = openNodeY * this.m_sizeX;

                        int openNodeStart = openNode - 1;
                        int openNodeEnd = openNode + 1;
                        int openNodeEndX = openNodeX + 1;

                        LogicTile tile;

                        do
                        {
                            if (openNodeX-- <= 0)
                                break;
                            tile = this.m_tiles[openNodeStart--];
                        } while (tile.GetRoomIdx() == 0);

                        while (openNodeEndX < this.m_sizeX)
                        {
                            if (this.m_tiles[openNodeEnd++].GetRoomIdx() != 0)
                            {
                                break;
                            }

                            openNodeEndX += 1;
                        }

                        if (openNodeX <= openNodeEndX)
                        {
                            int tileX = openNodeX;

                            do
                            {
                                if (tileX >= 0 && tileX < this.m_sizeX)
                                {
                                    if (tileX > openNodeX && tileX < openNodeEndX)
                                    {
                                        this.m_tiles[tileX + openNodeIdx].SetRoomIdx(roomIdx);
                                    }

                                    if (openNodeY > 0)
                                    {
                                        if (this.m_tiles[tileX + openNodeIdx - this.m_sizeX].GetRoomIdx() == 0)
                                        {
                                            this.m_openNodes[fillCount++] = tileX + openNodeIdx - this.m_sizeX;
                                        }
                                    }

                                    if (openNodeY < this.m_sizeY - 1)
                                    {
                                        if (this.m_tiles[tileX + openNodeIdx + this.m_sizeX].GetRoomIdx() == 0)
                                        {
                                            this.m_openNodes[fillCount++] = tileX + openNodeIdx + this.m_sizeX;
                                        }
                                    }
                                }
                            } while (tileX++ < openNodeEndX);
                        }
                    }
                }

                if (fillCount > tileCount)
                {
                    Debugger.Error("LogicTileMap::fillRoom - open nodes array overflowed.");
                }
            }
        }

        public bool IsPassablePathFinder(int x, int y)
        {
            LogicTile tile = this.GetTile(x / 2, y / 2);

            if (tile != null)
            {
                return tile.IsPassablePathFinder(x % 2, y % 2);
            }

            return false;
        }

        public bool IsValidAttackPos(int x, int y)
        {
            for (int i = 0, posX = x - 1; i < 2; i++, posX++)
            {
                for (int j = 0, posY = y - 1; j < 2; j++, posY++)
                {
                    LogicTile tile = this.GetTile(posX + i, posY + j);

                    if (tile != null)
                    {
                        for (int k = 0; k < tile.GetGameObjectCount(); k++)
                        {
                            LogicGameObject gameObject = tile.GetGameObject(k);

                            if (!gameObject.IsPassable())
                            {
                                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                {
                                    LogicBuilding building = (LogicBuilding) gameObject;

                                    if (!building.GetBuildingData().IsHidden())
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool GetNearestPassablePosition(int x, int y, LogicVector2 output, int radius)
        {
            int minDistance = -1;

            int startTileX = (x - radius) >> 8;
            int endTileX = (x + radius) >> 8;

            while (startTileX <= endTileX)
            {
                int startTileY = (y - radius) >> 8;
                int endTileY = (y + radius) >> 8;

                int posX = (startTileX << 8);
                int posY = (startTileY << 8);
                int distX = x - (startTileX << 8);
                int distY = y - (startTileY << 8);

                while (startTileY <= endTileY)
                {
                    if (this.IsPassablePathFinder(startTileX, startTileY))
                    {
                        int dist = LogicMath.Max(LogicMath.Abs(distX), LogicMath.Abs(distY));

                        if (minDistance < 0 || dist < minDistance)
                        {
                            minDistance = dist;
                            output.m_x = posX;
                            output.m_y = posY;
                        }
                    }

                    posY += 256;
                    distY -= 256;
                    startTileY += 1;
                }

                startTileX += 1;
            }

            return minDistance > -1;
        }

        public bool GetWallInPassableLine(int startX, int startY, int endX, int endY, LogicVector2 output)
        {
            int distanceX = endX - startX;
            int distanceY = endY - startY;
            int moveStepX;
            int moveStepY;
            int length;

            int wallX = 0;
            int wallY = 0;

            bool hasWall = false;

            if (LogicMath.Abs(distanceY) < LogicMath.Abs(distanceX))
            {
                moveStepX = distanceX > 0 ? 256 : -256;
                moveStepY = (distanceY << 8) / LogicMath.Abs(distanceX);
                length = distanceX / moveStepX;
            }
            else if (LogicMath.Abs(distanceX) >= LogicMath.Abs(distanceY))
            {
                moveStepX = distanceX > 0 ? 256 : -256;
                moveStepY = distanceY > 0 ? 256 : -256;
                length = distanceX / moveStepX;
            }
            else
            {
                moveStepX = (distanceX << 8) / LogicMath.Abs(distanceY);
                moveStepY = distanceY > 0 ? 256 : -256;
                length = distanceY / moveStepY;
            }

            LogicTile tile;

            for (int i = 0, posX = startX, posY = startY; i < length; i++)
            {
                int tileX = posX >> 9;
                int tileY = posY >> 9;

                tile = this.GetTile(tileX, tileY);

                if (tile != null)
                {
                    hasWall = tile.HasWall();

                    if (hasWall)
                    {
                        goto RESULT;
                    }
                }

                wallX = posX;
                wallY = posY;
                posX += moveStepX;
                posY += moveStepY;
            }

            int endTileX = endX >> 9;
            int endTileY = endY >> 9;

            tile = this.GetTile(endTileX, endTileY);

            if (tile != null)
            {
                hasWall = tile.HasWall();

                if (hasWall)
                {
                    goto RESULT;
                }
            }

            wallX = endX;
            wallY = endY;

            RESULT:

            output.m_x = wallX;
            output.m_y = wallY;

            return hasWall;
        }

        public bool GetPassablePositionInLine(int startX, int startY, int endX, int endY, int radius, LogicVector2 output)
        {
            int distanceX = endX - startX;
            int distanceY = endY - startY;
            int moveStepX;
            int moveStepY;
            int length;

            if (LogicMath.Abs(distanceY) < LogicMath.Abs(distanceX))
            {
                moveStepX = distanceX > 0 ? 64 : -64;
                moveStepY = (distanceY << 6) / LogicMath.Abs(distanceX);
                length = distanceX / moveStepX;
            }
            else if (LogicMath.Abs(distanceX) >= LogicMath.Abs(distanceY))
            {
                moveStepX = distanceX >= 0 && endX != startX ? 64 : -64;
                moveStepY = distanceY > 0 ? 64 : -64;
                length = distanceX / moveStepX;
            }
            else
            {
                moveStepX = (distanceX << 6) / LogicMath.Abs(distanceY);
                moveStepY = distanceY > 0 ? 64 : -64;
                length = distanceY / moveStepY;
            }

            int x = startX;
            int y = startY;

            for (int i = 0, l = 0; i < length; i++)
            {
                if (this.IsPassablePathFinder(x >> 8, y >> 8))
                {
                    output.m_x = x;
                    output.m_y = y;

                    return true;
                }

                l += moveStepX * moveStepX + moveStepY * moveStepY;

                if (l > radius * radius)
                {
                    return false;
                }

                x += moveStepX;
                y += moveStepY;
            }

            if (this.IsPassablePathFinder(endX >> 8, endY >> 8))
            {
                output.m_x = endX;
                output.m_y = endY;

                return true;
            }

            return false;
        }
    }
}