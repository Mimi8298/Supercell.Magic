namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Calendar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicMovementComponent : LogicComponent
    {
        private int m_jumpTime; // 224
        private int m_wallCount; // 212
        private int m_patrolPathTime; // 216
        private int m_pathLifetime; // 200
        private int m_attackPositionRandom; // 220
        private int m_patrolPoint; // 228

        private long m_closerDistance; // 192

        private int m_targetStartX; // 184
        private int m_targetStartY; // 188
        private int m_alertPositionX; // 240
        private int m_alertPositionY; // 244

        private bool m_moving; // 254
        private bool m_notPassablePos; // 255
        private bool m_patrolEnabled; // 256
        private bool m_alerted; // 257
        private bool m_hasAlerted; // 258

        private bool m_underground; // 252
        private bool m_flying; // 253

        private readonly LogicVector2 m_attackPosition; // 204
        private readonly LogicMovementSystem m_movementSystem; // 12

        private LogicRandom m_random; // 248
        private LogicBuilding m_baseBuilding; // 232
        private LogicArrayList<LogicGameObject> m_nearestBuildings;

        public LogicMovementComponent(LogicGameObject gameObject, int speed, bool flying, bool underground) : base(gameObject)
        {
            this.m_attackPosition = new LogicVector2();
            this.m_movementSystem = new LogicMovementSystem();

            this.m_targetStartX = -1;
            this.m_targetStartY = -1;

            if (underground)
            {
                int minerSpeedRandomPercentage = LogicDataTables.GetGlobals().GetMinerSpeedRandomPercentage();

                if (minerSpeedRandomPercentage > 0)
                {
                    speed = speed * (gameObject.Rand(100) % minerSpeedRandomPercentage + 100) / 100;
                }
            }

            this.m_movementSystem.Init(speed, this, null);

            this.m_flying = flying;
            this.m_underground = underground;
        }

        public void SetSpeed(int speed)
        {
            this.m_movementSystem.SetSpeed(speed);
        }

        public void SetUnderground(bool value)
        {
            this.m_underground = value;
        }

        public override void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            base.RemoveGameObjectReferences(gameObject);

            if (this.m_baseBuilding == gameObject)
            {
                this.m_baseBuilding = null;
            }
        }

        public LogicBuilding GetBaseBuilding()
        {
            return this.m_baseBuilding;
        }

        public void SetBaseBuilding(LogicBuilding building)
        {
            this.m_baseBuilding = building;
        }

        public int GetClosestPatrolPoint(int x, int y, LogicArrayList<LogicVector2> patrolPath)
        {
            int closestDistance = -1;
            int closestIdx = 0;

            for (int i = 0; i < patrolPath.Size(); i++)
            {
                int distanceSquared = patrolPath[i].GetDistanceSquaredTo(x, y);

                if (distanceSquared < closestDistance || closestDistance < 0)
                {
                    closestIdx = i;
                    closestDistance = distanceSquared;
                }
            }

            return closestIdx;
        }

        public int EvaluateTargetCost(LogicGameObject target)
        {
            bool move = true;
            LogicMovementComponent movementComponent = target.GetMovementComponent();

            if (movementComponent == null)
            {
                move = target.GetGameObjectType() == LogicGameObjectType.CHARACTER && ((LogicCharacter) target).GetParent() != null;
            }

            if (!this.m_flying && !this.m_underground && move)
            {
                if (movementComponent != null && movementComponent.m_notPassablePos)
                {
                    return 0x7FFFFFFF;
                }
            }

            this.MoveTo(target);

            if (this.m_jumpTime <= 0 || move || this.m_parent.GetLevel().GetTileMap().IsPassablePathFinder(this.m_targetStartX >> 8, this.m_targetStartY >> 8))
            {
                this.m_wallCount = this.m_movementSystem.GetWallCount();

                if (this.m_wallCount > 0 && this.m_parent.GetHitpointComponent().GetTeam() == 1 && this.m_jumpTime <= 0)
                {
                    return 0x7FFFFFFF;
                }

                int jumpCost = LogicDataTables.GetGlobals().UseWallWeightsForJumpSpell() ? 10000 : 0;
                int wallCost = this.m_jumpTime <= jumpCost ? 3584 * this.m_wallCount : 0;

                return this.m_movementSystem.GetPathLength() + wallCost;
            }

            return 0x7FFFFFFF;
        }

        public bool EnableUnderground()
        {
            if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;

                if (character.GetCharacterData().IsUnderground())
                {
                    if (character.GetHitpointComponent().GetTeam() == 0)
                    {
                        this.m_parent.GetCombatComponent().SetUndergroundTime(3600000);
                        return true;
                    }
                }
            }

            return false;
        }

        public void EnableJump(int ms)
        {
            if (this.m_parent.GetGameObjectType() != LogicGameObjectType.CHARACTER || !((LogicCharacter) this.m_parent).GetCharacterData().IsUnderground() ||
                this.m_parent.GetHitpointComponent().GetTeam() != 0)
            {
                if (!this.m_flying)
                {
                    if (this.m_jumpTime <= 0)
                    {
                        this.m_parent.GetCombatComponent().ForceNewTarget();
                    }

                    this.m_jumpTime = ms;
                }
            }
        }

        public bool IsFlying()
        {
            return this.m_flying;
        }

        public void SetFlying(bool value)
        {
            this.m_flying = value;
        }

        public bool IsInNotPassablePosition()
        {
            return this.m_notPassablePos;
        }

        public bool GetPatrolEnabled()
        {
            return this.m_patrolEnabled;
        }

        public bool IsUnderground()
        {
            return this.m_underground;
        }

        public int GetJump()
        {
            return this.m_jumpTime;
        }

        public bool CanJumpWall()
        {
            return this.m_jumpTime > (LogicDataTables.GetGlobals().UseWallWeightsForJumpSpell() ? 10000 : 0);
        }

        public LogicMovementSystem GetMovementSystem()
        {
            return this.m_movementSystem;
        }

        public void AvoidPoison()
        {
            if (this.m_movementSystem.NotMoving())
            {
                LogicHitpointComponent hitpointComponent = this.m_parent.GetHitpointComponent();

                if (hitpointComponent != null && hitpointComponent.GetPoisonRemainingMS() > 0)
                {
                    this.InitRandom();

                    int posX = this.m_parent.GetMidX();
                    int posY = this.m_parent.GetMidY();

                    LogicTileMap tileMap = this.m_parent.GetLevel().GetTileMap();

                    for (int i = 0; i < 25; i++)
                    {
                        int posOffset = this.m_random.Rand(i << 8);
                        int posAngle = this.m_random.Rand(360);

                        int randomX = posX + ((posOffset * LogicMath.Sin(posAngle)) >> 10);
                        int randomY = posY + ((posOffset * LogicMath.Cos(posAngle)) >> 10);

                        if (tileMap.IsPassablePathFinder(randomX >> 8, randomY >> 8))
                        {
                            LogicArrayList<LogicGameObject> gameObjects = this.m_parent.GetGameObjectManager().GetGameObjects(LogicGameObjectType.SPELL);

                            bool noPoison = true;

                            for (int j = 0; j < gameObjects.Size(); j++)
                            {
                                LogicSpell spell = (LogicSpell) gameObjects[j];
                                LogicSpellData data = spell.GetSpellData();

                                int radius = data.GetRadius(spell.GetUpgradeLevel());
                                int distanceX = randomX - spell.GetMidX();
                                int distanceY = randomY - spell.GetMidY();

                                if (distanceX * distanceX + distanceY * distanceY < (uint) (radius * radius))
                                {
                                    if (data.GetPoisonDamage(0) > 0)
                                    {
                                        noPoison = false;
                                        break;
                                    }
                                }
                            }

                            if (noPoison)
                            {
                                this.MoveTo(randomX, randomY);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void Tick()
        {
            LogicCombatComponent combatComponent = this.m_parent.GetCombatComponent();

            if (!combatComponent.GetUnk596())
            {
                if (combatComponent.GetTarget(0) == null && !this.m_notPassablePos && !this.m_patrolEnabled && combatComponent.GetUndergroundTime() <= 0)
                {
                    if (LogicDataTables.GetGlobals().UsePoisonAvoidance())
                    {
                        LogicHitpointComponent hitpointComponent = this.m_parent.GetHitpointComponent();

                        if (hitpointComponent != null && hitpointComponent.GetPoisonRemainingMS() > 0 && hitpointComponent.GetTeam() == 1 && !this.m_parent.IsFrozen())
                        {
                            this.AvoidPoison();
                        }
                        else
                        {
                            this.m_moving = false;
                            this.m_movementSystem.ClearPath();
                        }
                    }
                    else
                    {
                        this.m_moving = false;
                        this.m_movementSystem.ClearPath();
                    }
                }
            }

            if (this.m_moving)
            {
                if (this.m_pathLifetime > 0)
                {
                    if (this.m_notPassablePos)
                    {
                        this.m_pathLifetime = LogicMath.Max(this.m_pathLifetime - 64, 1);
                    }
                    else
                    {
                        this.m_pathLifetime = LogicMath.Max(this.m_pathLifetime - 64, 0);

                        if (this.m_pathLifetime <= 0)
                        {
                            this.NewTargetFound();
                        }
                    }
                }
            }

            if (!this.m_movementSystem.NotMoving())
            {
                this.CheckTriggers();
            }

            if (this.m_patrolEnabled)
            {
                if (this.m_baseBuilding != null)
                {
                    if (this.m_baseBuilding.GetHeroBaseComponent() != null || this.m_baseBuilding.GetBunkerComponent() != null)
                    {
                        if (!this.m_notPassablePos)
                        {
                            int distanceSquaredToEnd = this.m_movementSystem.GetDistSqToEnd();

                            if (distanceSquaredToEnd <= 0xFFFF)
                            {
                                this.m_movementSystem.ClearPath();

                                if (this.m_alerted)
                                {
                                    int alertDistanceX = this.m_alertPositionX - this.m_parent.GetMidX();
                                    int alertDistanceY = this.m_alertPositionY - this.m_parent.GetMidY();

                                    this.m_movementSystem.SetDirection(LogicMath.GetAngle(alertDistanceX, alertDistanceY));

                                    if (this.m_hasAlerted)
                                    {
                                        this.m_hasAlerted = false;
                                        // Listener.
                                    }
                                }

                                this.m_patrolPathTime += 64;
                            }
                        }

                        if (this.m_patrolPathTime > 2000)
                        {
                            this.m_patrolPathTime = 0;

                            if (!this.m_parent.IsHero())
                            {
                                this.m_patrolPathTime = LogicMath.Clamp(30 * this.m_movementSystem.GetSpeed() - 100, 0, 800) +
                                                        (byte) this.m_parent.GetCombatComponent().Rand(this.m_parent.GetX());
                            }

                            LogicArrayList<LogicVector2> patrolPath = this.m_baseBuilding.GetHeroBaseComponent() != null
                                ? this.m_baseBuilding.GetHeroBaseComponent().GetPatrolPath()
                                : this.m_baseBuilding.GetBunkerComponent() != null
                                    ? this.m_baseBuilding.GetBunkerComponent().GetPatrolPath()
                                    : null;

                            int pointIdx;

                            if (this.m_alerted)
                            {
                                pointIdx = this.GetClosestPatrolPoint(this.m_alertPositionX, this.m_alertPositionY, patrolPath);
                            }
                            else if (this.m_parent.IsHero())
                            {
                                pointIdx = this.GetClosestPatrolPoint(this.m_parent.GetPosition().m_x, this.m_parent.GetPosition().m_y, patrolPath) + 1;
                            }
                            else
                            {
                                if (this.m_random == null)
                                {
                                    this.m_random = new LogicRandom();
                                    this.m_random.SetIteratedRandomSeed(this.m_parent.GetGlobalID());
                                }

                                pointIdx = this.m_random.Rand(patrolPath.Size());
                            }

                            if (pointIdx < 0)
                            {
                                pointIdx = patrolPath.Size() - 1;
                            }

                            if (pointIdx >= patrolPath.Size())
                            {
                                pointIdx = 0;
                            }

                            for (int i = 0; i < patrolPath.Size(); i++)
                            {
                                int point = (pointIdx + i) % patrolPath.Size();

                                if (point != this.m_patrolPoint)
                                {
                                    int distanceSquared = patrolPath[point].GetDistanceSquared(this.m_parent.GetPosition());

                                    if (distanceSquared > 0x10000)
                                    {
                                        this.m_patrolPoint = point;
                                        this.MoveToPoint(patrolPath[point]);

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void SubTick()
        {
            LogicCombatComponent combatComponent = this.m_parent.GetCombatComponent();

            if (combatComponent.GetAttackFinished())
            {
                if (this.m_moving)
                {
                    LogicVector2 position = this.m_movementSystem.GetPosition();

                    if (this.m_flying || this.m_parent.GetLevel().GetTileMap().IsPassablePathFinder(position.m_x >> 8, position.m_y >> 8))
                    {
                        LogicGameObject target = combatComponent.GetTarget(0);

                        if (target != null)
                        {
                            this.m_movementSystem.SetDirection(LogicMath.GetAngle(target.GetMidX() - position.m_x, target.GetMidY() - position.m_y));
                            this.m_movementSystem.ClearPath();

                            if (target.GetMovementComponent() != null ||
                                target.GetGameObjectType() == LogicGameObjectType.CHARACTER && ((LogicCharacter) target).GetParent() != null)
                            {
                                this.m_pathLifetime = 100;
                            }
                            else
                            {
                                this.m_moving = false;
                            }
                        }
                        else
                        {
                            this.m_movementSystem.ClearPath();
                            this.m_moving = false;
                        }
                    }
                }
            }


            if (this.m_parent.IsAlive())
            {
                this.m_movementSystem.SubTick();

                if (this.m_jumpTime > 0)
                {
                    LogicVector2 position = this.m_movementSystem.GetPosition();
                    bool passablePathFinder = this.m_parent.GetLevel().GetTileMap().IsPassablePathFinder(position.m_x >> 8, position.m_y >> 8);

                    this.m_jumpTime = LogicMath.Max(this.m_jumpTime - 16, 0);
                    this.m_notPassablePos = passablePathFinder ^ true;

                    if (this.m_jumpTime == 0)
                    {
                        if (passablePathFinder)
                        {
                            this.m_parent.GetCombatComponent().ForceNewTarget();
                        }
                        else
                        {
                            this.m_jumpTime = 1;
                        }
                    }
                }
            }
        }

        public void CheckTriggers()
        {
            if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;

                if (!character.GetCharacterData().GetTriggersTraps())
                {
                    return;
                }
            }

            LogicVector2 position = this.m_movementSystem.GetPosition();

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    LogicTile tile = this.m_parent.GetLevel().GetTileMap().GetTile((position.m_x >> 9) + i, (position.m_y >> 9) + j);

                    if (tile != null)
                    {
                        for (int k = 0; k < tile.GetGameObjectCount(); k++)
                        {
                            LogicGameObject gameObject = tile.GetGameObject(k);

                            if (!this.m_movementSystem.IsPushed() || !this.m_movementSystem.IgnorePush())
                            {
                                LogicTriggerComponent triggerComponent = gameObject.GetTriggerComponent();

                                if (triggerComponent != null && !triggerComponent.IsTriggeredByRadius())
                                {
                                    triggerComponent.ObjectClose(this.m_parent);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void NewTargetFound()
        {
            LogicGameObject target = this.m_parent.GetCombatComponent().GetTarget(0);

            if (target != null)
            {
                if (LogicDataTables.GetGlobals().RepathDuringFly() || !this.m_movementSystem.IsPushed() || !this.m_movementSystem.IgnorePush())
                {
                    if (this.m_parent.GetCombatComponent().IsInRange(target))
                    {
                        this.m_moving = true;

                        if (this.m_patrolEnabled)
                        {
                            // Listener.
                        }

                        this.m_patrolEnabled = false;
                        this.m_alerted = false;
                        this.m_targetStartX = this.m_parent.GetMidX();
                        this.m_targetStartY = this.m_parent.GetMidY();

                        this.m_movementSystem.AddPoint(this.m_targetStartX, this.m_targetStartY);
                        this.m_movementSystem.SetDirection(LogicMath.GetAngle(target.GetMidX() - this.m_parent.GetMidX(), target.GetMidY() - this.m_parent.GetMidY()));

                        if (target.GetMovementComponent() != null)
                        {
                            this.m_pathLifetime = 1;
                        }
                        else if (target.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            this.m_pathLifetime = ((LogicCharacter) target).GetParent() != null ? 1 : 0;
                        }
                    }
                    else
                    {
                        this.MoveTo(target);

                        LogicGameObject wall = this.m_movementSystem.GetWall();

                        if (wall != null)
                        {
                            if (this.m_jumpTime <= 0)
                            {
                                if (!wall.IsWall() || ((LogicBuilding) wall).GetHitWallDelay() <= 0)
                                {
                                    this.m_parent.GetCombatComponent().ObstacleToDestroy(wall);
                                    this.m_pathLifetime = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void NoTargetFound()
        {
            LogicHitpointComponent hitpointComponent = this.m_parent.GetHitpointComponent();

            if (hitpointComponent == null || hitpointComponent.GetTeam() != 0)
            {
                LogicGameObject patrolPost = this.m_movementSystem.GetPatrolPost();

                if (patrolPost != null)
                {
                    LogicCalendarBuildingDestroyedSpawnUnit buildingDestroyedSpawnUnit = this.m_parent.GetLevel().GetCalendar().GetBuildingDestroyedSpawnUnit();
                    LogicBuildingData buildingData = buildingDestroyedSpawnUnit?.GetBuildingData();

                    if (patrolPost.GetDefenceUnitProduction() != null || patrolPost.GetData() == buildingData)
                    {
                        this.m_movementSystem.UpdatePatrolArea(this.m_parent.GetLevel());
                        this.m_patrolEnabled = true;
                    }
                }

                if (LogicDataTables.GetGlobals().AllianceTroopsPatrol() || this.m_parent.IsHero())
                {
                    bool prevPatrolEnabled = this.m_patrolEnabled;
                    this.m_patrolEnabled = true;

                    if (this.m_baseBuilding != null)
                    {
                        LogicHeroBaseComponent heroBaseComponent = this.m_baseBuilding.GetHeroBaseComponent();

                        if (heroBaseComponent != null)
                        {
                            LogicArrayList<LogicGameObject> gameObjects = this.m_parent.GetGameObjectManager().GetGameObjects(LogicGameObjectType.CHARACTER);

                            int alertRadius = heroBaseComponent.GetHeroData().GetAlertRadius() >> 9;
                            int alertRadiusSquared = alertRadius * alertRadius;

                            uint minDistance = 0xFFFFFFFF;

                            LogicGameObject closestGameObject = null;

                            for (int i = 0; i < gameObjects.Size(); i++)
                            {
                                LogicCharacter character = (LogicCharacter) gameObjects[i];
                                LogicHitpointComponent characterHitpointComponent = character.GetHitpointComponent();

                                if (characterHitpointComponent != null && characterHitpointComponent.GetTeam() == 1 ||
                                    !character.GetAttackerItemData().GetTrackAirTargets(false) && character.IsFlying() || !character.IsAlive())
                                {
                                    continue;
                                }

                                int distanceX = (character.GetMidX() - this.m_baseBuilding.GetMidX()) >> 9;
                                int distanceY = (character.GetMidY() - this.m_baseBuilding.GetMidY()) >> 9;

                                uint distanceSquared = (uint) (distanceX * distanceX + distanceY * distanceY);

                                if (distanceSquared < alertRadiusSquared && minDistance > distanceSquared)
                                {
                                    minDistance = distanceSquared;
                                    closestGameObject = character;
                                }
                            }

                            bool prevAlerted = this.m_alerted;
                            this.m_alerted = closestGameObject != null;

                            if (closestGameObject != null && prevPatrolEnabled && !prevAlerted)
                            {
                                this.m_hasAlerted = true;
                            }

                            if (closestGameObject != null)
                            {
                                this.m_alertPositionX = closestGameObject.GetMidX();
                                this.m_alertPositionY = closestGameObject.GetMidY();
                            }

                            if (!prevPatrolEnabled)
                            {
                                this.m_movementSystem.ClearPath();
                            }
                        }
                    }
                }
            }
        }

        public void MoveTo(LogicGameObject gameObject)
        {
            this.EnableUnderground();

            this.m_moving = true;

            LogicMovementComponent movementComponent = gameObject.GetMovementComponent();

            if (movementComponent != null || gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                if (movementComponent == null)
                {
                    LogicCharacter character = (LogicCharacter) gameObject;

                    if (character.GetParent() == null)
                    {
                        goto DEFAULT_MOVE;
                    }
                }

                if (this.m_patrolEnabled)
                {
                    this.m_parent.GetListener().SpottedEnemy();
                }

                this.m_patrolEnabled = false;
                this.m_alerted = false;

                if (this.FindGoodAttackPosAround(gameObject, this.GetAttackDist(), false))
                {
                    if (this.m_underground || this.m_flying)
                    {
                        this.m_movementSystem.AddPoint(this.m_targetStartX, this.m_targetStartY);
                    }
                    else
                    {
                        this.m_movementSystem.MoveTo(this.m_targetStartX, this.m_targetStartY, this.m_parent.GetLevel().GetTileMap(), true);
                    }

                    this.m_pathLifetime = LogicMath.Max(250 * this.m_movementSystem.GetPathLength() / 512, 1);
                }
                else
                {
                    this.m_pathLifetime = 1000;
                    this.m_movementSystem.ClearPath();
                }

                return;
            }

            DEFAULT_MOVE:

            if (this.m_patrolEnabled)
            {
                this.m_parent.GetListener().SpottedEnemy();
            }

            this.m_alerted = false;
            this.m_patrolEnabled = false;

            if (this.FindGoodAttackPosAround(gameObject, this.GetAttackDist(), false))
            {
                if (this.m_underground || this.m_flying)
                {
                    this.m_movementSystem.AddPoint(this.m_targetStartX, this.m_targetStartY);
                }
                else
                {
                    this.m_movementSystem.MoveTo(this.m_targetStartX, this.m_targetStartY, this.m_parent.GetLevel().GetTileMap(), true);
                }
            }

            this.m_pathLifetime = 0;
        }

        public void MoveTo(int x, int y)
        {
            this.EnableUnderground();
            this.m_moving = true;

            if (this.m_patrolEnabled)
            {
                this.m_parent.GetListener().SpottedEnemy();
            }

            this.m_patrolEnabled = false;
            this.m_alerted = false;

            if (this.m_underground || this.m_flying)
            {
                this.m_movementSystem.AddPoint(this.m_targetStartX, this.m_targetStartY);
            }
            else
            {
                LogicTileMap tileMap = this.m_parent.GetLevel().GetTileMap();

                if (tileMap.IsPassablePathFinder(x >> 8, y >> 8))
                {
                    this.m_movementSystem.MoveTo(x, y, tileMap, true);
                }
                else
                {
                    LogicVector2 nearestPassablePosition = new LogicVector2();

                    if (tileMap.GetNearestPassablePosition(x, y, nearestPassablePosition, 512) ||
                        tileMap.GetNearestPassablePosition(x, y, nearestPassablePosition, 1536))
                    {
                        this.m_movementSystem.MoveTo(nearestPassablePosition.m_x, nearestPassablePosition.m_y, tileMap, true);
                    }
                    else
                    {
                        this.m_movementSystem.ClearPath();
                    }
                }
            }

            this.m_pathLifetime = 0;
        }

        public bool FindGoodAttackPosAround(LogicGameObject gameObject, int attackDistance, bool doNotOverride)
        {
            LogicCombatComponent combatComponent = this.m_parent.GetCombatComponent();

            if (combatComponent.IsInRange(gameObject))
            {
                this.m_targetStartX = this.m_parent.GetMidX();
                this.m_targetStartY = this.m_parent.GetMidY();

                return true;
            }

            if (this.m_flying)
            {
                int midX = gameObject.GetMidX();
                int midY = gameObject.GetMidY();

                if (gameObject.GetMovementComponent() != null)
                {
                    this.m_attackPosition.m_x = this.m_parent.GetMidX() - midX;
                    this.m_attackPosition.m_y = this.m_parent.GetMidY() - midY;

                    this.m_attackPosition.Normalize(attackDistance);
                }
                else
                {
                    int randomAngle = combatComponent.Rand(29) % 360;

                    this.m_attackPosition.m_x = LogicMath.Sin(randomAngle, attackDistance);
                    this.m_attackPosition.m_y = LogicMath.Cos(randomAngle, attackDistance);
                }

                this.m_targetStartX = midX + this.m_attackPosition.m_x;
                this.m_targetStartY = midY + this.m_attackPosition.m_y;

                return true;
            }

            this.m_attackPositionRandom = 7;

            if (gameObject.IsWall())
            {
                this.m_attackPositionRandom = 0;
            }

            if (this.m_parent.IsHero() && !LogicDataTables.GetGlobals().HeroUsesAttackPosRandom())
            {
                this.m_attackPositionRandom = 0;
            }

            if (this.m_parent.GetCombatComponent().GetTotalTargets() == 0 && !LogicDataTables.GetGlobals().UseAttackPosRandomOn1stTarget())
            {
                this.m_attackPositionRandom = 0;
            }

            bool attackOverWalls = true;

            if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;

                if (character.IsWallBreaker())
                {
                    this.m_attackPositionRandom = 0;
                }

                attackOverWalls = character.GetCharacterData().GetAttackOverWalls();
            }

            this.m_closerDistance = 0x7FFFFFFFFFFFFFFF;
            this.m_targetStartX = -1;
            this.m_targetStartY = -1;

            int width = (gameObject.GetWidthInTiles() << 8) - (gameObject.PassableSubtilesAtEdge() << 8);
            int height = (gameObject.GetHeightInTiles() << 8) - (gameObject.PassableSubtilesAtEdge() << 8);

            bool success1 = this.ProcessLine(attackDistance, 0, height + attackDistance, width, height + attackDistance, gameObject, doNotOverride, false, attackOverWalls);
            bool success2 = this.ProcessLine(attackDistance, width, height, gameObject, doNotOverride, true, attackOverWalls);
            bool success3 = this.ProcessLine(attackDistance, width + attackDistance, height, width + attackDistance, 0, gameObject, doNotOverride, false, attackOverWalls);

            if (success1 && success2 && success3)
            {
                int gameObjectX = gameObject.GetMidX();
                int gameObjectY = gameObject.GetMidY();

                LogicTileMap tileMap = gameObject.GetLevel().GetTileMap();
                LogicVector2 wallPosition = new LogicVector2();

                if (tileMap.GetWallInPassableLine(gameObjectX, gameObjectY, this.m_parent.GetMidX(), this.m_parent.GetMidY(), wallPosition))
                {
                    int wallDistanceX = wallPosition.m_x - gameObjectX;
                    int wallDistanceY = wallPosition.m_y - gameObjectY;
                    int gameObjectWidth = gameObject.GetWidthInTiles() << 8;
                    int gameObjectHeight = gameObject.GetHeightInTiles() << 8;

                    int distanceX;
                    int distanceY;

                    if (wallDistanceX >= gameObjectWidth)
                    {
                        distanceX = wallDistanceX - gameObjectWidth;
                    }
                    else
                    {
                        distanceX = wallDistanceX - width;

                        if (distanceX < 0)
                        {
                            distanceX = 0;
                        }
                    }

                    if (wallDistanceY >= gameObjectHeight)
                    {
                        distanceY = wallDistanceY - gameObjectHeight;
                    }
                    else
                    {
                        distanceY = wallDistanceY - height;

                        if (distanceY < 0)
                        {
                            distanceY = 0;
                        }
                    }

                    if (distanceX * distanceX + distanceY * distanceY > (uint) attackDistance * attackDistance)
                    {
                        LogicVector2 tmp = new LogicVector2();

                        tmp.Set(distanceX, distanceY);
                        tmp.Normalize(attackDistance);

                        wallPosition.Set(tmp.m_x + gameObjectX, tmp.m_y + gameObjectY);
                    }

                    this.m_closerDistance = (wallPosition.m_x - this.m_parent.GetMidX()) * (wallPosition.m_x - this.m_parent.GetMidX()) +
                                            (wallPosition.m_y - this.m_parent.GetMidY()) * (wallPosition.m_y - this.m_parent.GetMidY());

                    if (tileMap.GetNearestPassablePosition(wallPosition.m_x, wallPosition.m_y, wallPosition, LogicDataTables.GetGlobals().UseNewPathFinder() ? 512 : 256))
                    {
                        this.m_targetStartX = wallPosition.m_x;
                        this.m_targetStartY = wallPosition.m_y;
                    }
                    else
                    {
                        this.m_targetStartX = -1;
                        this.m_targetStartY = -1;
                    }
                }
            }

            return this.m_targetStartX != -1;
        }

        public bool ProcessLine(int radius, int startX, int startY, int destX, int destY, LogicGameObject gameObject, bool doNotOverride, bool ignoreNearestBuildings,
                                bool attackOverWalls)
        {
            bool closest = false;

            if (doNotOverride && this.m_targetStartX != -1)
            {
                return false;
            }

            LogicVector2 position = this.m_movementSystem.GetPosition();
            LogicCombatComponent combatComponent = this.m_parent.GetCombatComponent();

            if (combatComponent.GetTargetGroupPosition() != null)
            {
                position = combatComponent.GetTargetGroupPosition();
            }

            int midX = gameObject.GetMidX();
            int midY = gameObject.GetMidY();

            int distance = LogicMath.Sqrt((destX - startX) * (destX - startX) + (destY - startY) * (destY - startY));
            int distanceTile = LogicMath.Max(1, (int) (distance + ((uint) ((distance + 256) >> 31) >> 23) + 256) >> 9);

            LogicTileMap tileMap = this.m_parent.GetLevel().GetTileMap();

            int distanceX = destX - startX;
            int distanceY = destY - startY;

            for (int i = 0; i < distanceTile; i++)
            {
                int offsetX = startX + distanceX / distanceTile / 2;
                int offsetY = startY + distanceY / distanceTile / 2;

                bool closest1 = this.CheckIfCloser(gameObject, radius, position, midX, midY, offsetX, offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);
                bool closest2 = this.CheckIfCloser(gameObject, radius, position, midX, midY, -offsetX, offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);
                bool closest3 = this.CheckIfCloser(gameObject, radius, position, midX, midY, offsetX, -offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);
                bool closest4 = this.CheckIfCloser(gameObject, radius, position, midX, midY, -offsetX, -offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);

                closest = closest1 && closest2 && closest3 && closest4;

                if (doNotOverride)
                {
                    if (this.m_targetStartX != -1)
                    {
                        break;
                    }
                }

                distanceX += 2 * destX - 2 * startX;
                distanceY += 2 * destY - 2 * startY;
            }

            return closest;
        }

        public bool ProcessLine(int radius, int startX, int startY, LogicGameObject gameObject, bool doNotOverride, bool ignoreNearestBuildings, bool attackOverWalls)
        {
            if (doNotOverride && this.m_targetStartX != -1)
            {
                return false;
            }

            bool closest = true;

            LogicVector2 position = this.m_movementSystem.GetPosition();
            LogicCombatComponent combatComponent = this.m_parent.GetCombatComponent();

            if (combatComponent.GetTargetGroupPosition() != null)
            {
                position = combatComponent.GetTargetGroupPosition();
            }

            int midX = gameObject.GetMidX();
            int midY = gameObject.GetMidY();
            int distance = LogicMath.Max(1, (int) (((uint) ((157 * radius / 100 + 256) >> 31) >> 23) + 157 * radius / 100 + 256) >> 9);

            LogicTileMap tileMap = this.m_parent.GetLevel().GetTileMap();

            int angle = 900 / distance;

            for (int i = 0, j = angle / 2; i < distance; i++, j += angle)
            {
                int offsetX = startX + LogicMath.Cos(j / 10, radius);
                int offsetY = startY + LogicMath.Sin(j / 10, radius);

                bool closest1 = this.CheckIfCloser(gameObject, radius, position, midX, midY, offsetX, offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);
                bool closest2 = this.CheckIfCloser(gameObject, radius, position, midX, midY, -offsetX, offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);
                bool closest3 = this.CheckIfCloser(gameObject, radius, position, midX, midY, offsetX, -offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);
                bool closest4 = this.CheckIfCloser(gameObject, radius, position, midX, midY, -offsetX, -offsetY, tileMap, ignoreNearestBuildings, attackOverWalls);

                closest = closest1 && closest2 && closest3 && closest4;

                if (doNotOverride)
                {
                    if (this.m_targetStartX != -1)
                    {
                        break;
                    }
                }
            }

            return closest;
        }

        public bool CheckIfCloser(LogicGameObject gameObject, int radius, LogicVector2 position, int midX, int midY, int offsetX, int offsetY, LogicTileMap tileMap,
                                  bool ignoreNearestBuildings,
                                  bool attackOverWalls)
        {
            int x = midX + offsetX;
            int y = midY + offsetY;
            int subTileX = x >> 8;
            int subTileY = y >> 8;
            int mapWidth = tileMap.GetSizeX() * 2;
            int mapHeight = tileMap.GetSizeY() * 2;

            if (subTileX >= mapWidth || subTileY >= mapHeight || (subTileX | subTileY) < 0)
            {
                return true;
            }

            long v71 = 0;

            if (!this.m_flying && !this.m_underground)
            {
                int pathFinderCost = tileMap.GetPathFinderCost(subTileX, subTileY);

                if (pathFinderCost == 0x7FFFFFFF)
                {
                    return true;
                }

                if (pathFinderCost > 0)
                {
                    v71 = mapWidth;
                }

                if (LogicDataTables.GetGlobals().TargetSelectionConsidersWallsOnPath())
                {
                    if (tileMap.GetWallInPassableLine(position.m_x, position.m_y, x, y, new LogicVector2()))
                    {
                        v71 += 1000;
                    }
                }

                if (!attackOverWalls)
                {
                    LogicVector2 wallPosition = new LogicVector2();

                    if (tileMap.GetWallInPassableLine(midX, midY, x, y, wallPosition))
                    {
                        x = wallPosition.m_x;
                        y = wallPosition.m_y;
                        subTileX = x >> 8;
                        subTileY = y >> 8;

                        int cost = tileMap.GetPathFinderCost(subTileX, subTileY);

                        if (cost == 0x7FFFFFFF)
                        {
                            return true;
                        }

                        if (cost > 0)
                        {
                            v71 = mapWidth;
                        }
                    }

                    wallPosition.Destruct();
                }

                if (!tileMap.IsPassablePathFinder(subTileX, subTileY))
                {
                    LogicVector2 passablePosition = new LogicVector2();

                    if (!tileMap.GetPassablePositionInLine(x, y, midX, midY, 512, passablePosition))
                    {
                        return true;
                    }

                    x = passablePosition.m_x;
                    y = passablePosition.m_y;
                    subTileX = x >> 8;
                    subTileY = y >> 8;

                    int cost = tileMap.GetPathFinderCost(subTileX, subTileY);

                    if (cost == 0x7FFFFFFF)
                    {
                        return true;
                    }

                    if (cost > 0)
                    {
                        v71 = mapWidth;
                    }
                }

                int blockedAttackPositionPenalty = LogicDataTables.GetGlobals().GetBlockedAttackPositionPenalty();

                if (blockedAttackPositionPenalty > 0)
                {
                    LogicTile currentTile = tileMap.GetTile(this.m_parent.GetTileX(), this.m_parent.GetTileY());
                    LogicTile endTile = tileMap.GetTile(x >> 9, y >> 9);

                    if (currentTile != null && endTile != null)
                    {
                        if (currentTile.GetRoomIdx() != endTile.GetRoomIdx())
                        {
                            v71 += 2 * blockedAttackPositionPenalty;
                        }
                    }
                }
            }

            LogicCombatComponent combatComponent = this.m_parent.GetCombatComponent();
            LogicGameObject target = combatComponent.GetTarget(0);

            if (combatComponent.GetAttackMultipleBuildings() && target != null && target.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                int nearestBuildings = this.GetNearestBuildings(x, y, combatComponent.GetAttackRange(0, false), tileMap);

                if (LogicDataTables.GetGlobals().ValkyriePrefers4Buildings())
                {
                    v71 += LogicMath.Clamp(4 - nearestBuildings, 0, 3) * 15;
                }
                else
                {
                    if (ignoreNearestBuildings)
                    {
                        nearestBuildings = 1;
                    }

                    v71 += LogicMath.Clamp(2 - nearestBuildings, 0, 1) * 20;
                }
            }

            int posX = position.m_x - x;
            int posY = position.m_y - y;

            if (this.m_attackPositionRandom > 0)
            {
                LogicMovementComponent movementComponent = gameObject.GetMovementComponent();

                int distX = midX - x;
                int distY = midY - y;
                int seed = movementComponent == null ? x + y : distX + distY;

                if (LogicDataTables.GetGlobals().TighterAttackPosition())
                {
                    int totalRadius = radius + (gameObject.GetWidthInTiles() << 8);

                    int randomDistance1 = LogicMath.Abs(combatComponent.Rand(this.m_parent.GetGlobalID() + seed) % totalRadius);
                    int randomDistance2 = combatComponent.Rand(this.m_parent.GetGlobalID() + seed + 1) % 128;

                    posX = position.m_x + distY * randomDistance2 / totalRadius - (x + distX * randomDistance1 / radius);
                    posY = position.m_y - (distX * randomDistance2 / radius + y + distY * randomDistance1 / radius);
                }
                else
                {
                    posX += (this.m_attackPositionRandom & combatComponent.Rand(seed + 1)) << 8;
                    posY += (this.m_attackPositionRandom & combatComponent.Rand(seed)) << 8;
                }
            }

            long distance = (long) posX * posX + posY * posY + (v71 << 8) * (v71 << 8);

            if (distance < this.m_closerDistance)
            {
                this.m_closerDistance = distance;
                this.m_targetStartX = x;
                this.m_targetStartY = y;
            }

            return false;
        }

        public int GetNearestBuildings(int x, int y, int radius, LogicTileMap tileMap)
        {
            if (this.m_nearestBuildings == null)
            {
                this.m_nearestBuildings = new LogicArrayList<LogicGameObject>(10);
            }

            int startX = (x - radius) >> 9;
            int endX = (x + radius) >> 9;

            while (startX <= endX)
            {
                int startY = (y - radius) >> 9;
                int endY = (y + radius) >> 9;

                while (startY <= endY)
                {
                    LogicTile tile = tileMap.GetTile(startX, startY);

                    if (tile != null)
                    {
                        for (int i = 0; i < tile.GetGameObjectCount(); i++)
                        {
                            LogicGameObject gameObject = tile.GetGameObject(i);

                            if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING && gameObject.IsAlive() && !gameObject.IsWall())
                            {
                                int idx = this.m_nearestBuildings.IndexOf(gameObject);

                                if (idx == -1 && gameObject.PassableSubtilesAtEdge() <= 1)
                                {
                                    this.m_nearestBuildings.Add(gameObject);
                                }
                            }
                        }
                    }

                    ++startY;
                }

                ++startX;
            }

            int count = this.m_nearestBuildings.Size();
            this.m_nearestBuildings.Clear();
            return count;
        }

        public int GetAttackDist()
        {
            return LogicMath.Max(32, this.m_parent.GetCombatComponent().GetAttackRange(0, false) - 32);
        }

        public void MoveToPoint(LogicVector2 position)
        {
            this.EnableUnderground();

            if (this.m_underground || this.m_flying)
            {
                this.m_movementSystem.AddPoint(position.m_x, position.m_y);
            }
            else
            {
                this.m_movementSystem.MoveTo(position.m_x, position.m_y, this.m_parent.GetLevel().GetTileMap(), true);
            }
        }

        public void InitRandom()
        {
            if (this.m_random == null)
            {
                this.m_random = new LogicRandom();
                this.m_random.SetIteratedRandomSeed(this.m_parent.GetLevel().GetLogicTime().GetTick() + this.m_parent.GetGlobalID());
            }
        }

        public void SetPatrolFreeze()
        {
            if (this.m_patrolEnabled && this.m_movementSystem.GetFreezeTime() > 300)
            {
                this.InitRandom();
                this.m_movementSystem.SetFreezeTime(this.m_random.Rand(100) + 200);

                Debugger.Print("Freeze");
            }
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.MOVEMENT;
        }
    }
}