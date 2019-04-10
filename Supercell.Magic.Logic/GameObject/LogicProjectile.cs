namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicProjectile : LogicGameObject
    {
        public const int MAX_BOUNCES = 4;

        private int m_minAttackRange;
        private int m_maxAttackRange;
        private int m_bounceCount;
        private int m_damage; // 176
        private int m_damageTime;
        private int m_damageRadius;
        private int m_groupsId;
        private int m_randomHitRange;
        private int m_shockwaveAngle;
        private int m_shockwavePushStrength;
        private int m_shockwaveArcLength;
        private int m_shockwaveExpandRadius;
        private int m_myTeam;
        private int m_penetratingRadius;
        private int m_preferredTargetDamageMod;
        private int m_pushBack; // 212
        private int m_speedMod; // 216
        private int m_statusEffectTime; // 220
        private int m_travelTime; // 268
        private int m_areaShieldSpeed;
        private int m_areaShieldDelay;

        private bool m_gravity; // 210
        private bool m_bounceProjectile; // 322
        private bool m_targetGroups; // 322
        private bool m_flyingTarget; // 209
        private bool m_penetrating; // 264
        private bool m_dummy; // 323
        private bool m_targetReached; // 208

        private LogicGameObject m_groups;
        private LogicGameObject m_target; // 132
        private LogicData m_preferredTarget;
        private LogicEffectData m_hitEffect;
        private LogicEffectData m_hitEffect2;
        private readonly LogicGameObject[] m_bounceTargets;
        private readonly LogicVector2[] m_bouncePositions;

        private readonly LogicVector2 m_targetPosition; // 136
        private readonly LogicVector2 m_unk144;
        private readonly LogicVector2 m_unk152;
        private readonly LogicVector2 m_unk160;
        private readonly LogicVector2 m_unk168;
        private readonly LogicVector2 m_unk248;
        private readonly LogicVector2 m_unk276;

        public LogicProjectile(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            this.m_myTeam = -1;
            this.m_targetPosition = new LogicVector2();
            this.m_unk144 = new LogicVector2();
            this.m_unk152 = new LogicVector2();
            this.m_unk160 = new LogicVector2();
            this.m_unk168 = new LogicVector2();
            this.m_unk248 = new LogicVector2();
            this.m_unk276 = new LogicVector2();

            this.m_bounceTargets = new LogicGameObject[LogicProjectile.MAX_BOUNCES];
            this.m_bouncePositions = new LogicVector2[LogicProjectile.MAX_BOUNCES];
        }

        public LogicProjectileData GetProjectileData()
        {
            return (LogicProjectileData) this.m_data;
        }

        public override LogicGameObjectType GetGameObjectType()
        {
            return LogicGameObjectType.PROJECTILE;
        }

        public override void Destruct()
        {
            base.Destruct();

            for (int i = 0; i < LogicProjectile.MAX_BOUNCES; i++)
            {
                this.m_bouncePositions[i]?.Destruct();
                this.m_bouncePositions[i] = null;
            }

            this.m_targetPosition.Destruct();
            this.m_unk144.Destruct();
            this.m_unk152.Destruct();
            this.m_unk160.Destruct();
            this.m_unk168.Destruct();
            this.m_unk248.Destruct();
            this.m_unk276.Destruct();
        }

        public override void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            if (this.m_target == gameObject)
            {
                this.m_target = null;
            }

            if (this.m_groups == gameObject)
            {
                this.m_groups = null;

                if (this.m_targetGroups)
                {
                    this.m_target = null;
                    this.m_targetGroups = false;
                }
            }
        }

        public override int GetWidthInTiles()
        {
            return 0;
        }

        public override int GetHeightInTiles()
        {
            return 0;
        }

        public override bool ShouldDestruct()
        {
            return this.m_targetReached && this.m_damageTime == -0x80000000;
        }

        public override bool IsUnbuildable()
        {
            return true;
        }

        public bool IsDummyProjectile()
        {
            return this.m_dummy;
        }

        public void SetDummyProjectile(bool dummy)
        {
            this.m_dummy = dummy;
        }

        public LogicEffectData GetHitEffect()
        {
            return this.m_hitEffect;
        }

        public void SetHitEffect(LogicEffectData hitEffect, LogicEffectData hitEffect2)
        {
            this.m_hitEffect = hitEffect;
            this.m_hitEffect2 = hitEffect2;
        }

        public void SetDamage(int damage)
        {
            this.m_damage = damage;
        }

        public void SetPushBack(int force, bool enabled)
        {
            this.m_pushBack = force;
            this.m_gravity = enabled;
        }

        public void SetSpeedMod(int speed)
        {
            this.m_speedMod = speed;
        }

        public void SetStatusEffectTime(int time)
        {
            this.m_statusEffectTime = time;
        }

        public void SetBounceCount(int value)
        {
            this.m_bounceCount = value;

            if (value > LogicProjectile.MAX_BOUNCES)
            {
                Debugger.Warning("LogicProjectile::setBounceCount() called with too high value, clamping to MAX_BOUNCES");
                this.m_bounceCount = LogicProjectile.MAX_BOUNCES;
            }
        }

        public void SetInitialPosition(LogicGameObject groups, int x, int y)
        {
            this.m_groups = groups;
            this.m_groupsId = groups != null ? groups.GetGlobalID() : 0;
            this.m_unk144.m_x = this.m_targetPosition.m_x - 8 * x;
            this.m_unk144.m_y = this.m_targetPosition.m_y - 8 * y;

            this.m_unk144.Normalize((this.GetProjectileData().GetStartOffset() << 9) / 100);
            this.SetInitialPosition(this.m_unk144.m_x + x, this.m_unk144.m_y + y);

            this.m_unk160.m_x = 0;
            this.m_unk160.m_y = 0;
            this.m_unk248.m_x = this.GetX();
            this.m_unk248.m_y = this.GetY();
            this.m_unk276.m_x = this.GetX() * 8;
            this.m_unk276.m_y = this.GetY() * 8;
            this.m_unk152.m_x = this.m_targetPosition.m_x - this.m_unk276.m_x;
            this.m_unk152.m_y = this.m_targetPosition.m_y - this.m_unk276.m_y;

            this.m_targetGroups = false;

            if (this.m_groups != null && this.m_groups.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicCombatComponent combatComponent = this.m_groups.GetCombatComponent();

                if (combatComponent != null && combatComponent.GetAttackerItemData().GetTargetGroups())
                {
                    this.m_targetGroups = true;
                }
            }
        }

        public void SetTarget(int x, int y, int randomHitRange, LogicGameObject target, bool randomHitPosition)
        {
            this.m_target = target;
            this.m_targetPosition.m_x = target.GetMidX() * 8;
            this.m_targetPosition.m_y = target.GetMidY() * 8;

            if (target.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) target;

                if (character.IsFlying())
                {
                    LogicCombatComponent combatComponent = target.GetCombatComponent();

                    this.m_randomHitRange = combatComponent != null && combatComponent.IsHealer() ? 200 : 1000;
                    this.m_flyingTarget = true;
                }

                if (randomHitPosition)
                {
                    LogicVector2 pos = new LogicVector2(this.m_targetPosition.m_x >> 3, this.m_targetPosition.m_y >> 3);

                    int distance = pos.GetDistance(this.GetPosition());

                    this.m_unk168.m_x = this.m_targetPosition.m_x - 8 * this.GetMidX();
                    this.m_unk168.m_y = this.m_targetPosition.m_y - 8 * this.GetMidY();

                    this.m_unk168.Rotate(90);
                    this.m_unk168.Normalize(64);

                    int rnd = ((distance / 10) & this.Rand(randomHitRange)) - distance / 20;

                    this.m_unk168.m_x = this.m_unk168.m_x * rnd / 64;
                    this.m_unk168.m_y = this.m_unk168.m_y * rnd / 64;

                    pos.Destruct();
                }
            }
            else
            {
                int range = target.IsWall() ? 1016 : 2040;

                this.m_unk168.m_x = 8 * x - this.m_targetPosition.m_x;
                this.m_unk168.m_y = 8 * y - this.m_targetPosition.m_y;

                this.m_unk168.Normalize(((target.GetWidthInTiles() - target.PassableSubtilesAtEdge()) << 12) / 3);

                this.m_unk168.m_x += (range & this.Rand(randomHitRange)) * (2 * (this.Rand(randomHitRange + 1) & 1) - 1);
                this.m_unk168.m_y += (range & this.Rand(randomHitRange + 2)) * (2 * (this.Rand(randomHitRange + 3) & 1) - 1);

                this.m_targetPosition.Add(this.m_unk168);

                this.m_randomHitRange = 150;
            }
        }

        public void SetTargetPos(int x, int y, int team, bool flyingTarget)
        {
            this.m_targetPosition.m_x = x * 8;
            this.m_targetPosition.m_y = y * 8;
            this.m_myTeam = team;
            this.m_randomHitRange = flyingTarget ? 1000 : 0;
            this.m_flyingTarget = flyingTarget;
        }

        public void SetTargetPos(int startX, int startY, int x, int y, int minAttackRange, int maxAttackRange, int shockwaveAngle, int shockwavePushStrength,
                                 int shockwaveArcLength,
                                 int shockwaveExpandRadius, int team, bool flyingTarget)
        {
            this.m_targetPosition.m_x = x * 8;
            this.m_targetPosition.m_y = y * 8;
            this.m_myTeam = team;
            this.m_randomHitRange = flyingTarget ? 1000 : 0;
            this.m_flyingTarget = flyingTarget;
            this.m_unk248.m_x = startX;
            this.m_unk248.m_y = startY;
            this.m_shockwaveAngle = shockwaveAngle;
            this.m_shockwaveArcLength = shockwaveArcLength;
            this.m_shockwaveExpandRadius = shockwaveExpandRadius;
            this.m_shockwavePushStrength = shockwavePushStrength;
            this.m_minAttackRange = minAttackRange;
            this.m_maxAttackRange = maxAttackRange;
        }

        public void SetMyTeam(int team)
        {
            this.m_myTeam = team;
        }

        public void SetPenetratingRadius(int radius)
        {
            this.m_penetrating = true;
            this.m_penetratingRadius = radius;
        }

        public void SetDamageRadius(int radius)
        {
            this.m_damageRadius = radius;
        }

        public void SetPreferredTargetDamageMod(LogicData data, int preferredTargetDamageMod)
        {
            this.m_preferredTarget = data;
            this.m_preferredTargetDamageMod = preferredTargetDamageMod;
        }

        public int GetTargetX()
        {
            return this.m_targetPosition.m_x >> 3;
        }

        public int GetTargetY()
        {
            return this.m_targetPosition.m_y >> 3;
        }

        public void SetBouncePosition(LogicVector2 pos)
        {
            this.m_bounceProjectile = true;

            for (int i = 0; i < this.m_bouncePositions.Length; i++)
            {
                if (this.m_bouncePositions[i] == null)
                {
                    this.m_bouncePositions[i] = pos;
                    break;
                }
            }
        }

        public int GetShockwaveArcLength()
        {
            LogicVector2 pos = new LogicVector2(this.GetMidX() - (this.m_targetPosition.m_x >> 3), this.GetMidY() - (this.m_targetPosition.m_y >> 3));

            int length = pos.GetLength();
            int shockwaveLength = this.m_maxAttackRange - length;

            if (shockwaveLength >= this.m_minAttackRange)
            {
                int arcLength = (this.m_shockwaveArcLength << 9) / 100;
                int expandedArcLength = LogicMath.Clamp(shockwaveLength * arcLength / ((this.m_shockwaveExpandRadius << 9) / 100), 0, arcLength);

                if (expandedArcLength < 0)
                {
                    expandedArcLength = arcLength;
                }

                if (expandedArcLength > arcLength)
                {
                    expandedArcLength = arcLength;
                }

                int calculateArcLength = 18000 * expandedArcLength / (314 * shockwaveLength);

                if (calculateArcLength < 180)
                {
                    return calculateArcLength;
                }

                return 180;
            }

            return 0;
        }

        public void UpdateDamage(int percentage)
        {
            this.m_damageTime -= 16;

            if (this.m_damageTime <= 0)
            {
                this.m_damageTime = -0x80000000;
                int damage = this.m_damage * percentage / 100;

                if (this.m_target != null && this.m_target.GetHitpointComponent() != null)
                {
                    this.UpdateTargetDamage(this.m_target, damage);
                }

                if (this.m_target == null)
                {
                    if (!this.m_penetrating)
                    {
                        if (this.m_damageRadius > 0 && this.m_shockwavePushStrength == 0)
                        {
                            this.m_level.AreaDamage(this.m_groupsId, this.m_targetPosition.m_x >> 3, this.m_targetPosition.m_y >> 3, this.m_damageRadius, damage,
                                                    this.m_preferredTarget,
                                                    this.m_preferredTargetDamageMod, this.m_hitEffect, this.m_myTeam, this.m_unk160, this.m_flyingTarget ? 0 : 1, 0,
                                                    this.m_pushBack,
                                                    this.m_gravity, false, 100, 0, this.m_groups, 100, 0);

                            if (this.m_speedMod != 0)
                            {
                                if (this.m_statusEffectTime > 0)
                                {
                                    this.m_level.AreaBoost(this.m_targetPosition.m_x >> 3, this.m_targetPosition.m_y >> 3, this.m_damageRadius, this.m_speedMod, this.m_speedMod, 0,
                                                           0,
                                                           this.m_statusEffectTime >> 4, 0, false);
                                }
                            }

                            if (this.GetProjectileData().GetSlowdownDefensePercent() > 0)
                            {
                                this.m_level.AreaBoost(this.m_targetPosition.m_x >> 3, this.m_targetPosition.m_y >> 3, this.m_damageRadius, 0,
                                                       -this.GetProjectileData().GetSlowdownDefensePercent(), 120);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateTargetDamage(LogicGameObject target, int damage)
        {
            if (target != null && !this.m_dummy && target.GetHitpointComponent() != null)
            {
                int totalDamage = damage;

                if (LogicCombatComponent.IsPreferredTarget(this.m_preferredTarget, target))
                {
                    totalDamage = damage * this.m_preferredTargetDamageMod / 100;
                }

                if (totalDamage >= 0 || target.GetData().GetDataType() == LogicDataType.HERO &&
                    (totalDamage = totalDamage * LogicDataTables.GetGlobals().GetHeroHealMultiplier() / 100) > 0 || !target.IsPreventsHealing())
                {
                    if (this.m_damageRadius <= 0)
                    {
                        target.GetHitpointComponent().CauseDamage(totalDamage, this.m_groupsId, this.m_groups);
                    }
                    else
                    {
                        this.m_level.AreaDamage(this.m_groupsId, target.GetMidX(), target.GetMidY(), this.m_damageRadius, damage, this.m_preferredTarget,
                                                this.m_preferredTargetDamageMod, this.m_hitEffect, this.m_myTeam, this.m_unk160, this.m_flyingTarget ? 0 : 1, 0, this.m_pushBack,
                                                this.m_gravity, false, 100, 0, this.m_groups, 100, 0);
                    }
                }

                int slowdownDefensePercent = this.GetProjectileData().GetSlowdownDefensePercent();

                if (slowdownDefensePercent > 0 && target.GetGameObjectType() == LogicGameObjectType.BUILDING)
                {
                    LogicCombatComponent combatComponent = target.GetCombatComponent();

                    if (combatComponent != null)
                    {
                        combatComponent.Boost(100, -slowdownDefensePercent, 120);
                    }
                }
            }
        }

        public void UpdateBounces()
        {
            LogicArrayList<LogicGameObject> gameObjects = this.GetGameObjectManager().GetGameObjects(LogicGameObjectType.BUILDING);

            int closestBuildingDistance = 0x7FFFFFFF;
            int closestWallDistance = 0x7FFFFFFF;

            LogicBuilding closestBuilding = null;
            LogicBuilding closestWall = null;

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];

                if (building != this.m_target && building.IsAlive())
                {
                    LogicHitpointComponent hitpointComponent = building.GetHitpointComponent();

                    if (hitpointComponent != null && hitpointComponent.IsEnemyForTeam(this.m_myTeam) && !building.IsHidden() && !building.IsWall())
                    {
                        int distanceSquared = this.GetDistanceSquaredTo(building);

                        if (distanceSquared <= 26214400 && distanceSquared < (building.IsWall() ? closestWallDistance : closestBuildingDistance))
                        {
                            int idx = -1;

                            for (int j = this.m_bounceCount; j < LogicProjectile.MAX_BOUNCES; j++)
                            {
                                if (this.m_bounceTargets[j] == building)
                                {
                                    idx = j;
                                    break;
                                }
                            }

                            if (idx == -1)
                            {
                                if (this.m_level.GetTileMap().GetWallInPassableLine(this.GetMidX(), this.GetMidY(), building.GetMidX(), building.GetMidY(), new LogicVector2()))
                                {
                                    if (building.IsWall())
                                    {
                                        closestWallDistance = distanceSquared;
                                        closestWall = building;
                                    }
                                    else
                                    {
                                        closestBuildingDistance = distanceSquared;
                                        closestBuilding = building;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            LogicBuilding nextTarget = closestBuilding ?? closestWall;

            if (nextTarget != null)
            {
                this.m_bounceCount -= 1;
                this.m_targetReached = false;
                this.m_damage /= 2;

                this.SetTarget(this.GetMidX(), this.GetMidY(), this.m_bounceCount, nextTarget, false);
                this.SetInitialPosition(this.m_groups, this.GetMidX(), this.GetMidY());
            }
        }

        public void UpdateShockwavePush(int team, int targetType)
        {
            LogicVector2 position = new LogicVector2(this.GetMidX() - this.m_unk248.m_x, this.GetMidY() - this.m_unk248.m_y);
            int length = position.GetLength();

            if (length >= this.m_minAttackRange)
            {
                int maxRangeDistance = length - this.m_maxAttackRange;
                int maxRadius = length;
                int minRadius = length - 512;

                if (minRadius < this.m_minAttackRange)
                {
                    minRadius = this.m_minAttackRange;
                }

                uint minRadiusSquared = (uint) (minRadius * minRadius);
                uint maxRadiusSquared = (uint) (maxRadius * maxRadius);

                int boostSpeed = this.m_speedMod * maxRangeDistance / this.m_maxAttackRange;
                int boostTime = this.m_statusEffectTime * maxRangeDistance / (16 * this.m_maxAttackRange);
                int shockwaveArcLength = this.GetShockwaveArcLength();

                LogicArrayList<LogicComponent> components = this.GetComponentManager().GetComponents(LogicComponentType.MOVEMENT);
                LogicVector2 pushBackPosition = new LogicVector2();

                for (int i = 0; i < components.Size(); i++)
                {
                    LogicMovementComponent movementComponent = (LogicMovementComponent) components[i];
                    LogicGameObject parent = movementComponent.GetParent();
                    LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();

                    if (!parent.IsHidden())
                    {
                        if (hitpointComponent == null || hitpointComponent.GetTeam() != team)
                        {
                            if (hitpointComponent != null && hitpointComponent.GetParent().IsFlying())
                            {
                                if (targetType == 1)
                                {
                                    continue;
                                }
                            }
                            else if (targetType == 0)
                            {
                                continue;
                            }

                            int distanceX = parent.GetMidX() - this.m_unk248.m_x;
                            int distanceY = parent.GetMidY() - this.m_unk248.m_y;

                            if (LogicMath.Abs(distanceX) <= maxRadius &&
                                LogicMath.Abs(distanceY) <= maxRadius)
                            {
                                int distance = distanceX * distanceX + distanceY * distanceY;

                                if (distance <= maxRadiusSquared && distance >= minRadiusSquared)
                                {
                                    if ((distanceX | distanceY) == 0)
                                    {
                                        distanceX = 1;
                                    }

                                    pushBackPosition.Set(distanceX, distanceY);

                                    int pushBackLength = pushBackPosition.Normalize(512);
                                    int angle =
                                        LogicMath.Abs(LogicMath.NormalizeAngle180(LogicMath.NormalizeAngle180(pushBackPosition.GetAngle()) -
                                                                                  LogicMath.NormalizeAngle180(this.m_shockwaveAngle)));

                                    if (angle < shockwaveArcLength / 2)
                                    {
                                        int pushBack = 100 * (this.m_maxAttackRange + 256 - pushBackLength) / 512;

                                        if (pushBack > this.m_shockwavePushStrength)
                                        {
                                            pushBack = this.m_shockwavePushStrength;
                                        }

                                        movementComponent.GetMovementSystem().ManualPushBack(pushBackPosition, pushBack, 750, this.m_globalId);

                                        if (boostSpeed != 0)
                                        {
                                            movementComponent.GetMovementSystem().Boost(boostSpeed, boostTime);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdatePenetrating(int damageMultiplier)
        {
            LogicVector2 pos1 = new LogicVector2((this.m_targetPosition.m_x >> 3) - this.m_unk248.m_x, (this.m_targetPosition.m_y >> 3) - this.m_unk248.m_y);

            pos1.Normalize(512);

            LogicVector2 pos2 = new LogicVector2(-pos1.m_y, pos1.m_x);

            int distance = ((200 - this.m_areaShieldDelay) * (8 * this.GetSpeed() - 8 * this.m_areaShieldSpeed) / 200 + 8 * this.m_areaShieldSpeed) >> 3;

            LogicArrayList<LogicComponent> components = this.GetComponentManager().GetComponents(LogicComponentType.MOVEMENT);

            for (int i = 0, damage = damageMultiplier * this.m_damage / 100; i < components.Size(); i++)
            {
                LogicMovementComponent component = (LogicMovementComponent) components[i];
                LogicGameObject parent = component.GetParent();
                LogicHitpointComponent hitpointComponent = parent.GetHitpointComponent();

                if (!parent.IsHidden() && hitpointComponent.GetTeam() != this.m_myTeam && hitpointComponent.GetHitpoints() > 0)
                {
                    int distanceX = parent.GetMidX() - this.GetMidX();
                    int distanceY = parent.GetMidY() - this.GetMidY();

                    if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        distanceX += parent.GetWidthInTiles() << 8;
                        distanceY += parent.GetHeightInTiles() << 8;
                    }

                    if ((!component.IsFlying() || this.m_flyingTarget) &&
                        LogicMath.Abs(distanceX) <= this.m_penetratingRadius &&
                        LogicMath.Abs(distanceY) <= this.m_penetratingRadius &&
                        distanceX * distanceX + distanceY * distanceY <= (uint) (this.m_penetratingRadius * this.m_penetratingRadius))
                    {
                        LogicVector2 position = new LogicVector2();

                        if (parent.GetGameObjectType() == LogicGameObjectType.CHARACTER && hitpointComponent.GetMaxHitpoints() <= damage)
                        {
                            int rnd = (byte) this.Rand(parent.GetGlobalID());

                            if (rnd > 170u)
                            {
                                position.Set((pos1.m_x >> 2) + pos2.m_x, (pos1.m_y >> 2) + pos2.m_y);
                            }
                            else
                            {
                                if (rnd > 85)
                                {
                                    position.Set(pos1.m_x, pos1.m_y);
                                }
                                else
                                {
                                    position.Set((pos1.m_x >> 2) - pos2.m_x, (pos1.m_y >> 2) - pos2.m_y);
                                }
                            }

                            if (hitpointComponent.GetInvulnerabilityTime() <= 0)
                            {
                                ((LogicCharacter) parent).Eject(position);
                            }

                            position.Destruct();
                        }
                        else
                        {
                            position.Set(pos1.m_x, pos1.m_y);
                            position.Normalize(distance);

                            if (parent.GetMovementComponent().GetMovementSystem().ManualPushTrap(position, 150, this.m_globalId) || parent.IsHero())
                            {
                                this.UpdateTargetDamage(parent, damage);
                            }
                        }
                    }
                }
            }

            pos1.Destruct();
            pos2.Destruct();
        }

        public override void SubTick()
        {
            base.SubTick();

            this.m_areaShieldSpeed = 0;

            bool isInAreaShield = false;
            int damagePercentage = 100;

            if (this.m_myTeam == 1)
            {
                LogicVector2 areaShield = new LogicVector2();

                if (this.m_level.GetAreaShield(this.GetMidX(), this.GetMidY(), areaShield))
                {
                    this.m_areaShieldSpeed = areaShield.m_x;

                    isInAreaShield = true;
                    damagePercentage = 0;
                }
            }

            if (this.m_targetReached)
            {
                if (this.m_damageTime > 0)
                {
                    this.UpdateDamage(damagePercentage);
                }
            }
            else
            {
                if (this.m_targetGroups)
                {
                    if (this.m_target != null && this.m_groups != null)
                    {
                        LogicCombatComponent combatComponent = this.m_groups.GetCombatComponent();

                        if (combatComponent != null && !combatComponent.IsInRange(this.m_target))
                        {
                            this.m_target = null;
                        }
                    }
                }

                if (isInAreaShield)
                {
                    this.m_areaShieldDelay = LogicMath.Min(this.m_areaShieldDelay + 16, 200);
                }
                else if (this.m_areaShieldDelay > 0)
                {
                    this.m_areaShieldDelay = LogicMath.Max(this.m_areaShieldDelay - 4, 0);
                }

                if (this.m_areaShieldDelay == 0)
                {
                    if (this.m_target != null && this.m_target.GetMovementComponent() != null)
                    {
                        this.m_targetPosition.Set(this.m_target.GetMidX() * 8, this.m_target.GetMidY() * 8);
                        this.m_targetPosition.Add(this.m_unk168);
                    }
                }
                else if (this.m_target != null && this.m_target.GetMovementComponent() != null)
                {
                    int x = this.m_unk168.m_x + this.m_target.GetMidX() * 8;
                    int y = this.m_unk168.m_y + this.m_target.GetMidY() * 8;

                    LogicVector2 tmp1 = new LogicVector2(x - this.m_unk276.m_x, y - this.m_unk276.m_y);
                    LogicVector2 tmp2 = new LogicVector2(this.m_unk152.m_x, this.m_unk152.m_y);

                    int length1 = tmp1.Normalize(512);
                    int length2 = tmp2.Normalize(512);

                    int angle1 = tmp1.GetAngle();
                    int angle2 = tmp2.GetAngle();

                    if (LogicMath.Abs(LogicMath.NormalizeAngle180(angle1 - angle2)) <= 30)
                    {
                        this.m_targetPosition.m_x += LogicMath.Clamp(x - this.m_targetPosition.m_x, length1 / -500, length1 / 500);
                        this.m_targetPosition.m_y += LogicMath.Clamp(y - this.m_targetPosition.m_y, length1 / -500, length1 / 500);
                    }
                    else
                    {
                        this.m_target = null;
                    }
                }

                this.m_unk144.m_x = this.m_targetPosition.m_x - this.m_unk276.m_x;
                this.m_unk144.m_y = this.m_targetPosition.m_y - this.m_unk276.m_y;

                int distance = (200 - this.m_areaShieldDelay) * (8 * this.GetSpeed() - 8 * this.m_areaShieldSpeed) / 200 + 8 * this.m_areaShieldSpeed;

                if (distance * distance >= this.m_unk144.GetDistanceSquaredTo(0, 0))
                {
                    this.TargetReached(damagePercentage);
                }
                else
                {
                    this.m_unk152.m_x = this.m_unk144.m_x;
                    this.m_unk152.m_y = this.m_unk144.m_y;

                    this.m_unk144.Normalize(distance);

                    this.m_unk276.m_x += this.m_unk144.m_x;
                    this.m_unk276.m_y += this.m_unk144.m_y;

                    this.SetPositionXY(this.m_unk276.m_x >> 3, this.m_unk276.m_y >> 3);

                    this.m_unk160.m_x = this.m_unk144.m_x >> 3;
                    this.m_unk160.m_y = this.m_unk144.m_y >> 3;
                }

                if (this.m_shockwavePushStrength > 0)
                {
                    this.UpdateShockwavePush(this.m_myTeam, this.m_flyingTarget ? 0 : 1);
                }

                if (this.m_penetrating)
                {
                    this.UpdatePenetrating(damagePercentage);
                }

                this.m_travelTime += 16;
            }
        }

        public void TargetReached(int damagePercent)
        {
            this.m_damageTime = this.GetProjectileData().GetDamageDelay();
            this.UpdateDamage(damagePercent);
            this.m_targetReached = true;

            if (!this.m_dummy)
            {
                if (this.m_hitEffect != null)
                {
                    if (this.m_target != null)
                    {
                        LogicHitpointComponent hitpointComponent = this.m_target.GetHitpointComponent();

                        if (hitpointComponent != null)
                        {
                            if (!this.m_bounceProjectile)
                            {
                                // Listener.
                            }
                        }
                    }
                    else if (!this.m_penetrating && this.m_shockwavePushStrength == 0)
                    {
                        // Listener.
                    }
                }

                if (this.m_hitEffect2 != null)
                {
                    if (this.m_target != null)
                    {
                        LogicHitpointComponent hitpointComponent = this.m_target.GetHitpointComponent();

                        if (hitpointComponent != null)
                        {
                            if (!this.m_bounceProjectile)
                            {
                                // Listener.
                            }
                        }
                    }
                    else if (!this.m_penetrating && this.m_shockwavePushStrength == 0)
                    {
                        // Listener.
                    }
                }

                if (this.m_target != null)
                {
                    if (this.m_bounceCount > 0)
                    {
                        this.m_bounceTargets[this.m_bounceCount - 1] = this.m_target;
                        this.UpdateBounces();
                    }
                }

                LogicSpellData hitSpell = this.GetProjectileData().GetHitSpell();

                if (hitSpell != null)
                {
                    LogicSpell spell = (LogicSpell) LogicGameObjectFactory.CreateGameObject(hitSpell, this.m_level, this.m_villageType);

                    spell.SetUpgradeLevel(this.GetProjectileData().GetHitSpellLevel());
                    spell.SetInitialPosition(this.GetMidX(), this.GetMidY());
                    spell.SetTeam(1);

                    this.GetGameObjectManager().AddGameObject(spell, -1);
                }

                if (this.m_bounceProjectile)
                {
                    int idx = -1;

                    for (int i = 0; i < LogicProjectile.MAX_BOUNCES; i++)
                    {
                        if (this.m_bouncePositions[i] != null)
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx != -1)
                    {
                        LogicVector2 bouncePosition = this.m_bouncePositions[idx];

                        this.m_bouncePositions[idx] = null;
                        this.m_target = null;

                        LogicEffectData bounceEffect = this.GetProjectileData().GetBounceEffect();

                        if (bounceEffect != null)
                        {
                            this.m_listener.PlayEffect(bounceEffect);
                        }

                        this.m_targetPosition.m_x = 8 * bouncePosition.m_x;
                        this.m_targetPosition.m_y = 8 * bouncePosition.m_y;

                        this.m_randomHitRange = this.m_flyingTarget ? 1000 : 0;

                        // Listener.

                        this.m_targetReached = false;
                        this.m_travelTime = 0;

                        bouncePosition.Destruct();
                    }
                    else
                    {
                        this.m_target = null;
                    }
                }

                if (this.m_targetReached)
                {
                    LogicEffectData destroyedEffect = this.GetProjectileData().GetDestroyedEffect();

                    if (destroyedEffect != null)
                    {
                        // Listener.
                    }
                }
            }
        }

        public int GetSpeed()
        {
            LogicProjectileData projectileData = this.GetProjectileData();

            if (projectileData.GetFixedTravelTime() != 0)
            {
                LogicVector2 position = new LogicVector2();

                position.m_x = (this.m_targetPosition.m_x - this.m_unk276.m_x) >> 3;
                position.m_y = (this.m_targetPosition.m_y - this.m_unk276.m_y) >> 3;

                int remMS = projectileData.GetFixedTravelTime() - this.m_travelTime;
                int speed = position.GetLength();

                if (remMS <= 0)
                {
                    remMS = 1000;
                    speed = projectileData.GetSpeed();
                }

                return 16 * speed / remMS;
            }

            return (int) (16L * projectileData.GetSpeed() / 1000L);
        }
    }
}