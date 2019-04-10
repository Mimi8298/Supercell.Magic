namespace Supercell.Magic.Logic.Util
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicPathFinderOld : LogicPathFinder
    {
        public const int SAVED_PATHS = 30;

        private bool m_costStrategyMode;

        private readonly int m_mapWidth;
        private readonly int m_mapHeight;
        private int m_costStrategy;
        private int m_pathLength;
        private int m_pathStartX;
        private int m_pathStartY;
        private int m_heapLength;
        private int m_savingPathIndex;
        private int m_maxHeapLength;

        private int[] m_pathState;
        private int[] m_parentBuffer;
        private int[] m_pathBuffer;
        private int[] m_heapBuffer;
        private int[] m_pathCost;

        private readonly LogicSavedPath[] m_savedPaths;

        public LogicPathFinderOld(LogicTileMap tileMap) : base(tileMap)
        {
            if (tileMap != null)
            {
                this.m_mapWidth = 2 * tileMap.GetSizeX();
                this.m_mapHeight = 2 * tileMap.GetSizeY();
            }
            else
            {
                this.m_mapWidth = 3;
                this.m_mapHeight = 4;
            }

            this.m_savedPaths = new LogicSavedPath[LogicPathFinderOld.SAVED_PATHS];

            for (int i = 0, j = this.m_mapWidth * 4; i < LogicPathFinderOld.SAVED_PATHS; i++)
            {
                this.m_savedPaths[i] = new LogicSavedPath(j);
            }

            int size = this.m_mapWidth * this.m_mapHeight;

            this.m_pathState = new int[size];
            this.m_heapBuffer = new int[size];
            this.m_parentBuffer = new int[size];
            this.m_pathBuffer = new int[size];
            this.m_pathCost = new int[size];

            this.ResetCostStrategyToDefault();
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_pathState = null;
            this.m_heapBuffer = null;
            this.m_parentBuffer = null;
            this.m_pathBuffer = null;
            this.m_pathCost = null;

            for (int i = 0; i < LogicPathFinderNew.SAVED_PATHS; i++)
            {
                if (this.m_savedPaths[i] != null)
                {
                    this.m_savedPaths[i].Destruct();
                    this.m_savedPaths[i] = null;
                }
            }
        }

        public unsafe void AStar(int startTile, int endTile)
        {
            for (int i = 0; i < LogicPathFinderNew.SAVED_PATHS; i++)
            {
                LogicSavedPath savedPath = this.m_savedPaths[i];

                if (savedPath.IsEqual(startTile, endTile, this.m_costStrategy))
                {
                    this.m_pathLength = savedPath.GetLength();
                    savedPath.ExtractPath(this.m_pathBuffer);
                    return;
                }
            }

            this.m_pathStartX = endTile % this.m_mapWidth;
            this.m_pathStartY = endTile / this.m_mapWidth;
            this.m_heapLength = 0;

            int mapSize = 4 * this.m_mapHeight * this.m_mapWidth;

            fixed (int* pathState = this.m_pathState)
            fixed (int* pathCost = this.m_pathCost)
            fixed (int* heapBuffer = this.m_heapBuffer)
            {
                for (int i = 0; i < mapSize; i++)
                {
                    pathState[i] = 0;
                    pathCost[i] = 0;
                    heapBuffer[i] = 0x7FFFFFFF;
                }
            }

            this.m_pathLength = 0;
            this.m_pathBuffer[this.m_pathLength++] = startTile;
            this.m_parentBuffer[startTile] = -1;
            this.m_parentBuffer[endTile] = -1;

            this.AStarAddTile(startTile);

            this.m_pathBuffer[0] = this.m_pathBuffer[this.m_pathLength-- - 1];
            this.m_pathState[startTile] = 2;

            if (this.m_heapLength != 0 && this.m_pathLength != 0)
            {
                do
                {
                    int removeSmallest = this.RemoveSmallest();
                    this.m_pathState[removeSmallest] = 2;
                    this.AStarAddTile(removeSmallest);
                } while (this.m_pathState[endTile] != 2 && this.m_heapLength > 0);

                this.m_pathLength = 0;

                if (endTile != -1)
                {
                    int parent = this.m_parentBuffer[endTile];

                    if (parent != -1)
                    {
                        this.m_pathBuffer[this.m_pathLength++] = endTile;

                        for (int i = this.m_parentBuffer[parent]; i != -1; parent = i, i = this.m_parentBuffer[i])
                        {
                            this.m_pathBuffer[this.m_pathLength++] = parent;
                        }
                    }
                }

                this.m_savingPathIndex += 1;

                if (this.m_savingPathIndex >= LogicPathFinderNew.SAVED_PATHS)
                {
                    this.m_savingPathIndex = 0;
                }

                this.m_savedPaths[this.m_savingPathIndex].StorePath(this.m_pathBuffer, this.m_pathLength, startTile, endTile, this.m_costStrategy);
            }
        }

        public void AStarAddTile(int tileIndex)
        {
            int tileX = tileIndex % this.m_mapWidth;
            int tileY = tileIndex / this.m_mapWidth;

            this.AStarAddTile(tileIndex, tileX, tileY - 1, tileIndex - this.m_mapWidth, 100);
            this.AStarAddTile(tileIndex, tileX, tileY + 1, tileIndex + this.m_mapWidth, 100);
            this.AStarAddTile(tileIndex, tileX - 1, tileY, tileIndex - 1, 100);
            this.AStarAddTile(tileIndex, tileX + 1, tileY, tileIndex + 1, 100);
            this.AStarAddTile(tileIndex, tileX - 1, tileY - 1, tileIndex - this.m_mapWidth - 1, 141);
            this.AStarAddTile(tileIndex, tileX - 1, tileY + 1, tileIndex + this.m_mapWidth - 1, 141);
            this.AStarAddTile(tileIndex, tileX + 1, tileY + 1, tileIndex + this.m_mapWidth + 1, 141);
            this.AStarAddTile(tileIndex, tileX + 1, tileY - 1, tileIndex - this.m_mapWidth + 1, 141);
        }

        public bool AStarAddTile(int tileIndex, int tileX, int tileY, int pathIdx, int cost)
        {
            if (tileX >= 0 && tileY >= 0 && this.m_mapWidth > tileX && this.m_mapHeight > tileY)
            {
                int state = this.m_pathState[pathIdx];

                if (state != 2)
                {
                    if (state != 0)
                    {
                        return true;
                    }

                    this.m_pathState[pathIdx] = 1;
                    this.m_pathBuffer[this.m_pathLength++] = pathIdx;

                    int pathFinderCost = this.m_tileMap.GetPathFinderCost(tileX, tileY);

                    if (pathFinderCost < (this.m_costStrategyMode ? 0x7FFFFFFF : 1))
                    {
                        int tileCost = this.m_pathCost[tileIndex] + cost + ((this.m_costStrategy * pathFinderCost) >> 8);

                        int tileDistanceX = tileX - this.m_pathStartX;
                        int tileDistanceY = tileY - this.m_pathStartY;

                        if (tileDistanceX < 1)
                        {
                            tileDistanceX = this.m_pathStartX - tileX;
                        }

                        if (tileDistanceY < 1)
                        {
                            tileDistanceY = this.m_pathStartY - tileY;
                        }

                        this.m_pathCost[tileIndex] = tileCost + 10 * (tileDistanceX + tileDistanceY);
                        this.Add(pathIdx);
                        this.m_maxHeapLength = this.m_maxHeapLength >= this.m_heapLength ? this.m_maxHeapLength : this.m_heapLength;

                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsCollision(int x, int y)
        {
            if (this.m_tileMap == null)
            {
                return false;
            }

            LogicTile tile = this.m_tileMap.GetTile(x >> 1, y >> 1);

            if (tile != null)
            {
                return !tile.IsPassablePathFinder((x & 1) + 2 * (y & 1));
            }

            return true;
        }

        public void Add(int tileIdx)
        {
            this.m_heapBuffer[this.m_heapLength++] = tileIdx;

            if (this.m_heapLength > 1)
            {
                int idx = this.m_heapLength - 1;

                do
                {
                    int prevHeapIdx = (idx - 1) >> 1;

                    int heap = this.m_heapBuffer[idx];
                    int prevHeap = this.m_heapBuffer[prevHeapIdx];

                    if (this.m_pathCost[heap] >= this.m_pathCost[prevHeap])
                    {
                        break;
                    }

                    this.m_heapBuffer[idx] = prevHeap;
                    this.m_heapBuffer[prevHeapIdx] = heap;

                    idx = prevHeapIdx;
                } while (idx > 0);
            }
        }

        public int RemoveSmallest()
        {
            if (this.m_heapLength == 0)
            {
                return -1;
            }

            int result = this.m_heapBuffer[0];
            int prevHeap = this.m_heapBuffer[--this.m_heapLength];

            this.m_heapBuffer[0] = prevHeap;

            int idx = 0;
            int nextIdx = 0;

            while (true)
            {
                int idx1 = 2 * idx + 1;
                int idx2 = 2 * idx + 2;

                if (idx2 < this.m_heapLength)
                {
                    if (this.m_pathCost[prevHeap] <= this.m_pathCost[this.m_heapBuffer[idx2]])
                    {
                        idx2 = idx;
                    }

                    nextIdx = idx2;
                }

                if (idx1 < this.m_heapLength)
                {
                    if (this.m_pathCost[this.m_heapBuffer[nextIdx]] > this.m_pathCost[this.m_heapBuffer[idx1]])
                    {
                        nextIdx = idx1;
                    }
                }

                if (nextIdx == idx)
                {
                    break;
                }

                this.m_heapBuffer[idx] = this.m_heapBuffer[nextIdx];
                this.m_heapBuffer[nextIdx] = prevHeap;

                idx = nextIdx;
            }

            return result;
        }

        public override int GetPathLength()
        {
            return this.m_pathLength;
        }

        public override void GetPathPoint(LogicVector2 position, int idx)
        {
            if (idx < 0 || this.m_pathLength <= idx)
            {
                Debugger.Error("illegal path index");
            }

            position.m_x = this.m_pathBuffer[idx] % this.m_mapWidth;
            position.m_y = this.m_pathBuffer[idx] / this.m_mapWidth;
        }

        public override void GetPathPointSubTile(LogicVector2 position, int idx)
        {
            Debugger.Warning("getPathPointSubTile() called. Should not be called ever for LogicPathFinderOld!");
        }

        public override void FindPath(LogicVector2 startPosition, LogicVector2 endPosition, bool clampPathFinderCost)
        {
            this.m_maxHeapLength = 0;

            if (this.m_tileMap.IsPassablePathFinder(startPosition.m_x, startPosition.m_y))
            {
                if (!this.IsReachable(endPosition.m_x, endPosition.m_y) && clampPathFinderCost)
                {
                    int distance = LogicMath.Sqrt((endPosition.m_x - startPosition.m_x) * (endPosition.m_x - startPosition.m_x) +
                                                  (endPosition.m_y - startPosition.m_y) * (endPosition.m_y - startPosition.m_y));
                    int lowDistanceX = LogicMath.Clamp(endPosition.m_x - distance, 0, 2 * this.m_mapWidth);
                    int lowDistanceY = LogicMath.Clamp(endPosition.m_y - distance, 0, 2 * this.m_mapHeight);
                    int highDistanceX = LogicMath.Clamp(endPosition.m_x + distance, 0, 2 * this.m_mapWidth);
                    int highDistanceY = LogicMath.Clamp(endPosition.m_y + distance, 0, 2 * this.m_mapHeight);

                    int minX = -1;
                    int minY = -1;
                    int minDistance = 0x7FFFFFFF;

                    while (lowDistanceX < highDistanceX)
                    {
                        int posX = lowDistanceX;

                        for (int posY = lowDistanceY; posY < highDistanceY; posY++)
                        {
                            if (this.IsReachable(posX, posY))
                            {
                                int pointDistance = (posX - endPosition.m_x) * (posX - endPosition.m_x) +
                                                    (posY - endPosition.m_y) * (posY - endPosition.m_y);

                                if (pointDistance < minDistance)
                                {
                                    minX = posX;
                                    minY = posY;
                                    minDistance = pointDistance;
                                }
                            }
                        }

                        ++lowDistanceX;
                    }

                    if (minX == -1)
                    {
                        this.m_pathLength = 0;
                        return;
                    }

                    endPosition.m_x = minX;
                    endPosition.m_y = minY;
                }

                if (this.IsReachable(endPosition.m_x, endPosition.m_y))
                {
                    int startTileIndex = startPosition.m_x + startPosition.m_y * this.m_mapWidth;
                    int endTileIndex = endPosition.m_x + endPosition.m_y * this.m_mapWidth;

                    if (this.IsLineOfSightClear(startPosition.m_x, startPosition.m_y, endPosition.m_x, endPosition.m_y))
                    {
                        this.m_pathLength = 0;
                        this.m_pathBuffer[this.m_pathLength++] = endTileIndex;
                        this.m_pathBuffer[this.m_pathLength++] = startTileIndex;
                    }
                    else
                    {
                        this.AStar(startTileIndex, endTileIndex);

                        if (this.m_pathLength > 0)
                        {
                            this.m_pathBuffer[this.m_pathLength++] = startTileIndex;
                        }
                    }
                }
                else
                {
                    this.m_pathLength = 0;
                }
            }
            else
            {
                this.m_pathLength = 0;
            }
        }

        public bool IsLineOfSightClear(int xA, int yA, int xB, int yB)
        {
            if (this.IsLineOfSightClearImpl(xA, yA, xB, yB))
            {
                int directionX = this.Sign(xB - xA);
                int directionY = this.Sign(yB - yA);

                return this.IsLineOfSightClearImpl(xA + directionX, yA, xB, yB - directionY) &&
                       this.IsLineOfSightClearImpl(xA, yA + directionY, xB - directionX, yB);
            }

            return false;
        }

        public bool IsLineOfSightClearImpl(int xA, int yA, int xB, int yB)
        {
            int directionX = xB > xA ? 1 : -1;
            int directionY = yB > yA ? 1 : -1;

            int distanceX = LogicMath.Abs(xB - xA);
            int distanceY = LogicMath.Abs(yB - yA);
            int direction = distanceX - distanceY;

            int subTileDistanceX = distanceX * 2;
            int subTileDistanceY = distanceY * 2;

            for (int i = distanceX + distanceY, posX = xA, posY = yA; i >= 0; i--)
            {
                if (this.IsCollision(posX, posY))
                {
                    return false;
                }

                if (direction > 0)
                {
                    direction -= subTileDistanceY;
                    posX += directionX;
                }
                else
                {
                    direction += subTileDistanceX;
                    posY += directionY;
                }
            }

            return true;
        }

        public override void SetCostStrategy(bool enabled, int quality)
        {
            this.m_costStrategyMode = enabled;
            this.m_costStrategy = quality;
        }

        public override void ResetCostStrategyToDefault()
        {
            this.m_costStrategyMode = true;
            this.m_costStrategy = 256;
        }

        public override void InvalidateCache()
        {
            for (int i = 0; i < LogicPathFinderNew.SAVED_PATHS; i++)
            {
                this.m_savedPaths[i].StorePath(null, 0, -1, -1, 0);
            }
        }

        public override int GetParent(int index)
        {
            Debugger.Warning("getParent(idx) should not be called for LogicPathFinderOld");
            return 0;
        }

        public int Sign(int value)
        {
            return value > 0 ? 1 : value >> 31;
        }

        public bool IsReachable(int x, int y)
        {
            return this.m_tileMap.GetPathFinderCost(x, y) < (this.m_costStrategyMode ? 0x7FFFFFFF : 1);
        }
    }
}