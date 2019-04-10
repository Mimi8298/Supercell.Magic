namespace Supercell.Magic.Logic.GameObject.Component
{
    using System;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicMovementSystem
    {
        private LogicMovementComponent m_parent;
        private LogicPathFinder m_pathFinder;

        private readonly LogicVector2 m_pushBackStartPosition; // 40
        private readonly LogicVector2 m_pushBackEndPosition; // 48
        private readonly LogicVector2 m_position; // 72
        private readonly LogicVector2 m_moveDistance; // 80
        private readonly LogicVector2 m_pathDistance; // 88
        private readonly LogicVector2 m_pathStartPosition; // 96
        private readonly LogicVector2 m_pathEndPosition; // 104
        private LogicArrayList<LogicVector2> m_wayPoints; // 12
        private readonly LogicArrayList<LogicVector2> m_path; // 120
        private LogicGameObject m_wall; // 132
        private LogicGameObject m_patrolPost; // 168

        private bool m_ignorePush;

        private int m_speed;
        private int m_direction;
        private int m_pathLength;

        private int m_slowTime;
        private int m_slowSpeed;
        private int m_wallCount;
        private int m_freezeTime; // 164
        private int m_pushTime; // 56
        private int m_pushInitTime; // 60
        private int m_patrolAreaCounter; // 8

        private readonly int[] m_boostTime;
        private readonly int[] m_boostSpeed;
        private readonly int[] m_preventsPushId;
        private readonly int[] m_preventsPushTime;

        public LogicMovementSystem()
        {
            this.m_pushBackStartPosition = new LogicVector2();
            this.m_pushBackEndPosition = new LogicVector2();
            this.m_position = new LogicVector2();
            this.m_moveDistance = new LogicVector2();
            this.m_pathDistance = new LogicVector2();
            this.m_pathStartPosition = new LogicVector2();
            this.m_pathEndPosition = new LogicVector2();
            this.m_wayPoints = new LogicArrayList<LogicVector2>();
            this.m_path = new LogicArrayList<LogicVector2>();

            this.m_preventsPushId = new int[3];
            this.m_preventsPushTime = new int[3];
            this.m_boostTime = new int[2];
            this.m_boostSpeed = new int[2];

            this.m_pushInitTime = 500;
        }

        public void Init(int speed, LogicMovementComponent parent, LogicPathFinder pathFinder)
        {
            this.m_parent = parent;
            this.m_pathFinder = pathFinder;

            if (parent != null && pathFinder != null)
            {
                Debugger.Error("LogicMovementSystem: both m_pParent and m_pPathFinder cant be used");
            }

            this.SetSpeed(speed);
        }

        public void Destruct()
        {
            this.ClearPath();
            this.ClearPatrolArea();

            this.m_pushBackStartPosition.Destruct();
            this.m_pushBackEndPosition.Destruct();
            this.m_moveDistance.Destruct();
            this.m_position.Destruct();
            this.m_pathDistance.Destruct();
            this.m_pathStartPosition.Destruct();
            this.m_pathEndPosition.Destruct();

            this.m_pathLength = 0;
        }

        public int GetSpeed()
        {
            int speed = this.m_speed;

            if (this.m_boostTime[0] > 0)
            {
                speed += this.m_boostSpeed[0];
            }

            if (this.m_slowTime > 0)
            {
                speed = (int) ((long) speed * (this.m_slowSpeed + 100) / 100);
            }

            if (this.m_parent != null && this.m_parent.GetParent().IsFrozen())
            {
                speed = 0;
            }

            return LogicMath.Max(0, speed);
        }

        public void SetSpeed(int speed)
        {
            this.m_speed = 16 * speed / 1000;
        }

        public int GetDirection()
        {
            return this.m_direction;
        }

        public void SetDirection(int angle)
        {
            this.m_direction = angle;
        }

        public bool IgnorePush()
        {
            return this.m_ignorePush;
        }

        public LogicVector2 GetPosition()
        {
            return this.m_position;
        }

        public LogicVector2 GetPathEndPosition()
        {
            return this.m_pathEndPosition;
        }

        public int GetPathLength()
        {
            return this.m_pathLength;
        }

        public void ClearPath()
        {
            this.m_wall = null;
            this.m_wallCount = 0;

            for (int i = this.m_path.Size() - 1; i >= 0; i--)
            {
                this.m_path[i].Destruct();
                this.m_path.Remove(i);
            }

            this.m_pathLength = 0;
        }

        public int GetWallCount()
        {
            return this.m_wallCount;
        }

        public bool NotMoving()
        {
            return this.m_path.Size() == 0;
        }

        public void Reset(int x, int y)
        {
            this.m_position.Set(x, y);

            if (this.m_parent != null)
            {
                this.ValidatePos();
            }
        }

        public void ValidatePos()
        {
            /*
            if (this.m_parent != null && !this.m_parent.IsFlying() && !this.m_parent.IsUnderground() && this.m_parent.GetJump() <= 0 && !this.m_ignorePush)
            {
                this.m_parent.GetParent().GetLevel().GetTileMap().IsPassablePathFinder(this.m_position.m_x >> 8, this.m_position.m_y >> 8);
            }
            */
        }

        public void CalculatePathLength()
        {
            this.m_pathLength = 0;

            if (this.m_path.Size() > 0)
            {
                this.m_pathLength += this.m_position.GetDistance(this.m_path[this.m_path.Size() - 1]);

                for (int i = 0; i < this.m_path.Size() - 1; i++)
                {
                    this.m_pathLength += this.m_path[i].GetDistance(this.m_path[i + 1]);
                }
            }
        }

        public void CalculateDirection(LogicVector2 pos)
        {
            LogicVector2 nextPath = this.m_path[this.m_path.Size() - 1];

            pos.m_x = nextPath.m_x - this.m_position.m_x;
            pos.m_y = nextPath.m_y - this.m_position.m_y;

            this.m_direction = pos.GetAngle();
        }

        public void ClearPatrolArea()
        {
            if (this.m_wayPoints != null)
            {
                while (this.m_wayPoints.Size() > 0)
                {
                    LogicVector2 wayPoint = this.m_wayPoints[this.m_wayPoints.Size() - 1];

                    if (wayPoint == null)
                    {
                        Debugger.Error("LogicMovementSystem::calculatePatrolArea: removed waypoint is NULL");
                    }

                    wayPoint.Destruct();

                    this.m_wayPoints.Remove(this.m_wayPoints.Size() - 1);
                }
            }

            this.m_wayPoints = null;
        }

        public int GetDistSqToEnd()
        {
            if (this.m_path.Size() > 0)
            {
                return this.m_position.GetDistanceSquared(this.m_path[0]);
            }

            return 0;
        }

        public void AddPoint(int x, int y)
        {
            this.ClearPath();

            this.m_path.Add(new LogicVector2(x, y));

            this.CalculatePathLength();
            this.CalculateDirection(this.m_pathDistance);
        }

        public void PopTarget()
        {
            int idx = this.m_path.Size() - 1;

            this.m_path[idx].Destruct();
            this.m_path.Remove(idx);
        }

        public void SubTick()
        {
            this.UpdateMovement(this.m_parent.GetParent().GetLevel());
        }

        public LogicGameObject GetWall()
        {
            return this.m_wall;
        }

        public void UpdateMovement(LogicLevel level)
        {
            if (this.m_boostTime[0] > 0)
            {
                this.m_boostTime[0] -= 1;

                if (this.m_boostTime[0] == 0)
                {
                    this.m_boostTime[0] = this.m_boostTime[1];
                    this.m_boostSpeed[0] = this.m_boostSpeed[1];

                    this.m_boostTime[1] = 0;
                    this.m_boostSpeed[1] = 0;
                }
            }

            if (this.m_slowTime > 0)
            {
                this.m_slowTime -= 1;

                if (this.m_slowTime == 0)
                {
                    this.m_slowSpeed = 0;
                }
            }

            if (this.m_freezeTime <= 0)
            {
                if (this.m_pushTime <= 0)
                {
                    if (this.m_path.Size() != 0)
                    {
                        int speed = this.GetSpeed();

                        while (speed > 0)
                        {
                            if (this.m_path.Size() == 0)
                            {
                                break;
                            }

                            LogicVector2 path = this.m_path[this.m_path.Size() - 1];

                            int distanceX = path.m_x - this.m_position.m_x;
                            int distanceY = path.m_y - this.m_position.m_y;

                            this.m_moveDistance.m_x = distanceX;
                            this.m_moveDistance.m_y = distanceY;

                            int length = this.m_moveDistance.Normalize(speed);

                            if (length > speed)
                            {
                                if (distanceX != 0 && this.m_moveDistance.m_x == 0)
                                {
                                    this.m_moveDistance.m_x = distanceX <= 0 ? -1 : 1;
                                }

                                if (distanceY != 0 && this.m_moveDistance.m_y == 0)
                                {
                                    this.m_moveDistance.m_y = distanceY <= 0 ? -1 : 1;
                                }

                                this.SetPosition(this.m_position.m_x + this.m_moveDistance.m_x, this.m_position.m_y + this.m_moveDistance.m_y);
                                this.m_pathLength += this.m_position.GetDistance(path) - length;
                                this.ValidatePos();

                                break;
                            }

                            this.SetPosition(path.m_x, path.m_y);
                            this.m_pathLength += this.m_position.GetDistance(path) - length;
                            this.ValidatePos();
                            this.PopTarget();

                            if (this.m_path.Size() == 0)
                            {
                                this.m_pathLength = 0;
                                this.UpdatePatrolArea(level);

                                break;
                            }

                            this.CalculateDirection(this.m_moveDistance);

                            speed -= this.m_moveDistance.GetLength();
                        }
                    }
                }
                else
                {
                    this.UpdatePushBack();
                }

                this.ValidatePos();

                if (this.m_parent != null && this.m_parent.GetParent().IsFrozen())
                {
                    if (this.m_path.Size() != 0)
                    {
                        this.ValidatePos();
                        this.ClearPath();
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    if (this.m_preventsPushTime[i] > 0)
                    {
                        this.m_preventsPushTime[i] -= 16;

                        if (this.m_preventsPushTime[i] <= 0)
                        {
                            this.m_preventsPushTime[i] = 0;
                            this.m_preventsPushId[i] = 0;
                        }
                    }
                }
            }
            else
            {
                this.m_freezeTime = LogicMath.Max(this.m_freezeTime - 16, 0);
            }
        }

        public void UpdatePatrolArea(LogicLevel level)
        {
            if (this.m_wayPoints != null)
            {
                int wayPointCount = this.m_wayPoints.Size();

                if (wayPointCount > 0 && this.m_pathLength == 0)
                {
                    for (int i = wayPointCount - 1; i >= 0; i--)
                    {
                        this.m_patrolAreaCounter = (this.m_patrolAreaCounter + 1) % wayPointCount;

                        if (this.MoveTo(this.m_wayPoints[this.m_patrolAreaCounter].m_x, this.m_wayPoints[this.m_patrolAreaCounter].m_y, level.GetTileMap(), true))
                        {
                            break;
                        }
                    }

                    if (this.m_patrolPost != null && this.m_patrolPost.GetDefenceUnitProduction() != null)
                    {
                        this.m_freezeTime = (level.IsInCombatState() ? 100 : 5000) + level.GetLogicTime().GetTick() % (level.IsInCombatState() ? 200 : 3000);
                    }
                }
            }
        }

        public void UpdatePushBack()
        {
            int startSpeed = this.m_pushTime * this.m_pushTime / this.m_pushInitTime;
            int endSpeed = this.m_pushInitTime - startSpeed;
            int pushBackX = (startSpeed * this.m_pushBackStartPosition.m_x + endSpeed * this.m_pushBackEndPosition.m_x) / this.m_pushInitTime;
            int pushBackY = (startSpeed * this.m_pushBackStartPosition.m_y + endSpeed * this.m_pushBackEndPosition.m_y) / this.m_pushInitTime;

            if (this.m_parent == null || this.m_parent.IsFlying() || this.m_parent.GetParent().GetLevel().GetTileMap().IsPassablePathFinder(pushBackX >> 8, pushBackY >> 8) ||
                this.m_ignorePush)
            {
                this.SetPosition(pushBackX, pushBackY);
            }
            else
            {
                this.m_pushBackStartPosition.m_x = this.m_position.m_x;
                this.m_pushBackStartPosition.m_y = this.m_position.m_y;
                this.m_pushBackEndPosition.m_x = this.m_position.m_x;
                this.m_pushBackEndPosition.m_y = this.m_position.m_y;
            }

            this.m_pushTime = LogicMath.Max(this.m_pushTime - 16, 0);

            if (this.m_pushTime == 0)
            {
                LogicGameObject parent = this.m_parent.GetParent();
                LogicCombatComponent combatComponent = parent.GetCombatComponent();

                if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) parent;

                    if (character.GetCharacterData().GetPickNewTargetAfterPushback() || this.m_ignorePush)
                    {
                        if (combatComponent != null)
                        {
                            combatComponent.ForceNewTarget();
                        }
                    }
                }

                this.m_parent.NewTargetFound();

                if (combatComponent != null)
                {
                    combatComponent.StopAttack();
                }

                this.m_ignorePush = false;
            }
        }

        public void SetPosition(int x, int y)
        {
            if (this.m_parent != null)
            {
                if (!this.m_parent.IsFlying() &&
                    !this.m_parent.IsUnderground() &&
                    this.m_parent.GetJump() <= 0 &&
                    !this.m_ignorePush)
                {
                    this.ValidatePos();

                    if (this.m_position.m_x >> 8 == x >> 8)
                    {
                        if ((this.m_position.m_y ^ (uint) y) < 256)
                        {
                            goto set;
                        }
                    }

                    LogicTileMap tileMap = this.m_parent.GetParent().GetLevel().GetTileMap();

                    int pathFinderX = x >> 8;
                    int pathFinderY = y >> 8;

                    if (!tileMap.IsPassablePathFinder(pathFinderX, pathFinderY))
                    {
                        LogicTile tile = tileMap.GetTile(pathFinderX / 2, pathFinderY / 2);

                        if (LogicDataTables.GetGlobals().JumpWhenHitJumpable())
                        {
                            bool allowJump = false;

                            for (int i = 0; i < tile.GetGameObjectCount(); i++)
                            {
                                LogicGameObject gameObject = tile.GetGameObject(i);

                                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                {
                                    LogicBuilding building = (LogicBuilding) gameObject;

                                    if (building.GetHitWallDelay() > 0)
                                    {
                                        allowJump = true;
                                    }
                                }
                            }

                            if (allowJump)
                            {
                                this.m_position.m_x = x;
                                this.m_position.m_y = y;

                                this.m_parent.EnableJump(128);

                                return;
                            }
                        }

                        if (LogicDataTables.GetGlobals().SlideAlongObstacles())
                        {
                            throw new NotImplementedException(); // TODO: Implement this.
                        }
                        else
                        {
                            x = LogicMath.Clamp(x, (int) (this.m_position.m_x & 0xFFFFFF00), this.m_position.m_x | 0xFF);
                            y = LogicMath.Clamp(y, (int) (this.m_position.m_y & 0xFFFFFF00), this.m_position.m_y | 0xFF);
                        }

                        this.m_position.m_x = x;
                        this.m_position.m_y = y;

                        this.ValidatePos();
                        return;
                    }
                }
            }

            set:

            this.m_position.m_x = x;
            this.m_position.m_y = y;
        }

        public bool IsPushed()
        {
            return this.m_pushTime > 0;
        }

        public bool MoveTo(int x, int y, LogicTileMap tileMap, bool defaultEndPoint)
        {
            this.ClearPath();

            if (this.m_parent != null)
            {
                if (this.m_parent.GetParent().IsFrozen())
                {
                    return false;
                }
            }

            this.m_wall = null;
            this.m_wallCount = 0;

            this.m_pathStartPosition.m_x = this.m_position.m_x >> 8;
            this.m_pathStartPosition.m_y = this.m_position.m_y >> 8;
            this.m_pathEndPosition.m_x = x >> 8;
            this.m_pathEndPosition.m_y = y >> 8;

            this.m_pathStartPosition.m_x = LogicMath.Clamp(this.m_pathStartPosition.m_x, 0, 99);
            this.m_pathStartPosition.m_y = LogicMath.Clamp(this.m_pathStartPosition.m_y, 0, 99);
            this.m_pathEndPosition.m_x = LogicMath.Clamp(this.m_pathEndPosition.m_x, 0, 99);
            this.m_pathEndPosition.m_y = LogicMath.Clamp(this.m_pathEndPosition.m_y, 0, 99);

            LogicPathFinder pathFinder;

            if (this.m_parent == null)
            {
                pathFinder = this.m_pathFinder;
                pathFinder.ResetCostStrategyToDefault();
            }
            else
            {
                bool resetStrategyCost = true;
                int strategyCost = 256;

                LogicGameObject parent = this.m_parent.GetParent();
                LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();

                if (hitpointComponent != null)
                {
                    if (hitpointComponent.GetTeam() == 1)
                    {
                        resetStrategyCost = false;
                        strategyCost = 768;
                    }
                }

                if (this.m_parent.CanJumpWall())
                {
                    resetStrategyCost = false;
                    strategyCost = 16;
                }

                if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) parent;

                    if (character.IsWallBreaker())
                    {
                        resetStrategyCost = false;
                        strategyCost = 128;
                    }
                }

                pathFinder = tileMap.GetPathFinder();

                if (resetStrategyCost)
                {
                    pathFinder.ResetCostStrategyToDefault();
                }
                else
                {
                    pathFinder.SetCostStrategy(true, strategyCost);
                }

                pathFinder.FindPath(this.m_pathStartPosition, this.m_pathEndPosition, true);
                pathFinder.GetPathLength();

                int pathLength = pathFinder.GetPathLength();

                this.m_path.EnsureCapacity(pathLength + 1);

                if (pathLength != 0 && defaultEndPoint)
                {
                    LogicVector2 pathPoint = new LogicVector2(x, y);

                    this.CheckWall(pathPoint);
                    this.m_path.Add(pathPoint);
                }

                if (LogicDataTables.GetGlobals().UseNewPathFinder())
                {
                    LogicTileMap pathFinderTileMap = pathFinder.GetTileMap();

                    int width = 2 * pathFinderTileMap.GetSizeX();
                    int height = 2 * pathFinderTileMap.GetSizeY();

                    int startTileIdx = this.m_pathStartPosition.m_x + width * this.m_pathStartPosition.m_y;
                    int endTileIdx = this.m_pathEndPosition.m_x + width * this.m_pathEndPosition.m_y;

                    if (!defaultEndPoint)
                    {
                        LogicVector2 pathPoint = new LogicVector2((endTileIdx % width) << 8, (endTileIdx / height) << 8);

                        this.CheckWall(pathPoint);
                        this.m_path.Add(pathPoint);
                    }

                    if (pathLength > 0 && !pathFinder.IsLineOfSightClear())
                    {
                        int iterationCount = 0;

                        while (endTileIdx != startTileIdx && endTileIdx != -1)
                        {
                            endTileIdx = pathFinder.GetParent(endTileIdx);

                            if (endTileIdx != startTileIdx && endTileIdx != -1)
                            {
                                LogicVector2 pathPoint = new LogicVector2((endTileIdx % width) << 8, (endTileIdx / height) << 8);

                                pathPoint.m_x += 128;
                                pathPoint.m_y += 128;

                                this.CheckWall(pathPoint);
                                this.m_path.Add(pathPoint);

                                if (iterationCount >= 100000)
                                {
                                    Debugger.Warning("LMSystem: iteration count > 100000");
                                    break;
                                }
                            }

                            iterationCount += 1;
                        }
                    }
                }
                else
                {
                    for (int i = -pathLength, j = 0; j + i != 0; j++)
                    {
                        LogicVector2 pathPoint = new LogicVector2();

                        pathFinder.GetPathPoint(pathPoint, i + j);

                        if (i + j == -1 && this.m_pathStartPosition.Equals(pathPoint))
                        {
                            pathPoint.Destruct();
                            pathPoint = null;
                        }
                        else
                        {
                            if (j != 0 || !this.m_pathStartPosition.Equals(pathPoint))
                            {
                                pathPoint.m_x = (pathPoint.m_x << 8) | 128;
                                pathPoint.m_y = (pathPoint.m_y << 8) | 128;
                            }
                            else
                            {
                                pathPoint.m_x = x;
                                pathPoint.m_y = y;
                            }

                            this.CheckWall(pathPoint);
                            this.m_path.Add(pathPoint);
                        }
                    }
                }
            }

            this.CalculatePathLength();

            if (this.m_path.Size() > 0)
            {
                this.CalculateDirection(this.m_pathDistance);
                return true;
            }

            return false;
        }

        public void CheckWall(LogicVector2 position)
        {
            if (this.m_parent != null)
            {
                LogicGameObject gameObject = this.m_parent.GetParent();
                LogicTile tile = gameObject.GetLevel().GetTileMap().GetTile(position.m_x >> 9, position.m_y >> 9);

                if (tile != null)
                {
                    for (int i = 0; i < tile.GetGameObjectCount(); i++)
                    {
                        LogicGameObject go = tile.GetGameObject(i);

                        if (go.IsWall() &&
                            go.IsAlive())
                        {
                            this.m_wall = go;

                            if (((LogicBuilding) go).GetHitWallDelay() <= 0)
                            {
                                ++this.m_wallCount;
                            }
                        }
                    }
                }
            }
        }

        public void PushBack(LogicVector2 position, int speed, int unk1, int id, bool ignoreWeight, bool gravity)
        {
            if (speed > 0 && this.m_pushTime <= 0)
            {
                if (this.m_parent != null && this.m_parent.GetJump() <= 0 && !this.m_parent.GetParent().IsHero())
                {
                    if (id != 0)
                    {
                        int idx = -1;

                        for (int k = 0; k < 3; k++)
                        {
                            if (this.m_preventsPushId[k] == id)
                            {
                                return;
                            }

                            if (this.m_preventsPushTime[k] == 0)
                            {
                                idx = k;
                            }
                        }

                        if (idx == -1)
                        {
                            return;
                        }

                        this.m_preventsPushId[idx] = id;
                        this.m_preventsPushTime[idx] = 1500;
                    }

                    LogicGameObject parent = this.m_parent.GetParent();
                    LogicGameObjectData data = parent.GetData();

                    int housingSpace = 1;

                    if (data.GetDataType() == LogicDataType.CHARACTER)
                    {
                        housingSpace = ((LogicCombatItemData) data).GetHousingSpace();

                        if (housingSpace >= 4 && !ignoreWeight)
                        {
                            return;
                        }
                    }

                    int pushForce = 256;

                    if (100 / unk1 != 0)
                    {
                        pushForce = 256 / (100 / unk1);
                    }

                    if (gravity)
                    {
                        pushForce = (LogicMath.Min(speed, 5000) << 8) / 5000 / housingSpace;
                    }

                    int rndX = parent.Rand(100) & 0x7F;
                    int rndY = parent.Rand(200) & 0x7F;
                    int pushBackX = rndX + position.m_x - 0x3F;
                    int pushBackY = rndY + position.m_y - 0x3F;
                    int pushBackTime = (1000 * pushForce) >> 8;

                    this.m_pushTime = pushBackTime;
                    this.m_pushInitTime = pushBackTime;
                    this.m_ignorePush = false;
                    this.m_pushBackStartPosition.m_x = this.m_position.m_x;
                    this.m_pushBackStartPosition.m_y = this.m_position.m_y;

                    pushForce *= 2;

                    this.m_pushBackEndPosition.m_x = this.m_position.m_x + ((pushForce * pushBackX) >> 8);
                    this.m_pushBackEndPosition.m_y = this.m_position.m_y + ((pushForce * pushBackY) >> 8);

                    int angle = position.GetAngle();
                    int direction = angle <= 180 ? 180 : -180;

                    this.m_direction = direction + angle;
                }
            }
        }

        public bool ManualPushBack(LogicVector2 position, int speed, int time, int id)
        {
            if (speed > 0)
            {
                if (this.m_parent != null && this.m_parent.GetJump() <= 0)
                {
                    if (id != 0)
                    {
                        int idx = -1;

                        for (int k = 0; k < 3; k++)
                        {
                            if (this.m_preventsPushId[k] == id)
                            {
                                return false;
                            }

                            if (this.m_preventsPushTime[k] == 0)
                            {
                                idx = k;
                            }
                        }

                        if (idx == -1)
                        {
                            return false;
                        }

                        this.m_preventsPushId[idx] = id;
                        this.m_preventsPushTime[idx] = 1500;
                    }

                    LogicGameObject parent = this.m_parent.GetParent();

                    int rndX = parent.Rand(100) & 0x7F;
                    int rndY = parent.Rand(200) & 0x7F;

                    int pushBackX = rndX + position.m_x - 0x3F;
                    int pushBackY = rndY + position.m_y - 0x3F;

                    LogicVector2 pushForce = new LogicVector2((2 * speed * pushBackX) >> 8, (2 * speed * pushBackY) >> 8);

                    int prevPushBackTime = this.m_pushTime;

                    if (prevPushBackTime <= 0)
                    {
                        this.m_pushTime = time - 16;
                        this.m_pushInitTime = time;

                        this.m_ignorePush = false;

                        this.m_pushBackStartPosition.m_x = this.m_position.m_x;
                        this.m_pushBackStartPosition.m_y = this.m_position.m_y;

                        this.m_pushBackEndPosition.m_x = this.m_position.m_x + pushForce.m_x;
                        this.m_pushBackEndPosition.m_y = this.m_position.m_y + pushForce.m_y;
                    }
                    else
                    {
                        LogicVector2 prevPushForce = new LogicVector2(this.m_pushBackEndPosition.m_x - this.m_position.m_x, this.m_pushBackEndPosition.m_y - this.m_position.m_y);

                        this.m_pushTime = prevPushBackTime + time - 16;
                        this.m_pushInitTime = prevPushBackTime + time;

                        this.m_ignorePush = false;

                        pushForce.Add(prevPushForce);

                        this.m_pushBackStartPosition.m_x = this.m_position.m_x;
                        this.m_pushBackStartPosition.m_y = this.m_position.m_y;
                        this.m_pushBackEndPosition.m_x = this.m_position.m_x + pushForce.m_x;
                        this.m_pushBackEndPosition.m_y = this.m_position.m_y + pushForce.m_y;

                        prevPushForce.Destruct();
                    }

                    return true;
                }
            }

            return false;
        }

        public void PushTrap(LogicVector2 position, int time, int id, bool ignorePrevPush, bool verifyPushPosition)
        {
            if (this.m_pushTime <= 0 || ignorePrevPush)
            {
                if (this.m_parent != null && this.m_parent.GetJump() <= 0 && !this.m_parent.GetParent().IsHero())
                {
                    LogicGameObject parent = this.m_parent.GetParent();

                    if (!parent.IsHero())
                    {
                        if (id != 0 && !ignorePrevPush)
                        {
                            int idx = -1;

                            for (int k = 0; k < 3; k++)
                            {
                                if (this.m_preventsPushId[k] == id)
                                {
                                    return;
                                }

                                if (this.m_preventsPushTime[k] == 0)
                                {
                                    idx = k;
                                }
                            }

                            if (idx == -1)
                            {
                                return;
                            }

                            this.m_preventsPushId[idx] = id;
                            this.m_preventsPushTime[idx] = 1500;
                        }

                        this.m_pushTime = time;
                        this.m_pushInitTime = time;
                        this.m_pushBackStartPosition.m_x = this.m_position.m_x;
                        this.m_pushBackStartPosition.m_y = this.m_position.m_y;
                        this.m_pushBackEndPosition.m_x = this.m_position.m_x + position.m_x;
                        this.m_pushBackEndPosition.m_y = this.m_position.m_y + position.m_y;

                        if (verifyPushPosition)
                        {
                            int pushBackEndPositionX = this.m_pushBackEndPosition.m_x;
                            int pushBackEndPositionY = this.m_pushBackEndPosition.m_y;

                            if (LogicMath.Max(LogicMath.Abs(position.m_x), LogicMath.Abs(position.m_y)) != 0)
                            {
                                LogicTileMap tileMap = parent.GetLevel().GetTileMap();

                                if (!tileMap.IsPassablePathFinder(pushBackEndPositionX >> 8, pushBackEndPositionY >> 8))
                                {
                                    LogicVector2 pos = new LogicVector2();
                                    LogicRandom rnd = new LogicRandom(pushBackEndPositionX + pushBackEndPositionY);

                                    tileMap.GetNearestPassablePosition(pushBackEndPositionX + rnd.Rand(512) - 256,
                                                                       pushBackEndPositionY + rnd.Rand(512) - 256, pos, 2048);

                                    pushBackEndPositionX = pos.m_x;
                                    pushBackEndPositionY = pos.m_y;
                                }

                                if (!tileMap.IsPassablePathFinder(pushBackEndPositionX >> 8, pushBackEndPositionY >> 8))
                                {
                                    Debugger.Warning("PushTrap->ended on inmovable");
                                }
                            }

                            this.m_pushBackEndPosition.m_x = pushBackEndPositionX;
                            this.m_pushBackEndPosition.m_y = pushBackEndPositionY;
                        }

                        this.m_ignorePush = verifyPushPosition;

                        int angle = position.GetAngle();
                        this.m_direction = angle + (angle <= 180 ? 180 : -180);
                    }
                }
            }
        }

        public bool ManualPushTrap(LogicVector2 position, int time, int id)
        {
            if (this.m_parent != null)
            {
                if (this.m_parent.GetJump() <= 0 && !this.m_parent.GetParent().IsHero())
                {
                    if (id != 0)
                    {
                        int idx = -1;

                        for (int k = 0; k < 3; k++)
                        {
                            if (this.m_preventsPushId[k] == id)
                            {
                                return false;
                            }

                            if (this.m_preventsPushTime[k] == 0)
                            {
                                idx = k;
                            }
                        }

                        if (idx == -1)
                        {
                            return false;
                        }

                        this.m_preventsPushId[idx] = id;
                        this.m_preventsPushTime[idx] = 1500;
                    }

                    LogicVector2 pushForce = new LogicVector2(time * position.m_x / 32, time * position.m_y / 32);

                    this.m_pushTime = time - 16;
                    this.m_pushInitTime = time;

                    this.m_ignorePush = false;

                    this.m_pushBackStartPosition.m_x = this.m_position.m_x;
                    this.m_pushBackStartPosition.m_y = this.m_position.m_y;
                    this.m_pushBackEndPosition.m_x = this.m_position.m_x + pushForce.m_x;
                    this.m_pushBackEndPosition.m_y = this.m_position.m_y + pushForce.m_y;

                    return true;
                }
            }

            return false;
        }

        public void Boost(int speed, int time)
        {
            if (speed < 0)
            {
                this.m_slowSpeed = LogicMath.Min(LogicMath.Max(-100, speed), this.m_slowSpeed);
                this.m_slowTime = time;
            }
            else
            {
                int idx = this.m_boostSpeed[0] != 0 ? 1 : 0;

                this.m_boostSpeed[idx] = LogicMath.Max(speed, this.m_boostSpeed[idx]);
                this.m_boostTime[idx] = time;
            }
        }

        public bool IsBoosted()
        {
            return this.m_boostTime[0] > 0;
        }

        public bool IsSlowed()
        {
            return this.m_slowTime > 0;
        }

        public LogicGameObject GetPatrolPost()
        {
            return this.m_patrolPost;
        }

        public void SetPatrolPost(LogicGameObject gameObject)
        {
            this.m_patrolPost = gameObject;
        }

        public int GetFreezeTime()
        {
            return this.m_freezeTime;
        }

        public void SetFreezeTime(int value)
        {
            this.m_freezeTime = value;
        }

        public void CreatePatrolArea(LogicGameObject patrolPost, LogicLevel level, bool unk, int idx)
        {
            LogicArrayList<LogicVector2> wayPoints = new LogicArrayList<LogicVector2>(8);

            if (this.m_patrolPost == null)
            {
                this.m_patrolPost = patrolPost;
            }

            int startX = 0;
            int startY = 0;
            int endX = 0;
            int endY = 0;
            int midX = 0;
            int midY = 0;

            int width = 0;
            int height = 0;

            int radius = 0;

            if (patrolPost != null)
            {
                startX = patrolPost.GetX() - 128;
                startY = patrolPost.GetY() - 128;
                endX = patrolPost.GetX() + (patrolPost.GetWidthInTiles() << 9) + 128;
                endY = patrolPost.GetY() + (patrolPost.GetHeightInTiles() << 9) + 128;
                midX = patrolPost.GetMidX();
                midY = patrolPost.GetMidY();
                width = patrolPost.GetWidthInTiles() << 8;
                height = patrolPost.GetHeightInTiles() << 8;
                radius = 1536;
            }

            if (radius * radius >= (uint) (width * width + height * height))
            {
                LogicVector2 tmp1 = new LogicVector2();
                LogicVector2 tmp2 = new LogicVector2();
                LogicVector2 tmp3 = new LogicVector2();
                LogicVector2 tmp4 = new LogicVector2();

                tmp2.Set(midX, midY);

                int rnd = patrolPost.GetLevel().GetLogicTime().GetTick() + idx;

                midX = midX + 127 * rnd % 1024 - 512;
                midY = midY + 271 * rnd % 1024 - 512;

                for (int i = 0, j = 45; i < 4; i++, j += 90)
                {
                    tmp1.Set(midX + LogicMath.Cos(j, radius), midY + LogicMath.Sin(j, radius));
                    LogicHeroBaseComponent.FindPoint(patrolPost.GetLevel().GetTileMap(), tmp3, tmp2, tmp1, tmp4);
                    wayPoints.Add(new LogicVector2(tmp4.m_x, tmp4.m_y));
                }

                tmp1.Destruct();
                tmp2.Destruct();
                tmp3.Destruct();
                tmp4.Destruct();
            }
            else
            {
                wayPoints.Add(new LogicVector2(endX, endY));
                wayPoints.Add(new LogicVector2(startX, endY));
                wayPoints.Add(new LogicVector2(startX, startY));
                wayPoints.Add(new LogicVector2(endX, startY));
            }

            this.ClearPatrolArea();

            this.m_wayPoints = wayPoints;
            this.m_patrolAreaCounter = 0;

            if (this.m_wayPoints.Size() > 1)
            {
                int closestLength = 0x7FFFFFFF;

                for (int i = 1, size = this.m_wayPoints.Size(); i < size; i++)
                {
                    LogicVector2 wayPoint = this.m_wayPoints[i];

                    int length = (wayPoint.m_x - (this.m_position.m_x >> 16)) * (wayPoint.m_x - (this.m_position.m_x >> 16)) +
                                 (wayPoint.m_y - (this.m_position.m_y >> 16)) * (wayPoint.m_y - (this.m_position.m_y >> 16));

                    if (length < closestLength)
                    {
                        this.m_patrolAreaCounter = i;
                        closestLength = length;
                    }
                }
            }

            this.MoveTo(this.m_wayPoints[this.m_patrolAreaCounter].m_x, this.m_wayPoints[this.m_patrolAreaCounter].m_y, level.GetTileMap(), true);
        }
    }
}