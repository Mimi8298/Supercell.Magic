namespace Supercell.Magic.Logic.GameObject.Component
{
    using System;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;
    using Debugger = Supercell.Magic.Titan.Debug.Debugger;

    public sealed class LogicCombatComponent : LogicComponent
    {
        private LogicAttackerItemData m_attackerData;
        private readonly LogicComponentFilter m_enemyFilter; // 48
        private readonly LogicComponentFilter m_groupEnemyFilter; // 616
        private LogicGameObject m_originalTarget; // 44
        private readonly LogicArrayList<LogicGameObject> m_enemyList; // 416
        private readonly LogicRandom m_random; // 484
        private LogicData m_preferredTarget; // 428
        private readonly LogicVector2 m_rangePosition; // 88
        private readonly LogicVector2 m_unk604;
        private readonly LogicVector2 m_targetGroupPosition; // 652
        private readonly LogicVector2 m_unk660;
        private readonly LogicTargetList m_preferredTargetList; // 104
        private readonly LogicTargetList m_targetList; // 192
        private LogicGameObject m_targetGroup; // 612

        private readonly LogicArrayList<LogicGameObject> m_targetGroups; // 640
        private readonly LogicArrayList<LogicGameObject> m_targetGroupEnemyList; // 668
        private readonly LogicArrayList<int> m_targetGroupWeights; // 692
        private readonly LogicArrayList<int> m_groupWeights; // 680
        private readonly LogicArrayList<int> m_targetGroupWeightMultiplier; // 704

        private int[] m_healerTargetWeights; // 488

        private bool m_readyForAttack; // 496
        private readonly bool m_attackMultipleBuildings; // 500
        private bool m_hasAltAttackMode; // 498
        private bool m_attackWithGroups; // 598
        private bool m_groupNoDamageMod; // 597
        private bool m_altMultiTargets; // 499
        private bool m_preferredTargetNotTargeting; // 436
        private bool m_spawnOnAttack; // 503
        private bool m_troopChild;
        private bool m_skeletonSpell; // 599
        private bool m_alerted; // 438
        private bool m_unk497;
        private bool m_unk502;
        private bool m_unk504;
        private bool m_unk596;

        private readonly bool[] m_forceNewTarget; // 505
        private readonly bool[] m_useAltAttackMode; // 510
        private readonly bool[] m_draftUseAltAttackMode;

        private readonly int[] m_boostTime; // 448
        private readonly int[] m_boostDamage; // 456

        private readonly int[] m_attackDelay; // 356
        private readonly int[] m_hitTime; // 284
        private readonly int[] m_burstTime;
        private readonly int[] m_forgetTargetTime;
        private readonly int[] m_attackCooldownOverride; // 304
        private readonly int[] m_hideTime; // 396
        private readonly int[] m_damage2Time; // 724
        private readonly int[] m_damage2X; // 744
        private readonly int[] m_damage2Y; // 764
        private readonly int[] m_aimAngle;
        private readonly int[] m_draftAimAngle;

        private readonly LogicGameObject[] m_targets; // 24

        private int m_ammo;
        private int m_damage;
        private int m_targetGroupsRadius; // 592
        private int m_prepareSpeed; // 344
        private int m_preferredTargetDamageMod;
        private int m_wakeUpTime;
        private int m_undergroundTime; // 492
        private int m_hitCount; // 280
        private int m_activationTime;
        private int m_targetCount; // 20
        private int m_searchRadius; // 72
        private int m_damageTime; // 16
        private int m_damageLevelEffect; // 80
        private int m_summonCooldownTime; // 352
        private int m_totalTargets; // 444
        private int m_ammoReloadingTotalTime; // 476
        private int m_ammoReloadingTime; // 480
        private int m_mergeDamage;
        private int m_deployedHousingSpace; // 788
        private int m_slowTime; // 464
        private int m_slowDamage; // 468
        private int m_attackSpeedBoost; // 716
        private int m_attackSpeedBoostTime; // 720
        private int m_alertTime; // 440
        private int m_originalForgetTargetTime; // 600

        public LogicCombatComponent(LogicGameObject gameObject) : base(gameObject)
        {
            this.m_targetCount = 1;

            this.m_targets = new LogicGameObject[8];

            this.m_boostTime = new int[2];
            this.m_boostDamage = new int[2];

            this.m_forceNewTarget = new bool[8];
            this.m_attackDelay = new int[8];
            this.m_hitTime = new int[8];
            this.m_burstTime = new int[8];
            this.m_forgetTargetTime = new int[8];
            this.m_attackCooldownOverride = new int[8];
            this.m_hideTime = new int[8];
            this.m_damage2Time = new int[8];
            this.m_damage2X = new int[8];
            this.m_damage2Y = new int[8];

            for (int i = 0; i < 8; i++)
            {
                this.m_damage2X[i] = -1;
                this.m_damage2Y[i] = -1;
            }

            this.m_aimAngle = new int[8];
            this.m_draftAimAngle = new int[8];
            this.m_useAltAttackMode = new bool[8];
            this.m_draftUseAltAttackMode = new bool[8];

            this.m_unk502 = true;

            this.m_rangePosition = new LogicVector2();
            this.m_unk604 = new LogicVector2(-1, -1);
            this.m_targetGroupPosition = new LogicVector2(-1, -1);
            this.m_unk660 = new LogicVector2(-1, -1);
            this.m_preferredTargetList = new LogicTargetList();
            this.m_targetList = new LogicTargetList();
            this.m_enemyFilter = new LogicComponentFilter();
            this.m_groupEnemyFilter = new LogicComponentFilter();
            this.m_random = new LogicRandom();

            this.m_targetGroups = new LogicArrayList<LogicGameObject>();
            this.m_targetGroupEnemyList = new LogicArrayList<LogicGameObject>();
            this.m_targetGroupWeights = new LogicArrayList<int>();
            this.m_groupWeights = new LogicArrayList<int>();
            this.m_targetGroupWeightMultiplier = new LogicArrayList<int>();

            this.m_enemyFilter.SetComponentType(LogicComponentType.HITPOINT);
            this.m_groupEnemyFilter.SetComponentType(LogicComponentType.HITPOINT);

            if (gameObject.GetHitpointComponent() == null)
            {
                Debugger.Error("LogicCombatComponent::constructor - Enemy filter works only if Hitpoint component is initialized!");
            }

            this.m_enemyFilter.PassEnemyOnly(gameObject);
            this.m_enemyList = new LogicArrayList<LogicGameObject>(20);

            if (this.m_parent.GetData().GetDataType() == LogicDataType.CHARACTER)
            {
                LogicCharacterData characterData = (LogicCharacterData) this.m_parent.GetData();

                if (characterData.IsSecondaryTroop())
                {
                    this.m_totalTargets = 1;
                }

                this.m_attackMultipleBuildings = characterData.GetAttackMultipleBuildings();
            }

            this.m_preferredTargetDamageMod = 100;
            this.m_random.SetIteratedRandomSeed(5512);
        }

        public override void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            base.RemoveGameObjectReferences(gameObject);

            for (int i = 0; i < this.m_targetCount; i++)
            {
                if (this.m_targets[0] == gameObject || this.m_originalTarget == gameObject)
                {
                    this.m_targets[i] = null;
                    this.m_hitTime[i] = 0;

                    if (this.m_burstTime[i] != 0)
                    {
                        this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                    }

                    this.m_burstTime[i] = 0;
                    this.m_originalTarget = null;

                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) this.m_parent;

                        if (character.GetCharacterData().IsUnderground())
                        {
                            this.m_hideTime[0] = LogicDataTables.GetGlobals().GetMinerHideTime() + this.m_parent.Rand(0) % LogicDataTables.GetGlobals().GetMinerHideTimeRandom();
                        }
                    }
                }
            }

            for (int i = 0; i < this.m_enemyList.Size(); i++)
            {
                if (this.m_enemyList[i] == gameObject)
                {
                    this.m_enemyList.Remove(i--);
                }
            }

            for (int i = 0; i < this.m_targetGroups.Size(); i++)
            {
                if (this.m_targetGroups[i] == gameObject)
                {
                    this.m_targetGroups.Remove(i--);
                }
            }

            if (this.m_targetGroup == gameObject)
            {
                this.m_targetGroup = null;
            }
        }

        public void SetPreferredTarget(LogicData data, int preferredTargetDamageMod, bool preferredTargetNotTargeting)
        {
            this.m_preferredTarget = data;
            this.m_preferredTargetDamageMod = preferredTargetDamageMod;
            this.m_preferredTargetNotTargeting = preferredTargetNotTargeting;
        }

        public void SetAttackValues(LogicAttackerItemData data, int damagePercentage)
        {
            this.m_attackerData = data;
            this.m_prepareSpeed = data.GetPrepareSpeed();
            this.m_damage = damagePercentage * data.GetDamage(0, data.GetMultiTargets(false)) / 100;
            this.m_preferredTarget = data.GetPreferredTargetData();
            this.m_preferredTargetDamageMod = 100 * data.GetPreferredTargetDamageMod();
            this.m_preferredTargetNotTargeting = data.GetPreferredTargetNoTargeting();

            this.m_unk502 = true;

            this.m_summonCooldownTime = data.GetSummonCooldown() / 4;
            this.m_wakeUpTime = data.GetWakeUpSpeed();

            this.m_spawnOnAttack = data.GetSpawnOnAttack();

            if (this.m_attackerData.GetAttackSpeed() < 64)
            {
                Debugger.Error(this.m_parent.GetData().GetName() + " has too fast attack speed!");
            }

            if (this.m_attackerData.GetFightWithGroups())
            {
                if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    this.m_attackWithGroups = true;
                    this.m_targetGroupsRadius = this.m_attackerData.GetTargetGroupsRadius();
                    this.m_groupNoDamageMod = true;
                }
            }

            if (this.m_attackerData.HasAlternativeAttackMode())
            {
                this.m_hasAltAttackMode = true;
                this.m_altMultiTargets = this.m_attackerData.GetMultiTargets(true);
                this.m_targetCount = this.m_altMultiTargets ? this.m_attackerData.GetAltNumMultiTargets() : 1;
            }

            if (this.m_attackerData.GetAmmoCount() > 0)
            {
                int attackSpeed = this.m_attackerData.GetAttackSpeed();
                int unk = 1;

                if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                {
                    attackSpeed = this.m_attackerData.GetAltAttackSpeed();
                }

                if (attackSpeed >= 64)
                {
                    unk = attackSpeed / 64;
                }

                this.m_ammoReloadingTotalTime = unk;
                this.m_ammoReloadingTime = unk;
            }

            if (this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0)
            {
                this.m_enemyFilter.PassEnemyOnly(this.m_parent);
            }
            else
            {
                if (this.m_attackerData.GetDamage2() > 0)
                {
                    this.m_enemyFilter.PassEnemyOnly(this.m_parent);
                }
                else
                {
                    this.m_enemyFilter.PassFriendlyOnly(this.m_parent);
                }
            }
        }

        public void SetAttackDelay(int idx, int time)
        {
            this.m_attackDelay[idx] = time;

            this.m_targets[idx] = null;
            this.m_originalTarget = null;
        }

        public LogicData GetPreferredTarget()
        {
            return this.m_preferredTarget;
        }

        public void SetActivationTime(int value)
        {
            this.m_activationTime = value;
        }

        public void SetTroopChild(bool child)
        {
            this.m_troopChild = child;
        }

        public void SetSearchRadius(int radius)
        {
            this.m_searchRadius = radius;
        }

        public int Rand(int rnd)
        {
            return this.m_parent.Rand(rnd);
        }

        public LogicGameObject GetTarget(int idx)
        {
            return this.m_targets[idx];
        }

        public bool IsTargetValid(LogicGameObject gameObject)
        {
            if (gameObject != null)
            {
                LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                if (combatComponent != null && combatComponent.m_undergroundTime > 0)
                {
                    if (this.m_damage > 0 || this.m_attackerData.GetDamage2() > 0 || this.m_attackerData.GetShockwavePushStrength() != 0)
                    {
                        return false;
                    }
                }

                LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

                if (this.m_damage > 0 || this.m_attackerData.GetDamage2() > 0 || this.m_attackerData.GetShockwavePushStrength() != 0)
                {
                    if (hitpointComponent != null && hitpointComponent.IsEnemy(this.m_parent))
                    {
                        if (this.m_parent.GetMovementComponent() == null)
                        {
                            return this.IsInRange(gameObject);
                        }

                        return true;
                    }
                }
                else
                {
                    if (hitpointComponent != null && hitpointComponent.GetTeam() == this.m_parent.GetHitpointComponent().GetTeam())
                    {
                        return gameObject.IsAlive();
                    }
                }
            }

            return false;
        }

        public LogicAttackerItemData GetAttackerItemData()
        {
            return this.m_attackerData;
        }

        public bool HasAltAttackMode()
        {
            return this.m_hasAltAttackMode;
        }

        public bool UseAltAttackMode(int layoutId, bool draft)
        {
            if (draft)
            {
                return this.m_draftUseAltAttackMode[layoutId];
            }

            return this.m_useAltAttackMode[layoutId];
        }

        public bool IsInRange(LogicGameObject gameObject)
        {
            int activeLayout = this.m_parent.GetLevel().GetCurrentLayout();
            int aimAngle = 0;

            if (this.m_attackerData.GetTargetingConeAngle() != 0)
            {
                aimAngle = this.m_aimAngle[activeLayout];
            }

            this.CalculateDistance(gameObject, this.m_rangePosition);

            int distance = this.m_rangePosition.m_x;
            int angle = this.m_rangePosition.m_y;

            if (this.m_parent.IsFlying() ||
                gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                int minAttackRange = this.m_attackerData.GetMinAttackRange();

                if (distance < minAttackRange * minAttackRange)
                    return false;

                int attackRange = this.GetAttackRange(activeLayout, false);

                if (distance > (attackRange + 256) * (attackRange + 256))
                    return false;
            }
            else
            {
                int attackRange = this.GetAttackRange(activeLayout, false);

                if (distance >= attackRange * attackRange)
                    return false;
            }

            if (this.m_attackerData.GetTargetingConeAngle() > 0)
            {
                int attackAngle = LogicMath.GetAngleBetween(angle, LogicMath.NormalizeAngle180(aimAngle));

                if (attackAngle > this.m_attackerData.GetTargetingConeAngle() / 2)
                    return false;
            }

            return true;
        }

        public bool IsInLine(LogicGameObject gameObject)
        {
            if (gameObject != null)
            {
                if (gameObject.IsWall() || this.m_parent.GetGameObjectType() != LogicGameObjectType.CHARACTER)
                {
                    return true;
                }

                LogicCharacter parent = (LogicCharacter) this.m_parent;

                if (!parent.GetCharacterData().GetAttackOverWalls())
                {
                    return !this.m_parent.GetLevel().GetTileMap()
                                .GetWallInPassableLine(this.m_parent.GetMidX(), this.m_parent.GetMidY(), gameObject.GetMidX(), gameObject.GetMidY(), new LogicVector2());
                }
            }

            return true;
        }

        public void CalculateDistance(LogicGameObject gameObject, LogicVector2 position)
        {
            int midX = this.m_parent.GetMidX();
            int midY = this.m_parent.GetMidY();
            int distance;
            int angle = 0;

            if (this.m_parent.IsFlying() ||
                gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                distance = this.m_parent.GetDistanceSquaredTo(gameObject);

                if (this.m_attackerData.GetTargetingConeAngle() > 0)
                {
                    angle = LogicMath.NormalizeAngle180(LogicMath.GetAngle(gameObject.GetMidX() - midX, gameObject.GetMidY() - midY));
                }
            }
            else
            {
                int passableSubtilesAtEdge = gameObject.PassableSubtilesAtEdge() << 8;

                int goX = gameObject.GetX();
                int goY = gameObject.GetY();

                int goClampedX = LogicMath.Clamp(midX, goX + passableSubtilesAtEdge, goX + (gameObject.GetWidthInTiles() << 9) - passableSubtilesAtEdge);
                int goClampedY = LogicMath.Clamp(midY, goY + passableSubtilesAtEdge, goY + (gameObject.GetHeightInTiles() << 9) - passableSubtilesAtEdge);

                distance = (goClampedX - midX) * (goClampedX - midX) + (goClampedY - midY) * (goClampedY - midY);

                if (this.m_attackerData.GetTargetingConeAngle() > 0)
                {
                    angle = LogicMath.NormalizeAngle180(LogicMath.GetAngle(goClampedX - midX, goClampedY - midY));
                }
            }

            position.m_x = distance;
            position.m_y = angle;
        }

        public int GetAttackRange(int layout, bool draft)
        {
            if (draft)
            {
                if (this.m_draftUseAltAttackMode[layout])
                {
                    return this.m_attackerData.GetAttackRange(true);
                }
            }
            else if (this.m_useAltAttackMode[layout])
            {
                return this.m_attackerData.GetAttackRange(true);
            }

            int attackRange = this.m_attackerData.GetAttackRange(false);

            if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;
                LogicCharacterData data = character.GetCharacterData();

                if (character.GetSpecialAbilityAvailable() && data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_SPECIAL_PROJECTILE)
                {
                    attackRange += data.GetSpecialAbilityAttribute2(character.GetUpgradeLevel()) * this.m_attackerData.GetAttackRange(false) / 100;
                }
            }

            return attackRange;
        }

        public bool IsWallBreaker()
        {
            return !this.m_preferredTargetNotTargeting && this.m_preferredTarget != null && this.m_preferredTarget.GetDataType() == LogicDataType.BUILDING_CLASS &&
                   ((LogicBuildingClassData) this.m_preferredTarget).IsWall();
        }

        public void SearchTarget(int idx, LogicGameObject prevTarget)
        {
            if (this.m_attackWithGroups && this.SearchTargetWithGroups(idx, prevTarget))
                return;

            if (this.m_unk504)
            {
                this.SelectTarget(((LogicCharacter) this.m_parent).GetParent().GetCombatComponent().m_targets[idx], idx);
                return;
            }

            if (this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.GetDamage2() > 0 ||
                !LogicDataTables.GetGlobals().UseSmarterHealer() && !LogicDataTables.GetGlobals().UseStickToClosestUnitHealer())
            {
                if (this.m_attackerData.GetPreferredTargetNoTargeting())
                {
                    LogicGameObject searchTargetNoTargeting = this.SearchTargetNoTargeting();

                    if (searchTargetNoTargeting != null && this.IsTargetValid(searchTargetNoTargeting))
                    {
                        this.SelectTarget(searchTargetNoTargeting, idx);
                        return;
                    }
                }

                bool wallBreaker = false;

                if (this.IsWallBreaker())
                {
                    LogicGameObject wall = this.SelectWall();

                    if (wall != null && this.IsTargetValid(wall))
                    {
                        this.SelectTarget(wall, idx);
                        return;
                    }

                    wallBreaker = true;
                }

                this.m_targetList.Clear();
                this.m_preferredTargetList.Clear();
                this.m_enemyList.Clear();

                this.m_parent.GetGameObjectManager().GetGameObjects(this.m_enemyList, this.m_enemyFilter);

                LogicArrayList<LogicGameObject> troopChildTargets = new LogicArrayList<LogicGameObject>();
                LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();

                for (int i = 0, cnt = this.m_enemyList.Size(); i < cnt; i++)
                {
                    LogicGameObject gameObject = this.m_enemyList[i];

                    if (!gameObject.IsHidden())
                    {
                        if (gameObject.IsStealthy())
                        {
                            if (this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.GetDamage2() > 0)
                            {
                                continue;
                            }
                        }

                        if (this.CanAttackHeightCheck(gameObject))
                        {
                            if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                            {
                                LogicCharacter character = (LogicCharacter) gameObject;
                                LogicCombatComponent combatComponent;

                                if (character.GetSpawnDelay() > 0 || character.GetSpawnIdleTime() > 0 ||
                                    (combatComponent = character.GetCombatComponent()) != null && combatComponent.m_undergroundTime > 0)
                                {
                                    continue;
                                }

                                if (character.GetChildTroops() != null)
                                {
                                    continue;
                                }
                            }

                            bool containsTarget = false;

                            for (int j = 0; j < this.m_targetCount; j++)
                            {
                                if (idx != j)
                                {
                                    if (this.m_targets[j] == gameObject)
                                    {
                                        containsTarget = true;
                                    }
                                }
                            }

                            if (containsTarget)
                            {
                                continue;
                            }

                            int distance;

                            if (!LogicDataTables.GetGlobals().MorePreciseTargetSelection() ||
                                LogicDataTables.GetGlobals().MovingUnitsUseSimpleSelect() && movementComponent != null)
                            {
                                int distanceX = (gameObject.GetMidX() - this.m_parent.GetMidX()) >> 9;
                                int distanceY = (gameObject.GetMidY() - this.m_parent.GetMidY()) >> 9;

                                distance = distanceX * distanceX + distanceY * distanceY;
                            }
                            else
                            {
                                this.CalculateDistance(gameObject, this.m_rangePosition);
                                distance = this.m_rangePosition.m_x;
                            }

                            if (LogicDataTables.GetGlobals().GetMinerTargetRandomPercentage() > 0 && this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                            {
                                LogicCharacter character = (LogicCharacter) gameObject;
                                LogicCharacterData data = character.GetCharacterData();

                                if (data.IsUnderground())
                                {
                                    if (this.m_hitCount > 0)
                                    {
                                        distance -= distance / 100 * this.m_random.Rand(LogicDataTables.GetGlobals().GetMinerTargetRandomPercentage());
                                    }
                                }
                            }

                            if (this.m_searchRadius > 0 && movementComponent != null)
                            {
                                LogicBuilding heroBaseBuilding = movementComponent.GetBaseBuilding();

                                if (heroBaseBuilding != null)
                                {
                                    this.m_rangePosition.m_x = gameObject.GetMidX() - heroBaseBuilding.GetMidX();
                                    this.m_rangePosition.m_y = gameObject.GetMidY() - heroBaseBuilding.GetMidY();

                                    if (this.m_rangePosition.GetLengthSquared() > (this.m_searchRadius << 9) * (this.m_searchRadius << 9))
                                    {
                                        continue;
                                    }
                                }

                                if (!this.m_parent.IsHero())
                                {
                                    this.m_rangePosition.m_x = gameObject.GetMidX() - this.m_parent.GetMidX();
                                    this.m_rangePosition.m_y = gameObject.GetMidY() - this.m_parent.GetMidY();

                                    int castleDefenderSearchRadius = LogicDataTables.GetGlobals().GetClanDefenderSearchRadius();

                                    if (this.m_rangePosition.GetLengthSquared() > (castleDefenderSearchRadius << 9) * (castleDefenderSearchRadius << 9))
                                    {
                                        continue;
                                    }
                                }
                            }

                            if ((this.m_attackerData.GetMinAttackRange() > 0 || this.m_attackerData.GetTargetingConeAngle() > 0) && !this.IsInRange(gameObject))
                            {
                                continue;
                            }

                            if (this.m_damage <= 0 && this.m_attackerData.GetShockwavePushStrength() == 0 && this.m_attackerData.GetDamage2() <= 0)
                            {
                                if (gameObject != this.m_parent && gameObject.IsAlive() && (wallBreaker || !this.IsWall(gameObject)))
                                {
                                    LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

                                    if (hitpointComponent != null)
                                    {
                                        if (!hitpointComponent.HasFullHitpoints() || hitpointComponent.IsDamagedRecently())
                                        {
                                            this.m_preferredTargetList.AddCandidate(gameObject, distance);
                                            continue;
                                        }
                                    }

                                    this.m_targetList.AddCandidate(gameObject, distance);
                                }

                                continue;
                            }

                            if (wallBreaker || !this.IsWall(gameObject))
                            {
                                if (this.m_troopChild)
                                {
                                    if (this.IsTargetValid(gameObject))
                                    {
                                        troopChildTargets.Add(gameObject);
                                    }

                                    continue;
                                }

                                if (this.IsPreferredTargetForMe(gameObject, distance))
                                {
                                    this.m_preferredTargetList.AddCandidate(gameObject, distance);
                                    continue;
                                }

                                this.m_targetList.AddCandidate(gameObject, distance);
                            }
                        }
                    }
                }

                LogicGameObject target = this.m_preferredTargetList.EvaluateTargets(movementComponent);

                if (target != null && movementComponent == null && !this.IsInRange(target))
                {
                    target = null;
                }

                if (troopChildTargets.Size() <= 0)
                {
                    if (target == null)
                    {
                        if (this.m_preferredTarget != null && this.m_preferredTarget.GetDataType() == LogicDataType.BUILDING)
                        {
                            this.m_preferredTarget = ((LogicBuildingData) this.m_preferredTarget).GetBuildingClass();
                            return;
                        }

                        target = this.m_targetList.EvaluateTargets(movementComponent);
                    }
                }
                else
                {
                    target = troopChildTargets[this.m_random.Rand(troopChildTargets.Size())];
                }

                this.SelectTarget(target, idx);
                return;
            }

            LogicGameObject selectedTarget = LogicDataTables.GetGlobals().UseSmarterHealer() ? this.SearchSmartHealerTarget() : this.SearchHealerTargetUsingStick();

            if (selectedTarget != null && this.IsTargetValid(selectedTarget))
            {
                this.SelectTarget(selectedTarget, idx);
            }
        }

        public LogicGameObject SearchTargetNoTargeting()
        {
            this.m_targetList.Clear();
            this.m_preferredTargetList.Clear();
            this.m_enemyList.Clear();

            this.m_parent.GetGameObjectManager().GetGameObjects(this.m_enemyList, this.m_enemyFilter);

            LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();
            LogicGameObject closestTarget = null;

            for (int i = 0, minDistance = 0; i < this.m_enemyList.Size(); i++)
            {
                LogicGameObject gameObject = this.m_enemyList[i];

                if (!gameObject.IsHidden())
                {
                    if (gameObject.IsStealthy())
                    {
                        if (this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.GetDamage2() > 0)
                        {
                            continue;
                        }
                    }

                    if (this.CanAttackHeightCheck(gameObject))
                    {
                        int distance;

                        if (!LogicDataTables.GetGlobals().MorePreciseTargetSelection() ||
                            LogicDataTables.GetGlobals().MovingUnitsUseSimpleSelect() && movementComponent != null)
                        {
                            int distanceX = (gameObject.GetMidX() - this.m_parent.GetMidX()) >> 9;
                            int distanceY = (gameObject.GetMidY() - this.m_parent.GetMidY()) >> 9;

                            distance = distanceX * distanceX + distanceY * distanceY;
                        }
                        else
                        {
                            this.CalculateDistance(gameObject, this.m_rangePosition);
                            distance = this.m_rangePosition.m_x;
                        }

                        if (distance < minDistance || closestTarget == null)
                        {
                            minDistance = distance;
                            closestTarget = gameObject;
                        }
                    }
                }
            }

            return closestTarget;
        }

        public int IntArrayListToChecksum(LogicArrayList<int> arrayList)
        {
            int checksum = 0;

            for (int i = 0; i < arrayList.Size(); i++)
            {
                checksum += arrayList[i];
            }

            return checksum;
        }

        public bool SearchTargetWithGroups(int idx, LogicGameObject prevTarget)
        {
            if (this.m_originalForgetTargetTime == 0)
            {
                this.RefreshTargetGroups(true);
                this.m_originalForgetTargetTime = LogicDataTables.GetGlobals().GetForgetTargetTime();
            }

            this.m_targetGroupEnemyList.Clear();
            this.m_groupWeights.Clear();

            this.m_targetGroupEnemyList.EnsureCapacity(this.m_enemyList.Size());
            this.m_groupWeights.EnsureCapacity(this.m_enemyList.Size());

            int attackRange = this.GetAttackRange(this.m_parent.GetLevel().GetCurrentLayout(), false);
            int attackRangeSquared = attackRange * attackRange;
            int originalTargetDistanceSquared = (attackRange - 512) * (attackRange - 512);
            int maxGroupWeight = 0;

            LogicGameObject groupTarget = null;

            for (int i = 0; i < this.m_targetGroups.Size(); i++)
            {
                LogicGameObject gameObject = this.m_targetGroups[i];
                LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                if (combatComponent != null)
                {
                    int groupWeight = 1;

                    if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) gameObject;

                        groupWeight = this.m_attackWithGroups
                            ? character.GetCharacterData().GetFriendlyGroupWeight()
                            : character.GetCharacterData().GetEnemyGroupWeight();
                    }

                    LogicGameObject target = combatComponent.m_targets[0];

                    if (target != null)
                    {
                        if (!target.IsWall() || (target = combatComponent.m_originalTarget) != null &&
                            (target.GetDistanceSquaredTo(combatComponent.m_targets[0]) <= originalTargetDistanceSquared ||
                             target.GetDistanceSquaredTo(this.m_parent) <= attackRangeSquared))
                        {
                            int gameObjectIdx = this.m_targetGroupEnemyList.IndexOf(target);

                            if (gameObjectIdx == -1)
                            {
                                this.m_targetGroupEnemyList.Add(target);
                                this.m_groupWeights.Add(groupWeight);
                            }
                            else
                            {
                                groupWeight += this.m_groupWeights[gameObjectIdx];
                                this.m_groupWeights[gameObjectIdx] = groupWeight;
                            }

                            if (groupWeight > maxGroupWeight)
                            {
                                maxGroupWeight = groupWeight;
                                groupTarget = target;
                            }
                        }
                    }
                }
            }
            
            if (groupTarget == null)
            {
                for (int i = 0; i < this.m_targetGroups.Size(); i++)
                {
                    LogicGameObject gameObject = this.m_targetGroups[i];
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                    if (combatComponent != null)
                    {
                        int groupWeight = 1;

                        if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            LogicCharacter character = (LogicCharacter) gameObject;

                            groupWeight = this.m_attackWithGroups
                                ? character.GetCharacterData().GetFriendlyGroupWeight() 
                                : character.GetCharacterData().GetEnemyGroupWeight();
                        }

                        LogicGameObject target = combatComponent.m_targets[0];

                        if (target != null)
                        {
                            int gameObjectIdx = this.m_targetGroupEnemyList.IndexOf(target);

                            if (gameObjectIdx == -1)
                            {
                                this.m_targetGroupEnemyList.Add(target);
                                this.m_groupWeights.Add(groupWeight);
                            }
                            else
                            {
                                groupWeight += this.m_groupWeights[gameObjectIdx];
                                this.m_groupWeights[gameObjectIdx] = groupWeight;
                            }

                            if (groupWeight > maxGroupWeight)
                            {
                                maxGroupWeight = groupWeight;
                                groupTarget = target;
                            }
                        }
                    }
                }

                if (groupTarget == null)
                {
                    if (prevTarget != null && this.m_alertTime <= 0 && prevTarget.IsAlive() && this.IsInRange(prevTarget))
                    {
                        groupTarget = prevTarget;
                    }
                }
            }
            
            if (groupTarget != null)
            {
                this.SelectTarget(groupTarget, idx);
                return true;
            }
            
            return false;
        }

        public LogicGameObject SearchSmartHealerTarget()
        {
            if (this.m_healerTargetWeights == null)
            {
                this.m_healerTargetWeights = new int[625];
            }

            Array.Clear(this.m_healerTargetWeights, 0, 625);

            this.m_enemyList.Clear();
            this.m_parent.GetGameObjectManager().GetGameObjects(this.m_enemyList, this.m_enemyFilter);

            for (int i = 0; i < this.m_enemyList.Size(); i++)
            {
                LogicGameObject gameObject = this.m_enemyList[i];

                if (gameObject.IsAlive() && gameObject.GetData() != this.m_parent.GetData() && this.CanAttackHeightCheck(gameObject))
                {
                    uint distanceOffsetX = (uint) (gameObject.GetMidX() >> 10);
                    uint distanceOffsetY = (uint) (gameObject.GetMidY() >> 10);

                    int offset = (int) (distanceOffsetX + 25 * distanceOffsetY);

                    if (distanceOffsetX >= 25 || distanceOffsetY >= 25)
                    {
                        offset = -1;
                    }

                    if (offset >= 0)
                    {
                        int housingSpaceWeights = 0;

                        if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            housingSpaceWeights = ((LogicCharacter) gameObject).GetCharacterData().GetHousingSpace() / 2 + 1;
                        }

                        int weight = this.m_healerTargetWeights[offset] + (housingSpaceWeights << 16);

                        LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

                        if (hitpointComponent != null)
                        {
                            if (!hitpointComponent.HasFullHitpoints() || hitpointComponent.IsDamagedRecently())
                            {
                                weight += housingSpaceWeights;
                            }
                        }

                        this.m_healerTargetWeights[offset] = weight;
                    }
                }
            }

            int currentTargetOffset = -1;

            LogicGameObject currentTarget = this.m_targets[0];
            LogicGameObject selectTarget = currentTarget;

            if (currentTarget != null)
            {
                uint distanceOffsetX = (uint) (currentTarget.GetMidX() >> 10);
                uint distanceOffsetY = (uint) (currentTarget.GetMidY() >> 10);

                currentTargetOffset = (int) (distanceOffsetX + 25 * distanceOffsetY);

                if (distanceOffsetX >= 25 || distanceOffsetY >= 25)
                {
                    currentTargetOffset = -1;
                }
            }

            int minAttackRange = LogicMath.Max(0, LogicMath.GetRadius(-7168, 512) - this.m_attackerData.GetAttackRange(false));

            if (currentTargetOffset < 0)
            {
                minAttackRange = -((int) ((minAttackRange >> 9) + ((uint) ((minAttackRange >> 9) * (minAttackRange >> 9)) >> 2)));
            }
            else
            {
                minAttackRange = this.GetHealerTargetCost(currentTargetOffset) -
                                 LogicCombatComponent.GetHealerAttackRange(this.m_parent.GetMidX(), this.m_parent.GetMidY(), this.m_attackerData.GetAttackRange(false),
                                                                           currentTargetOffset);
            }

            int maxAttackRange = LogicMath.Max(0, LogicMath.GetRadius(-25088, 512) - this.m_attackerData.GetAttackRange(false));

            maxAttackRange = -((int) ((maxAttackRange >> 9) + ((uint) ((maxAttackRange >> 9) * (maxAttackRange >> 9)) >> 2)));
            minAttackRange += minAttackRange > 0 ? minAttackRange / 2 + 1 : 1;

            int minOffset = -1;
            int maxOffset = -1;

            for (int i = 0, midX = this.m_parent.GetMidX(), midY = this.m_parent.GetMidY(); i < 625; i++)
            {
                int healerAttackRange = LogicCombatComponent.GetHealerAttackRange(midX, midY, this.m_attackerData.GetAttackRange(false), i);
                int healerTargetCost = this.GetHealerTargetCost(i);
                int weights = LogicMath.Min(this.m_healerTargetWeights[i] >> 16, 20);

                if (healerTargetCost > 0)
                {
                    int tmp = healerTargetCost - healerAttackRange;

                    if (tmp > minAttackRange)
                    {
                        minOffset = i;
                        minAttackRange = tmp;
                    }
                }

                if (weights > 1)
                {
                    int tmp = weights - healerAttackRange;

                    if (tmp > maxAttackRange)
                    {
                        maxOffset = i;
                        maxAttackRange = tmp;
                    }
                }
            }

            if (minOffset < 0)
            {
                if (selectTarget != null)
                {
                    return selectTarget;
                }

                if (maxOffset < 0)
                {
                    return null;
                }

                minOffset = maxOffset;
            }

            int closestDistance = 0x7FFFFFFF;

            for (int i = 0, x = ((minOffset % 25) << 10) | 512, y = ((minOffset / 25) << 10) | 512; i < this.m_enemyList.Size(); i++)
            {
                LogicGameObject gameObject = this.m_enemyList[i];

                if (gameObject.IsAlive() && this.CanAttackHeightCheck(gameObject) && gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    this.m_rangePosition.m_x = x - gameObject.GetMidX();
                    this.m_rangePosition.m_y = y - gameObject.GetMidY();

                    int distanceSquared = this.m_rangePosition.GetLengthSquared();

                    if (distanceSquared < closestDistance)
                    {
                        closestDistance = distanceSquared;
                        selectTarget = gameObject;
                    }
                }
            }

            return selectTarget;
        }

        public int GetHealerTargetCost(int weights)
        {
            int x = weights % 25;
            int y = weights / 25;

            uint weights1X = (uint) x;
            uint weights1Y = (uint) (((y << 10) - 1024) >> 10);
            int weights1 = (int) (weights1X + 25 * weights1Y);

            if (weights1X >= 25 || weights1Y >= 25)
            {
                weights1 = -1;
            }

            uint weights2X = (uint) x;
            uint weights2Y = (uint) (((y << 10) + 1024) >> 10);
            int weights2 = (int) (weights2X + 25 * weights2Y);

            if (weights2X >= 25 || weights2Y >= 25)
            {
                weights2 = -1;
            }

            uint weights3X = (uint) (((x << 10) - 1024) >> 10);
            uint weights3Y = (uint) y;
            int weights3 = (int) (weights3X + 25 * weights3Y);

            if (weights3X >= 25 || weights3Y >= 25)
            {
                weights3 = -1;
            }

            uint weights4X = (uint) (((x << 10) + 1024) >> 10);
            uint weights4Y = (uint) y;
            int weights4 = (int) (weights4X + 25 * weights4Y);

            if (weights4X >= 25 || weights4Y >= 25)
            {
                weights4 = -1;
            }

            int cost = (ushort) this.m_healerTargetWeights[weights];

            if (weights1 >= 0)
            {
                cost += (ushort) this.m_healerTargetWeights[weights1] >> 2;
            }

            if (weights2 >= 0)
            {
                cost += (this.m_healerTargetWeights[weights2] >> 2) & 0x3FFF;
            }

            if (weights3 >= 0)
            {
                cost += (this.m_healerTargetWeights[weights3] >> 2) & 0x3FFF;
            }

            if (weights4 >= 0)
            {
                cost += (this.m_healerTargetWeights[weights4] >> 2) & 0x3FFF;
            }

            return LogicMath.Min(cost / 2, 20);
        }

        public static int GetHealerAttackRange(int x, int y, int attackRange, int currentTargetWeights)
        {
            int radius = LogicMath.GetRadius((((currentTargetWeights % 25) << 10) | 512) - x, (((currentTargetWeights / 25) << 10) | 512) - y);
            int range = LogicMath.Max(0, radius - attackRange);

            return (int) ((range >> 9) + ((uint) ((range >> 9) * (range >> 9)) >> 2));
        }

        public LogicGameObject SearchHealerTargetUsingStick()
        {
            this.m_enemyList.Clear();
            this.m_parent.GetGameObjectManager().GetGameObjects(this.m_enemyList, this.m_enemyFilter);

            LogicGameObject closestTarget = null;

            for (int i = 0, minDistance = 0x7FFFFFFF; i < this.m_enemyList.Size(); i++)
            {
                LogicGameObject gameObject = this.m_enemyList[i];

                if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER && gameObject.IsAlive() && this.CanAttackHeightCheck(gameObject))
                {
                    if (gameObject.GetData() != this.m_parent.GetData() && this.IsTargetValid(gameObject) && gameObject.GetHitpointComponent() != null)
                    {
                        this.m_rangePosition.m_x = this.m_parent.GetMidX() - gameObject.GetMidX();
                        this.m_rangePosition.m_y = this.m_parent.GetMidY() - gameObject.GetMidY();

                        int lengthSquared = this.m_rangePosition.GetLengthSquared();

                        if (lengthSquared < minDistance)
                        {
                            minDistance = lengthSquared;
                            closestTarget = gameObject;
                        }
                    }
                }
            }

            return closestTarget;
        }

        public void SelectTarget(LogicGameObject gameObject, int idx)
        {
            if (LogicDataTables.GetGlobals().ClearAlertStateIfNoTargetFound())
            {
                if (idx == 0 && this.m_alertTime > 0)
                {
                    if (gameObject == null || gameObject.GetGameObjectType() != LogicGameObjectType.CHARACTER)
                    {
                        this.m_alertTime = 0;
                    }
                }
            }

            this.m_targets[idx] = gameObject;
            this.m_originalTarget = null;
            this.m_damageTime = 0;

            if (this.m_unk502)
            {
                this.m_damageLevelEffect = 0;
            }

            if (this.m_targets[idx] != null)
            {
                this.m_parent.IsHero();
            }

            this.m_forgetTargetTime[idx] = LogicDataTables.GetGlobals().GetForgetTargetTime();
        }

        public bool IsPreferredTargetForMe(LogicGameObject gameObject, int distance)
        {
            if (this.m_preferredTarget != null)
            {
                return LogicCombatComponent.IsPreferredTarget(this.m_preferredTarget, gameObject);
            }

            if (this.m_alertTime <= 0 || gameObject.GetGameObjectType() != LogicGameObjectType.CHARACTER)
            {
                return false;
            }

            int charVsCharRadius = !LogicDataTables.GetGlobals().MorePreciseTargetSelection() ||
                                   LogicDataTables.GetGlobals().MovingUnitsUseSimpleSelect() && this.m_parent.GetMovementComponent() != null
                ? LogicDataTables.GetGlobals().GetCharVersusCharRadiusForAttacker() / 512
                : LogicDataTables.GetGlobals().GetCharVersusCharRadiusForAttacker();

            LogicMovementComponent movementComponent = gameObject.GetMovementComponent();

            if (movementComponent != null)
            {
                if (charVsCharRadius * charVsCharRadius > distance)
                {
                    return !movementComponent.GetPatrolEnabled();
                }
            }

            return false;
        }

        public bool GetTrackAirTargets(int layout, bool draft)
        {
            return this.m_attackerData.GetTrackAirTargets(draft ? this.m_draftUseAltAttackMode[layout] : this.m_useAltAttackMode[layout]);
        }

        public bool GetTrackGroundTargets(int layout, bool draft)
        {
            return this.m_attackerData.GetTrackGroundTargets(draft ? this.m_draftUseAltAttackMode[layout] : this.m_useAltAttackMode[layout]);
        }

        public int GetTotalTargets()
        {
            return this.m_totalTargets;
        }

        public int GetHitCount()
        {
            return this.m_hitCount;
        }

        public bool GetAttackMultipleBuildings()
        {
            return this.m_attackMultipleBuildings;
        }

        public int GetMaxAmmo()
        {
            return this.m_attackerData.GetAmmoCount();
        }

        public void SetUndergroundTime(int ms)
        {
            this.m_undergroundTime = ms;
        }

        public int GetUndergroundTime()
        {
            return this.m_undergroundTime;
        }

        public int GetAmmoCount()
        {
            return this.m_ammo;
        }

        public bool UseAmmo()
        {
            return this.m_attackerData.GetAmmoCount() > 0;
        }

        public void LoadAmmo()
        {
            int maxAmmo = this.m_attackerData.GetAmmoCount();

            if (maxAmmo > 0)
            {
                this.m_ammo = maxAmmo;
            }
        }

        public void RemoveAmmo()
        {
            this.m_ammo = 0;
        }

        public void ToggleAttackMode(int layout, bool draft)
        {
            if (this.m_hasAltAttackMode)
            {
                bool[] array = this.m_useAltAttackMode;

                if (draft)
                {
                    array = this.m_draftUseAltAttackMode;
                }

                array[layout] ^= true;
            }
        }

        public void ToggleAimAngle(int count, int layout, bool draft)
        {
            if (this.m_attackerData.GetTargetingConeAngle() != 0)
            {
                int[] array = this.m_aimAngle;

                if (draft)
                {
                    array = this.m_draftAimAngle;
                }

                int angle = array[layout] + count;

                if (angle >= 360)
                {
                    angle -= 360;
                }

                if (angle < 0)
                {
                    angle += 360;
                }

                array[layout] = angle;
            }
        }

        public int GetAimAngle(int layout, bool draft)
        {
            if (this.m_attackerData.GetTargetingConeAngle() != 0)
            {
                if (draft)
                {
                    return this.m_draftAimAngle[layout];
                }

                return this.m_aimAngle[layout];
            }

            return 0;
        }

        public void ForceNewTarget()
        {
            if (this.m_preferredTarget == null || this.m_preferredTarget.GetDataType() != LogicDataType.BUILDING_CLASS ||
                !((LogicBuildingClassData) this.m_preferredTarget).IsWall())
            {
                this.m_forceNewTarget[0] = true;
            }
        }

        public void UpdateSelectedTargetGroup()
        {
            LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();

            if (movementComponent != null)
            {
                if (this.m_unk660.m_x == -1 && this.m_unk660.m_y == -1)
                {
                    this.m_unk660.m_x = this.m_parent.GetX();
                    this.m_unk660.m_y = this.m_parent.GetY();
                }

                long totalWeight = 0;
                long totalWeightX = 0;
                long totalWeightY = 0;

                for (int i = 0; i < this.m_targetGroups.Size(); i++)
                {
                    LogicGameObject gameObject = this.m_targetGroups[i];
                    int groupWeight;

                    if (this.m_targetGroups[i].GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) gameObject;

                        groupWeight = this.m_attackWithGroups
                            ? character.GetCharacterData().GetFriendlyGroupWeight()
                            : character.GetCharacterData().GetEnemyGroupWeight();
                    }
                    else
                    {
                        groupWeight = 1;
                    }
                    
                    totalWeightX += groupWeight * gameObject.GetMidX();
                    totalWeightY += groupWeight * gameObject.GetMidY();
                    totalWeight += groupWeight;
                }

                if (totalWeight > 0)
                {
                    LogicVector2 pos1 = new LogicVector2(this.m_unk604.m_x, this.m_unk604.m_y);

                    int posX = (int) (totalWeightX / totalWeight);
                    int posY = (int) (totalWeightY / totalWeight);
                    
                    this.m_unk604.Set(posX, posY);
                    this.m_targetGroupPosition.Set(posX, posY);

                    LogicVector2 pos2 = new LogicVector2(this.m_unk660.m_x - this.m_unk604.m_x, this.m_unk660.m_y - this.m_unk604.m_y);

                    pos2.Normalize(3072);

                    this.m_targetGroupPosition.Add(pos2);
                    this.m_targetGroupPosition.Set(
                        (this.m_targetGroupPosition.m_x + this.m_parent.GetMidX()) / 2,
                        (this.m_targetGroupPosition.m_y + this.m_parent.GetMidY()) / 2);

                    if (this.m_unk596)
                    {
                        totalWeightX = 0;
                        totalWeightY = 0;
                        totalWeight = 0;

                        for (int i = 0; i < this.m_targetGroups.Size(); i++)
                        {
                            LogicGameObject gameObject = this.m_targetGroups[i];
                            int groupWeight;

                            if (this.m_targetGroups[i].GetGameObjectType() == LogicGameObjectType.CHARACTER)
                            {
                                LogicCharacter character = (LogicCharacter) gameObject;

                                groupWeight = this.m_attackWithGroups
                                    ? character.GetCharacterData().GetFriendlyGroupWeight()
                                    : character.GetCharacterData().GetEnemyGroupWeight();
                            }
                            else
                            {
                                groupWeight = 1;
                            }

                            LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                            if (combatComponent != null)
                            {
                                LogicGameObject target = combatComponent.m_targets[0];

                                if (target != null)
                                {
                                    pos2.Set(target.GetMidX() - gameObject.GetMidX(), target.GetMidY() - gameObject.GetMidY());
                                    pos2.Normalize(512);

                                    totalWeightX += groupWeight * pos2.m_x;
                                    totalWeightY += groupWeight * pos2.m_y;
                                    totalWeight += groupWeight;
                                }
                            }
                        }

                        if (totalWeight > 0)
                        {
                            posX = (int) (totalWeightX / totalWeight);
                            posY = (int) (totalWeightY / totalWeight);
                            pos2.Set(posX, posY);
                            pos2.Normalize(512);

                            this.m_unk604.Substract(pos2);
                        }
                    }
                }
            }
            else
            {
                int attackSpeed = this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()]
                    ? this.m_attackerData.GetAltAttackSpeed()
                    : this.m_attackerData.GetAttackSpeed();

                if (this.m_hitTime[0] <= attackSpeed)
                {
                    int midX = this.m_parent.GetMidX();
                    int midY = this.m_parent.GetMidY();

                    LogicGameObject selectTarget = null;

                    for (int i = 0, selectTargetWeight = 0, selectTargetDistance = 0; i < this.m_targetGroups.Size(); i++)
                    {
                        LogicGameObject gameObject = this.m_targetGroups[i];

                        if (this.m_targetGroups[i].GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            LogicCharacter character = (LogicCharacter) gameObject;

                            int groupWeight = this.m_attackWithGroups
                                ? character.GetCharacterData().GetFriendlyGroupWeight()
                                : character.GetCharacterData().GetEnemyGroupWeight();
                            int distanceX = character.GetMidX() - midX;
                            int distanceY = character.GetMidY() - midY;
                            int distanceSquared = distanceX * distanceX + distanceY * distanceY;

                            if (groupWeight <= selectTargetWeight)
                            {
                                if (distanceSquared < selectTargetDistance && groupWeight == selectTargetWeight)
                                {
                                    selectTargetDistance = distanceSquared;
                                    selectTarget = character;
                                }
                            }
                            else
                            {
                                selectTarget = character;
                                selectTargetDistance = distanceSquared;
                                selectTargetWeight = groupWeight;
                            }
                        }
                    }

                    if (selectTarget != null)
                    {
                        this.m_targetGroup = selectTarget;

                        int posX = selectTarget.GetX() + (selectTarget.GetWidthInTiles() << 8);
                        int posY = selectTarget.GetY() + (selectTarget.GetWidthInTiles() << 8);
                        
                        this.m_unk604.Set(posX, posY);
                        this.m_targetGroupPosition.Set(posX, posY);
                    }
                }
                else
                {
                    if (this.m_targetGroup != null)
                    {
                        this.m_unk604.m_x = this.m_targetGroup.GetX() + (this.m_targetGroup.GetWidthInTiles() << 8);
                        this.m_unk604.m_y = this.m_targetGroup.GetY() + (this.m_targetGroup.GetWidthInTiles() << 8);
                    }
                }
            }
        }

        public bool GetAttackFinished()
        {
            return this.m_readyForAttack;
        }

        public void StopAttack()
        {
            for (int i = 0; i < this.m_targetCount; i++)
            {
                this.m_hitTime[i] = 0;

                if (this.m_burstTime[i] != 0)
                {
                    this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                }

                this.m_burstTime[i] = 0;
            }

            this.m_readyForAttack = false;
            this.m_unk497 = false;
        }

        public void RefreshTarget(bool destructTarget)
        {
            if (LogicDataTables.GetGlobals().UseStickToClosestUnitHealer())
            {
                if (this.m_damage > 0)
                {
                    this.m_enemyFilter.PassEnemyOnly(this.m_parent);
                    goto REFRESH;
                }

                if (this.m_attackerData.GetShockwavePushStrength() == 0 && this.m_attackerData.GetDamage2() <= 0)
                {
                    if (this.m_targets[0] != null)
                    {
                        if (this.IsTargetValid(this.m_targets[0]))
                        {
                            return;
                        }
                    }
                }
            }

            if (this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.GetDamage2() > 0)
            {
                this.m_enemyFilter.PassEnemyOnly(this.m_parent);
            }
            else
            {
                this.m_enemyFilter.PassFriendlyOnly(this.m_parent);
            }

            REFRESH:

            if (this.m_unk596)
            {
                this.RefreshTargetGroups(this.m_groupNoDamageMod);
            }
            else
            {
                if (!this.m_attackerData.GetTargetGroups())
                {
                    for (int i = 0; i < this.m_targetCount; i++)
                    {
                        if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            LogicCharacter character = (LogicCharacter) this.m_parent;

                            if (character.GetSpawnDelay() > 0 || character.GetSpawnIdleTime() > 0)
                            {
                                this.m_targets[i] = null;
                                this.m_originalTarget = null;

                                continue;
                            }
                        }

                        bool damageUnit = false;
                        bool notPassablePosition = false;

                        if (LogicDataTables.GetGlobals().UseStickToClosestUnitHealer())
                        {
                            if (this.m_targets[i] != null && this.m_forgetTargetTime[i] == 0 && this.m_parent.GetMovementComponent() != null && !this.IsWallBreaker())
                            {
                                damageUnit = this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.GetDamage2() > 0;
                            }
                        }
                        else
                        {
                            if (this.m_targets[i] != null && this.m_forgetTargetTime[i] == 0 && this.m_parent.GetMovementComponent() != null && !this.IsWallBreaker())
                            {
                                damageUnit = true;
                            }
                        }

                        LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();

                        if (movementComponent != null)
                        {
                            notPassablePosition = movementComponent.IsInNotPassablePosition();
                        }

                        LogicGameObject originalTarget = this.m_originalTarget ?? this.m_targets[i];

                        if (!notPassablePosition && this.m_attackWithGroups)
                        {
                            if (this.m_targets[i] != null && this.m_originalForgetTargetTime == 0 && this.m_hitTime[i] <= 64)
                            {
                                this.RefreshTarget_Internal1(i, true);
                            }
                            else
                            {
                                if (this.m_targetGroups.Size() != 0)
                                {
                                    if (this.m_hitTime[i] <= 64)
                                    {
                                        bool isBreak = false;

                                        for (int j = 0; j < this.m_targetGroups.Size(); j++)
                                        {
                                            LogicCombatComponent combatComponent = this.m_targetGroups[j].GetCombatComponent();

                                            if (combatComponent != null)
                                            {
                                                if (combatComponent.m_targets[0] == this.m_targets[i] ||
                                                    combatComponent.m_originalTarget == this.m_targets[i])
                                                {
                                                    isBreak = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (!isBreak)
                                            damageUnit |= this.m_hitTime[i] <= 64;
                                    }
                                }
                            }
                        }
                        
                        if (damageUnit || destructTarget)
                        {
                            this.RefreshTarget_Internal1(i, true);
                        }

                        if (this.m_forceNewTarget[i])
                        {
                            this.RefreshTarget_Internal1(i, false);
                        }

                        if (this.m_targets[i] != null)
                        {
                            bool unk3 = true;

                            if (this.IsTargetValid(this.m_targets[i]))
                            {
                                if (this.m_targets[i].IsStealthy())
                                {
                                    if (this.m_damage <= 0 && this.m_attackerData.GetShockwavePushStrength() <= 0 && this.m_attackerData.GetDamage2() <= 0)
                                    {
                                        unk3 = false;
                                    }
                                }
                                else
                                {
                                    unk3 = false;
                                }
                            }

                            bool originalTargetNotValid = this.m_originalTarget != null && !this.IsTargetValid(this.m_originalTarget);

                            if (originalTargetNotValid || unk3)
                            {
                                // Listener

                                this.m_targets[i] = null;
                                this.m_originalTarget = null;

                                if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                                {
                                    LogicCharacter character = (LogicCharacter) this.m_parent;
                                    LogicCharacterData data = character.GetCharacterData();

                                    if (data.IsUnderground())
                                    {
                                        this.m_hideTime[i] = LogicDataTables.GetGlobals().GetMinerHideTime() +
                                                             this.m_parent.Rand(0) % LogicDataTables.GetGlobals().GetMinerHideTimeRandom();
                                    }

                                    if (this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.GetDamage2() > 0)
                                    {
                                        if (!character.IsHero())
                                        {
                                            this.m_attackDelay[i] = LogicMath.Abs(this.m_parent.Rand(0)) % 800 + 200;
                                        }
                                    }
                                }
                            }
                        }

                        if (!LogicDataTables.GetGlobals().UseStickToClosestUnitHealer())
                        {
                            if (this.m_hideTime[i] != 0)
                            {
                                continue;
                            }

                            if (this.m_targets[i] != null)
                            {
                                if (this.m_damage > 0 || this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.GetDamage2() > 0)
                                {
                                    continue;
                                }
                            }
                        }
                        else if (this.m_hideTime[i] != 0 || this.m_targets[i] != null)
                        {
                            continue;
                        }
                        
                        if (this.m_attackDelay[i] != 0)
                        {
                            if (!this.m_troopChild || notPassablePosition)
                            {
                                continue;
                            }
                        }
                        else if (notPassablePosition)
                        {
                            continue;
                        }

                        if (LogicDataTables.GetGlobals().RestartAttackTimerOnAreaDamageTurrets() || this.m_attackerData.GetDamageRadius() == 0 || movementComponent != null)
                        {
                            this.m_hitTime[i] = 0;

                            if (this.m_burstTime[i] != 0)
                            {
                                this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                            }

                            this.m_burstTime[i] = 0;
                        }

                        this.m_originalTarget = null;
                        this.SearchTarget(i, originalTarget);

                        if (this.m_targets[i] != null)
                        {
                            if (this.IsTargetValid(this.m_targets[i]))
                            {
                                if (movementComponent != null)
                                {
                                    movementComponent.NewTargetFound();
                                    // Listener
                                }

                                this.m_totalTargets += 1;
                            }
                            else
                            {
                                this.m_targets[i] = null;
                            }
                        }

                        if (movementComponent != null && this.m_targets[0] == null)
                        {
                            movementComponent.NoTargetFound();
                        }

                        if (this.m_targets[0] == null)
                        {
                            this.m_hitTime[i] = 0;

                            if (this.m_burstTime[i] != 0)
                            {
                                this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                            }

                            this.m_burstTime[i] = 0;
                        }

                        this.m_attackDelay[i] = 500;
                    }
                }
                else
                {
                    if (this.m_burstTime[0] == 0 && (this.m_targetGroups.Size() <= 0 && this.m_attackCooldownOverride[0] <= 0 && this.m_attackDelay[0] == 0 || this.m_forceNewTarget[0]))
                    {
                        this.RefreshTargetGroups(this.m_damage <= 0 && this.m_attackerData.GetShockwavePushStrength() == 0 && this.m_attackerData.GetDamage2() <= 0);

                        this.m_forceNewTarget[0] = false;
                        this.m_attackDelay[0] = 500;
                        this.m_hitTime[0] = 0;
                        this.m_readyForAttack = false;

                        if (this.m_burstTime[0] != 0)
                        {
                            this.m_attackCooldownOverride[0] = this.m_attackerData.GetAttackCooldownOverride();
                        }

                        this.m_burstTime[0] = 0;
                    }
                }
            }
        }

        private void RefreshTarget_Internal1(int i, bool forceNewTarget)
        {
            if (forceNewTarget)
            {
                this.m_forceNewTarget[i] = true;
            }

            // Listener.

            this.m_forceNewTarget[i] = false;
            this.m_targets[i] = null;
            this.m_originalTarget = null;
            this.m_targetGroup = null;

            if (forceNewTarget)
            {
                this.m_attackDelay[i] = 0;
            }
            else
            {
                this.m_attackDelay[i] = LogicMath.Abs(this.m_parent.Rand(0));
                this.m_attackDelay[i] = this.m_attackDelay[i] % 800;
            }
        }

        public LogicVector2 GetTargetGroupPosition()
        {
            if (this.m_targetGroups.Size() > 0 || this.m_burstTime[0] > 0)
                return this.m_targetGroupPosition;
            return null;
        }

        public bool GetUnk596()
        {
            return this.m_unk596;
        }

        public bool RefreshTargetGroups(bool noDamage)
        {
            int radius = this.m_unk596 || this.m_attackWithGroups ? this.m_targetGroupsRadius : this.m_attackerData.GetTargetGroupsRadius();
            int groupTileCount = 2 * radius / 5;

            if (groupTileCount <= 0)
                groupTileCount = 1;

            int subGroupCount = 2 * radius / groupTileCount;
            int groupCount = 25600 / groupTileCount;

            if (25600u % groupTileCount > groupTileCount / 3u)
                groupCount += 1;

            int groupCountSquared = groupCount * groupCount;

            this.m_targetGroupWeights.Clear();
            this.m_targetGroupWeights.EnsureCapacity(groupCountSquared);

            for (int i = 0; i < groupCountSquared; i++)
            {
                this.m_targetGroupWeights.Add(0);
            }

            this.m_targetGroups.Clear();
            this.m_targetGroups.EnsureCapacity(20);

            if (noDamage)
                this.m_groupEnemyFilter.PassFriendlyOnly(this.m_parent);
            else
                this.m_groupEnemyFilter.PassEnemyOnly(this.m_parent);

            this.m_targetGroupEnemyList.Clear();
            this.m_parent.GetGameObjectManager().GetGameObjects(this.m_targetGroupEnemyList, this.m_groupEnemyFilter);

            int maxGroupWeight = 0;
            int targetGroupsRangeSquared = this.m_attackerData.GetTargetGroupsRange() * this.m_attackerData.GetTargetGroupsRange();

            if (this.m_targetGroupEnemyList.Size() == 0)
                return false;

            for (int i = 0; i < this.m_targetGroupEnemyList.Size(); i++)
            {
                LogicGameObject gameObject = this.m_targetGroupEnemyList[i];

                if (gameObject == this.m_parent || gameObject.IsHidden() || (gameObject.IsStealthy() && !noDamage) || !gameObject.IsAlive() || !this.CanAttackHeightCheck(gameObject))
                    continue;

                if (noDamage)
                {
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                    if (combatComponent != null && combatComponent.m_damage <= 0 && combatComponent.m_attackerData.GetShockwavePushStrength() == 0 &&
                        combatComponent.m_attackerData.GetDamage2() <= 0)
                        continue;
                }

                int groupWeight = 1;

                if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) gameObject;

                    if (character.GetSpawnDelay() > 0 || character.GetSpawnIdleTime() > 0 || character.GetChildTroops() != null)
                        continue;

                    groupWeight = this.m_attackWithGroups
                        ? character.GetCharacterData().GetFriendlyGroupWeight()
                        : character.GetCharacterData().GetEnemyGroupWeight();
                }

                if (this.m_parent.GetMovementComponent() != null || this.IsInRange(gameObject))
                {
                    int midX = gameObject.GetMidX();
                    int midY = gameObject.GetMidY();

                    if ((midX | midY) >= 0)
                    {
                        if (targetGroupsRangeSquared != 0)
                        {
                            int parentMidX = this.m_parent.GetMidX();
                            int parentMidY = this.m_parent.GetMidY();
                            int distanceX = parentMidX - midX;
                            int distanceY = parentMidY - midY;

                            if (distanceX * distanceX + distanceY * distanceY > targetGroupsRangeSquared)
                                continue;
                        }

                        int groupX = midX / groupTileCount;
                        int groupY = midY / groupTileCount;

                        if (groupX < groupCount && groupY < groupCount)
                        {
                            int offset = groupX + groupCount * groupY;
                            int weight = this.m_targetGroupWeights[offset] + groupWeight;

                            this.m_targetGroupWeights[offset] = weight;

                            if (weight > maxGroupWeight)
                            {
                                maxGroupWeight = weight;
                            }
                        }
                    }
                }
            }

            if (maxGroupWeight == 0)
                return false;

            int tmp1 = subGroupCount * groupTileCount / -2;
            int tmp2 = tmp1 + (groupTileCount >> 1);

            if (this.m_targetGroupWeightMultiplier.Size() == 0)
            {
                this.m_targetGroupWeightMultiplier.EnsureCapacity(subGroupCount * subGroupCount);
                
                LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();

                if (movementComponent != null)
                {
                    for (int x = 0, subX = 0; x < subGroupCount; x++)
                    {
                        int offset = tmp2 + groupTileCount * x;
                        int offsetSquared = offset * offset;

                        for (int y = 0, subY = tmp2; y < subGroupCount; y++)
                        {
                            int sqrt = LogicMath.Sqrt(offsetSquared + subY * subY);
                            int clamp = LogicMath.Clamp(radius - sqrt, -(groupTileCount >> 1), groupTileCount >> 1);

                            this.m_targetGroupWeightMultiplier.Add(subX + y, 100 * ((groupTileCount >> 1) + clamp) / (groupTileCount & 0x7FFFFFFE));

                            subY += groupTileCount;
                        }

                        subX += subGroupCount;
                    }
                }
                else
                {
                    for (int i = 0, subX = 0; i < subGroupCount; i++)
                    {
                        int tmp = tmp2 + i * groupTileCount;
                        int offset = tmp;

                        if (tmp != 0)
                            offset = tmp - (groupTileCount >> 1);
                        if (tmp < 0)
                            offset = tmp + (groupTileCount >> 1);

                        int offsetSquared = offset * offset;

                        for (int j = 0, subY = tmp1; j < subGroupCount; j++)
                        {
                            int offset2 = subY + (groupTileCount >> 1);

                            if (offset2 < 0)
                                offset2 = 2 * (groupTileCount >> 1) + subY;
                            else if (offset2 != 0)
                                offset2 = subY;
                            
                            int value = radius - LogicMath.Sqrt(offsetSquared + offset2 * offset2);

                            if (value < 0)
                                value = 0;
                            
                            this.m_targetGroupWeightMultiplier.Add(subX + j, 100 * value / radius);

                            subY += groupTileCount;
                        }

                        subX += subGroupCount;
                    }
                }
            }
            
            int bestGroupX = 0;
            int bestGroupY = 0;
            int count = groupCount - subGroupCount;

            long bestGroupWeightMultiplied = 0;

            for (int i = 0; i <= count; i++)
            {
                int offset = tmp1 - i * groupTileCount;

                for (int j = 0; j <= count; j++)
                {
                    long groupWeight = 0;
                    long groupWeightMultiplied = 0;
                    
                    for (int k = 0, subX = 0; k < subGroupCount; k++)
                    {
                        int offset2 = j + (i + k) * groupCount;

                        for (int l = 0; l < subGroupCount; l++)
                        {
                            int weight = this.m_targetGroupWeights[offset2 + l];
                            int multiplier = this.m_targetGroupWeightMultiplier[subX + l];
                            
                            groupWeightMultiplied += weight * multiplier;
                            groupWeight += weight;
                        }

                        subX += subGroupCount;
                    }

                    if (groupWeight >= this.m_attackerData.GetTargetGroupsMinWeight())
                    {
                        if (bestGroupWeightMultiplied < groupWeightMultiplied * 1000)
                        {
                            int midX = this.m_parent.GetMidX();
                            int midY = this.m_parent.GetMidY();
                            int offset2 = tmp1 - j * groupTileCount;
                            
                            int sqrt = LogicMath.Sqrt((offset2 + midX) * (offset2 + midX) + (offset + midY) * (offset + midY)) >> 8;
                            int sqrt20000 = LogicMath.Sqrt(20000);
                            uint multiplier = (uint) (1000 * (sqrt20000 - sqrt) / sqrt20000 * (1000 * (sqrt20000 - sqrt) / sqrt20000) / 1000u);

                            groupWeightMultiplied *= multiplier;

                            if (groupWeightMultiplied < 1)
                                groupWeightMultiplied = 1;
                        }
                        
                        if (bestGroupWeightMultiplied < groupWeightMultiplied)
                        {
                            bestGroupWeightMultiplied = groupWeightMultiplied;
                            bestGroupX = j;
                            bestGroupY = i;
                        }
                    }
                }
            }

            int minX = bestGroupX * groupTileCount;
            int maxX = bestGroupX * groupTileCount + subGroupCount * groupTileCount;
            int minY = bestGroupY * groupTileCount;
            int maxY = bestGroupY * groupTileCount + subGroupCount * groupTileCount;

            int totalWeight = 0;

            for (int i = 0; i < this.m_targetGroupEnemyList.Size(); i++)
            {
                LogicGameObject gameObject = this.m_targetGroupEnemyList[i];

                if (gameObject == this.m_parent || gameObject.IsHidden() || (gameObject.IsStealthy() && !noDamage) || !gameObject.IsAlive() || !this.CanAttackHeightCheck(gameObject))
                    continue;

                if (noDamage)
                {
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                    if (combatComponent != null && combatComponent.m_damage <= 0 && combatComponent.m_attackerData.GetShockwavePushStrength() <= 0 &&
                        combatComponent.m_attackerData.GetDamage2() <= 0)
                        continue;
                }

                int groupWeight = 1;

                if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) gameObject;

                    if (character.GetSpawnDelay() > 0 || character.GetSpawnIdleTime() > 0 || character.GetChildTroops() != null)
                        continue;

                    groupWeight = this.m_attackWithGroups
                        ? character.GetCharacterData().GetFriendlyGroupWeight()
                        : character.GetCharacterData().GetEnemyGroupWeight();
                }

                int midX = gameObject.GetMidX();
                int midY = gameObject.GetMidY();

                if (midX >= minX && midX <= maxX && midY >= minY && midY <= maxY)
                {
                    if (this.m_parent.GetMovementComponent() != null || this.IsInRange(gameObject))
                    {
                        this.m_targetGroups.Add(gameObject);
                        totalWeight += groupWeight;
                    }
                }
            }

            if (totalWeight < this.m_attackerData.GetTargetGroupsMinWeight())
            {
                this.m_targetGroups.Clear();
                return false;
            }
            
            this.UpdateSelectedTargetGroup();
            return this.m_targetGroups.Size() != 0;
        }
        
        public LogicGameObject SelectWall()
        {
            if (this.m_parent.GetHitpointComponent().GetTeam() != 1)
            {
                LogicArrayList<LogicGameObject> buildings = this.m_parent.GetGameObjectManager().GetGameObjects(LogicGameObjectType.BUILDING);
                LogicGameObject wall = this.GetBestWallToBreak(buildings);

                if (wall == null)
                {
                    LogicGameObject closestWall = null;

                    for (int i = 0, minDistance = 0; i < buildings.Size(); i++)
                    {
                        LogicBuilding building = (LogicBuilding) buildings[i];

                        if (building.IsWall())
                        {
                            this.m_rangePosition.m_x = building.GetMidX() - this.m_parent.GetMidX();
                            this.m_rangePosition.m_y = building.GetMidY() - this.m_parent.GetMidY();

                            int lengthSquared = this.m_rangePosition.GetLengthSquared();

                            if ((closestWall == null || lengthSquared < minDistance) && building.IsConnectedWall())
                            {
                                minDistance = lengthSquared;
                                closestWall = building;
                            }
                        }
                    }

                    wall = closestWall;
                }

                if (wall != null)
                {
                    if (wall.IsAlive())
                    {
                        return wall;
                    }

                    int wallTileX = wall.GetTileX();
                    int wallTileY = wall.GetTileY();

                    int distanceX = wallTileX - this.m_parent.GetTileX();
                    int distanceY = wallTileY - this.m_parent.GetTileY();

                    if ((distanceX | distanceY) != 0)
                    {
                        this.m_rangePosition.m_x = distanceX;
                        this.m_rangePosition.m_y = distanceY;

                        this.m_rangePosition.Normalize(10);

                        return this.FindNextWallInLine(wallTileX, wallTileY, wallTileX + this.m_rangePosition.m_x, wallTileY + this.m_rangePosition.m_y);
                    }
                }
            }

            return null;
        }

        public bool IsWall(LogicGameObject gameObject)
        {
            if (gameObject != null)
            {
                LogicGameObjectData data = gameObject.GetData();

                if (data.GetDataType() == LogicDataType.BUILDING)
                {
                    return ((LogicBuildingData) data).IsWall();
                }
            }

            return false;
        }

        public bool IsHealer()
        {
            return this.m_damage >> 31 != 0;
        }

        public bool CanAttackHeightCheck(LogicGameObject gameObject)
        {
            int layout = this.m_parent.GetLevel().GetCurrentLayout();
            bool flying = gameObject.IsFlying();

            if (!this.m_useAltAttackMode[layout] || !this.m_altMultiTargets)
            {
                if (this.GetTrackAirTargets(layout, false) || !flying)
                {
                    return flying || this.GetTrackGroundTargets(layout, false);
                }

                return false;
            }

            return true;
        }

        public bool IsAlerted()
        {
            return this.m_alertTime > 0;
        }

        public void StartAllianceAlert(LogicGameObject gameObject, LogicGameObject target)
        {
            if (this.m_preferredTarget == null)
            {
                if (!LogicDataTables.GetGlobals().UseStickToClosestUnitHealer() ||
                    this.m_damage > 0 ||
                    this.m_attackerData.GetShockwavePushStrength() != 0 ||
                    this.m_attackerData.GetDamage2() > 0)
                {
                    if ((this.m_parent == target || this.m_alertTime <= 0) && (!LogicDataTables.GetGlobals().IgnoreAllianceAlertForNonValidTargets() ||
                                                                               this.IsTargetValid(gameObject) && this.CanAttackHeightCheck(gameObject)))
                    {
                        for (int i = 0; i < this.m_targetCount; i++)
                        {
                            LogicGameObject tmp = this.m_targets[i];

                            if (tmp != null)
                            {
                                if (tmp.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                                {
                                    return;
                                }
                            }
                        }

                        this.m_alertTime = 1000;
                        this.m_alerted = true;
                    }
                }
            }
        }

        public void ObstacleToDestroy(LogicGameObject obstacle)
        {
            if (this.m_targets[0] == null || !this.IsInRange(this.m_targets[0]) || !this.IsInLine(this.m_targets[0]))
            {
                if (this.IsTargetValid(obstacle))
                {
                    if (LogicDataTables.GetGlobals().RememberOriginalTarget() && this.m_originalTarget == null)
                    {
                        this.m_originalTarget = this.m_targets[0];
                    }

                    this.m_targets[0] = obstacle;

                    if (obstacle.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        ((LogicBuilding) obstacle).StartSelectedWallTime();
                    }
                }
                else
                {
                    this.m_attackDelay[0] = 2500;
                    this.m_targets[0] = null;
                }
            }
        }

        public LogicGameObject GetBestWallToBreak(LogicArrayList<LogicGameObject> gameObjects)
        {
            if (LogicDataTables.GetGlobals().GetWallBreakerSmartCountLimit() != 0)
            {
                if (this.m_totalTargets < LogicDataTables.GetGlobals().GetWallBreakerSmartRetargetLimit())
                {
                    LogicArrayList<LogicGameObject> characters = this.m_parent.GetGameObjectManager().GetGameObjects(LogicGameObjectType.CHARACTER);

                    int wallBreakerCount = 0;

                    for (int i = 0; i < characters.Size(); i++)
                    {
                        LogicCharacter character = (LogicCharacter) characters[i];
                        LogicCombatComponent combatComponent = character.GetCombatComponent();

                        if (combatComponent != null && combatComponent.m_preferredTarget != null && combatComponent.m_preferredTarget.GetDataType() == LogicDataType.BUILDING_CLASS &&
                            ((LogicBuildingClassData) combatComponent.m_preferredTarget).IsWall())
                        {
                            if (character.IsAlive())
                            {
                                wallBreakerCount += 1;
                            }
                        }
                    }

                    if (wallBreakerCount <= LogicDataTables.GetGlobals().GetWallBreakerSmartCountLimit())
                    {
                        int smartRadius = LogicDataTables.GetGlobals().GetWallBreakerSmartRadius();
                        int smartRadiusSquared = smartRadius * smartRadius;

                        LogicArrayList<int> goLength = new LogicArrayList<int>(50);
                        LogicArrayList<int> goIdx = new LogicArrayList<int>(50);

                        for (int i = 0; i < gameObjects.Size(); i++)
                        {
                            LogicGameObject gameObject = gameObjects[i];

                            if (!gameObject.IsWall() &&
                                !gameObject.IsHidden())
                            {
                                if (gameObject.IsAlive())
                                {
                                    this.m_rangePosition.m_x = gameObject.GetMidX() - this.m_parent.GetMidX();
                                    this.m_rangePosition.m_y = gameObject.GetMidY() - this.m_parent.GetMidY();

                                    int length = this.m_rangePosition.GetLengthSquared();

                                    if (length <= smartRadiusSquared && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                    {
                                        if (goLength.Size() >= 50)
                                        {
                                            goLength.Remove(49);
                                            goIdx.Remove(49);
                                        }

                                        bool added = false;

                                        for (int j = 0; j < goLength.Size(); j++)
                                        {
                                            if (goLength[j] > length)
                                            {
                                                goLength.Add(j, length);
                                                goIdx.Add(j, i);

                                                added = true;

                                                break;
                                            }
                                        }

                                        if (!added)
                                        {
                                            goLength.Add(length);
                                            goIdx.Add(i);
                                        }
                                    }
                                }
                            }
                        }

                        if (LogicDataTables.GetGlobals().WallBreakerUseRooms())
                        {
                            LogicTileMap tileMap = this.m_parent.GetLevel().GetTileMap();

                            int roomIdx = tileMap.GetTile(this.m_parent.GetTileX(), this.m_parent.GetTileY())?.GetRoomIdx() ?? -1;

                            for (int i = 0; i < goIdx.Size(); i++)
                            {
                                LogicGameObject gameObject = gameObjects[goIdx[i]];

                                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                {
                                    int goRoomIdx = tileMap.GetTile(gameObject.GetTileX(), gameObject.GetTileY())?.GetRoomIdx() ?? -1;

                                    if (goRoomIdx != roomIdx)
                                    {
                                        LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();
                                        LogicMovementSystem movementSystem = movementComponent.GetMovementSystem();

                                        movementComponent.MoveTo(gameObject);

                                        LogicGameObject wall = movementSystem.GetWall();

                                        if (wall != null)
                                        {
                                            return wall;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < goIdx.Size(); i++)
                            {
                                LogicGameObject gameObject = gameObjects[goIdx[i]];

                                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                {
                                    LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();
                                    LogicMovementSystem movementSystem = movementComponent.GetMovementSystem();

                                    movementComponent.MoveTo(gameObject);

                                    LogicGameObject wall = movementSystem.GetWall();

                                    if (wall != null)
                                    {
                                        return wall;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public LogicGameObject FindNextWallInLine(int startX, int startY, int endX, int endY)
        {
            int moveStepX = endX > startX ? 1 : -1;
            int moveStepY = endY > startY ? 1 : -1;

            int distanceX = LogicMath.Abs(endX - startX);
            int distanceY = LogicMath.Abs(endY - startY);
            int direction = distanceX - distanceY;

            int subTileDistanceX = distanceX * 2;
            int subTileDistanceY = distanceY * 2;

            LogicTileMap tileMap = this.m_parent.GetLevel().GetTileMap();

            for (int i = distanceX + distanceY, posX = startX, posY = startY; i >= 0; i--)
            {
                LogicTile tile = tileMap.GetTile(posX, posY);

                if (tile == null)
                {
                    break;
                }

                for (int k = 0; k < tile.GetGameObjectCount(); k++)
                {
                    LogicGameObject gameObject = tile.GetGameObject(k);

                    if (gameObject.IsWall() && gameObject.IsAlive())
                    {
                        return gameObject;
                    }
                }

                if (direction > 0)
                {
                    direction -= subTileDistanceY;
                    posX += moveStepX;
                }
                else
                {
                    direction += subTileDistanceX;
                    posY += moveStepY;
                }
            }

            return null;
        }

        public void Boost(int damage, int attackSpeedBoost, int time)
        {
            if (damage < 0)
            {
                this.m_slowDamage = LogicMath.Min(LogicMath.Max(-100, damage), this.m_slowDamage);
                this.m_slowTime = time;
            }
            else
            {
                int idx = this.m_boostDamage[0] != 0 ? 1 : 0;

                this.m_boostDamage[idx] = LogicMath.Max(damage, this.m_boostDamage[idx]);
                this.m_boostTime[idx] = time;
            }

            this.m_attackSpeedBoost = attackSpeedBoost;
            this.m_attackSpeedBoostTime = time;
        }

        public bool IsBoosted()
        {
            return this.m_boostTime[0] > 0;
        }

        public bool IsSlowed()
        {
            return this.m_slowTime > 0;
        }

        public override void SubTick()
        {
            if (this.m_activationTime <= 0)
            {
                if (this.m_attackerData.GetTargetGroups() || this.m_unk596 || this.m_attackWithGroups)
                {
                    LogicVector2 position = new LogicVector2();
                    position.Set(this.m_unk604.m_x, this.m_unk604.m_y);
                    this.UpdateSelectedTargetGroup();

                    if (this.m_targetGroups.Size() > 0)
                    {
                        int targetGroupsRadius = this.m_unk596 || this.m_attackWithGroups ? this.m_targetGroupsRadius : this.m_attackerData.GetTargetGroupsRadius();

                        if (this.m_parent.GetMovementComponent() == null)
                        {
                            LogicVector2 position2 = new LogicVector2();

                            position2.Set(this.m_unk604.m_x, this.m_unk604.m_y);
                            position2.Substract(position);

                            int length = position2.Normalize(30);

                            if (length <= 2 * targetGroupsRadius && length > 30)
                            {
                                this.m_unk604.Set(position.m_x, position.m_y);
                                this.m_unk604.Add(position2);
                            }
                        }

                        // Listener.

                        if (this.m_unk596)
                        {
                            LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();

                            if (movementComponent == null || !movementComponent.IsInNotPassablePosition())
                            {
                                if (movementComponent != null)
                                {
                                    LogicMovementSystem movementSystem = movementComponent.GetMovementSystem();
                                    LogicVector2 pathEndPosition = new LogicVector2(movementSystem.GetPathEndPosition().m_x, movementSystem.GetPathEndPosition().m_y);

                                    int distance = pathEndPosition.GetDistanceSquaredTo(this.m_unk604.m_x, this.m_unk604.m_y);

                                    if (distance > 0x10000)
                                    {
                                        if (this.m_parent.GetLevel().GetTileMap().GetNearestPassablePosition(this.m_unk604.m_x, this.m_unk604.m_y, this.m_unk604, 512))
                                        {
                                            movementComponent.MoveTo(this.m_unk604.m_x, this.m_unk604.m_y);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.m_activationTime -= 16;
            }
        }

        public override void Tick()
        {
            if (this.m_activationTime <= 0)
            {
                if (this.m_altMultiTargets)
                {
                    this.m_targetCount = this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()] ? this.m_attackerData.GetAltNumMultiTargets() : 1;
                }

                if (!this.m_parent.IsAlive() || this.m_parent.IsHidden() || this.m_attackerData.GetAmmoCount() > 0 && this.m_ammo == 0 || this.m_parent.IsFrozen())
                {
                    for (int i = 0; i < this.m_targetCount; i++)
                    {
                        this.m_targets[i] = null;
                        this.m_hitTime[i] = 0;
                        this.m_burstTime[i] = 0;
                        this.m_damage2Time[i] = 0;

                        // Listener.

                        if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            LogicCharacter character = (LogicCharacter) this.m_parent;

                            if (character.GetCharacterData().IsUnderground())
                            {
                                this.m_hideTime[i] = LogicDataTables.GetGlobals().GetMinerHideTime() +
                                                     this.m_parent.Rand(0) % LogicDataTables.GetGlobals().GetMinerHideTimeRandom();
                            }
                        }
                    }

                    this.m_originalTarget = null;
                    this.m_prepareSpeed = this.m_attackerData.GetPrepareSpeed();
                    this.m_targetGroups.Clear();

                    // Listener.

                    this.m_unk604.Set(0, 0);
                    this.m_targetGroupPosition.Set(0, 0);

                    this.m_targetGroup = null;

                    // Listener.
                }
                else
                {
                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.BUILDING && this.m_attackerData.GetWakeUpSpace() > 0)
                    {
                        int prevDeployedHousingSpace = this.m_deployedHousingSpace;
                        this.m_deployedHousingSpace = this.m_parent.GetLevel().GetBattleLog().GetDeployedHousingSpace();

                        if (this.m_deployedHousingSpace >= this.m_attackerData.GetWakeUpSpace())
                        {
                            if (prevDeployedHousingSpace < this.m_attackerData.GetWakeUpSpace())
                            {
                                // Listener.
                            }

                            this.m_wakeUpTime = LogicMath.Max(this.m_wakeUpTime - 64, 0);

                            if (this.m_wakeUpTime > 0)
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    this.RefreshTarget(false);

                    int boostDefenceMS = ((this.m_attackSpeedBoost << 6) + 6400) / 100;

                    this.m_damageTime += 64;
                    this.m_alertTime = LogicMath.Max(this.m_alertTime - 64, 0);
                    this.m_summonCooldownTime = LogicMath.Max(this.m_summonCooldownTime - boostDefenceMS, 0);
                    this.m_originalForgetTargetTime = LogicMath.Max(this.m_originalForgetTargetTime - 64, 0);

                    int targetCount = 0;

                    for (int i = 0; i < this.m_targetCount; i++)
                    {
                        this.m_forgetTargetTime[i] = LogicMath.Max(this.m_forgetTargetTime[i] - 64, 0);
                        this.m_attackDelay[i] = LogicMath.Max(this.m_attackDelay[i] - 64, 0);
                        this.m_attackCooldownOverride[i] = LogicMath.Max(this.m_attackCooldownOverride[i] - boostDefenceMS, 0);
                        this.m_damage2Time[i] = LogicMath.Max(this.m_damage2Time[i] - boostDefenceMS, 0);

                        if (this.m_targets[i] != null)
                        {
                            ++targetCount;
                        }
                    }

                    if (this.m_targetGroups.Size() > 0)
                    {
                        for (int i = 0; i < this.m_targetGroups.Size(); i++)
                        {
                            LogicGameObject gameObject = this.m_targetGroups[i];

                            if (!gameObject.IsAlive() || gameObject.IsHidden() ||
                                (this.m_damage > 0 || this.m_attackerData.GetDamage2() > 0 || this.m_attackerData.GetShockwavePushStrength() != 0) && !this.m_attackWithGroups &&
                                gameObject.IsStealthy() || gameObject.GetMovementComponent() == null && !this.IsInRange(gameObject))
                            {
                                // Listener.
                                this.m_targetGroups.Remove(i--);

                                if (gameObject == this.m_targetGroup)
                                {
                                    this.m_targetGroup = null;
                                }
                            }
                        }

                        if (this.m_targetGroups.Size() == 0)
                        {
                            // Listener.
                        }
                    }

                    if ((targetCount != 0 || this.m_attackerData.GetTargetGroups()) &&
                        (!this.m_attackerData.GetTargetGroups() || this.m_targetGroups.Size() > 0 || this.m_burstTime[0] > 0))
                    {
                        this.m_prepareSpeed = LogicMath.Max(this.m_prepareSpeed - 64, 0);
                    }
                    else
                    {
                        this.m_prepareSpeed = LogicMath.Min(this.m_prepareSpeed + 64, this.m_attackerData.GetPrepareSpeed());
                    }

                    bool attackFinished = this.m_readyForAttack;
                    this.m_readyForAttack = false;

                    if (this.m_attackerData.GetAmmoCount() > 0)
                    {
                        if (this.m_ammoReloadingTime > 0)
                        {
                            --this.m_ammoReloadingTime;
                        }
                    }

                    bool destructTarget = true;

                    for (int i = 0; i < this.m_targetCount; i++)
                    {
                        if (this.m_hideTime[i] > 0)
                        {
                            this.m_hideTime[i] -= 64;

                            if (this.m_hideTime[i] < 0)
                            {
                                this.m_hideTime[i] = 0;
                            }
                        }

                        if (this.m_targets[i] != null)
                        {
                            bool validTarget = false;

                            LogicGameObject target = this.m_targets[i];

                            if (this.IsInRange(target))
                            {
                                validTarget = true;

                                if (this.m_parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                {
                                    validTarget = ((LogicBuilding) this.m_parent).IsValidTarget(target);
                                }
                            }

                            if (this.m_attackMultipleBuildings)
                            {
                                if (!target.IsWall() && target.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                {
                                    LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();

                                    if (movementComponent != null)
                                    {
                                        validTarget = movementComponent.GetMovementSystem().NotMoving() & validTarget;
                                    }
                                }
                            }

                            bool isInLine = false;

                            if (validTarget)
                            {
                                isInLine = this.IsInLine(target);

                                if (!attackFinished && isInLine)
                                {
                                    this.m_hitTime[i] = this.m_attackerData.GetNewTargetAttackDelay();
                                }
                            }

                            if (!isInLine && this.m_hitTime[i] <= 0)
                            {
                                this.m_hitTime[i] = 0;

                                if (this.m_burstTime[i] != 0)
                                {
                                    this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                                }

                                this.m_burstTime[i] = 0;

                                goto NEXT;
                            }

                            this.m_forgetTargetTime[i] = LogicDataTables.GetGlobals().GetForgetTargetTime();
                            this.m_readyForAttack = true;

                            if (this.m_prepareSpeed != 0 || this.m_attackCooldownOverride[i] != 0)
                            {
                                destructTarget = false;
                                goto NEXT;
                            }

                            if (this.m_attackerData.GetPreAttackEffect() != null)
                            {
                                // Listener.
                            }

                            this.m_hitTime[i] += boostDefenceMS;
                            int attackSpeed = this.m_attackerData.GetAttackSpeed();

                            if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                            {
                                attackSpeed = this.m_attackerData.GetAltAttackSpeed();
                            }

                            if (this.m_hitTime[i] < attackSpeed)
                            {
                                destructTarget = false;
                                goto NEXT;
                            }

                            int burstCount = this.m_attackerData.GetBurstCount();
                            int burstDelay = this.m_attackerData.GetBurstDelay();

                            if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                            {
                                burstCount = this.m_attackerData.GetAltBurstCount();
                                burstDelay = this.m_attackerData.GetAltBurstDelay();
                            }

                            if (burstCount <= 0 || burstDelay <= 0)
                            {
                                this.Hit(i);
                            }
                            else
                            {
                                this.m_hitTime[i] = attackSpeed;

                                int prevBurstId = (burstDelay + this.m_burstTime[i] - 1) / burstDelay % (burstCount + 1);
                                this.m_burstTime[i] += boostDefenceMS;
                                int burstId = (burstDelay + this.m_burstTime[i] - 1) / burstDelay % (burstCount + 1);

                                if (burstId <= prevBurstId)
                                {
                                    goto NEXT;
                                }

                                this.Hit(i);

                                if (burstId != burstCount)
                                {
                                    goto NEXT;
                                }
                            }

                            this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                            this.m_hitTime[i] = isInLine ? LogicMath.Min(this.m_hitTime[i] - attackSpeed, attackSpeed) : 0;
                        }
                        else
                        {
                            if (!this.m_attackerData.GetTargetGroups())
                            {
                                this.m_hitTime[i] = 0;

                                if (this.m_burstTime[i] != 0)
                                {
                                    this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                                }

                                this.m_burstTime[i] = 0;
                            }
                            else
                            {
                                if (this.m_targetGroups.Size() > 0 || this.m_burstTime[0] > 0 || this.m_hitTime[i] >= this.m_attackerData.GetAttackSpeed())
                                {
                                    this.m_forgetTargetTime[i] = LogicDataTables.GetGlobals().GetForgetTargetTime();
                                    this.m_readyForAttack = true;

                                    if (this.m_prepareSpeed != 0 || this.m_attackCooldownOverride[i] != 0)
                                    {
                                        goto NEXT;
                                    }

                                    if (this.m_attackerData.GetPreAttackEffect() != null)
                                    {
                                        if (this.m_hitTime[i] < 64)
                                        {
                                            // Listener.
                                        }
                                    }

                                    this.m_hitTime[i] += boostDefenceMS;

                                    if (this.m_hitTime[i] < this.m_attackerData.GetAttackSpeed())
                                    {
                                        destructTarget = false;
                                        goto NEXT;
                                    }

                                    int burstCount = this.m_attackerData.GetBurstCount();
                                    int burstDelay = this.m_attackerData.GetBurstDelay();

                                    if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                                    {
                                        burstCount = this.m_attackerData.GetAltBurstCount();
                                        burstDelay = this.m_attackerData.GetAltBurstDelay();
                                    }

                                    if (burstCount <= 0)
                                    {
                                        this.Hit(i);
                                    }
                                    else
                                    {
                                        int attackerSpeed = this.m_attackerData.GetAttackSpeed();

                                        if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                                        {
                                            attackerSpeed = this.m_attackerData.GetAltAttackSpeed();
                                        }

                                        this.m_hitTime[i] = attackerSpeed;

                                        int prevBurstId = (burstDelay + this.m_burstTime[i] - 1) / burstDelay;
                                        this.m_burstTime[i] += boostDefenceMS;
                                        int burstId = (burstDelay + this.m_burstTime[i] - 1) / burstDelay;

                                        if (burstId <= prevBurstId)
                                        {
                                            goto NEXT;
                                        }

                                        this.Hit(i);

                                        if (burstId != burstCount)
                                        {
                                            goto NEXT;
                                        }
                                    }

                                    this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();

                                    int attackSpeed = this.m_attackerData.GetAttackSpeed();

                                    if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                                    {
                                        attackSpeed = this.m_attackerData.GetAltAttackSpeed();
                                    }

                                    this.m_hitTime[i] = LogicMath.Min(this.m_hitTime[i] - attackSpeed, attackSpeed);
                                    this.m_burstTime[i] = 0;
                                    this.m_targetGroups.Clear();

                                    // Listener.

                                    this.m_unk604.Set(-1, -1);
                                    this.m_targetGroupPosition.Set(-1, -1);

                                    this.m_targetGroup = null;
                                }
                                else
                                {
                                    this.m_hitTime[i] = 0;

                                    if (this.m_burstTime[i] != 0)
                                    {
                                        this.m_attackCooldownOverride[i] = this.m_attackerData.GetAttackCooldownOverride();
                                    }

                                    this.m_burstTime[i] = 0;

                                    this.m_unk604.Set(-1, -1);
                                    this.m_targetGroupPosition.Set(-1, -1);
                                }
                            }
                        }

                        NEXT:

                        if (this.m_damage2Time[i] != 0 || this.m_damage2X[i] < 0)
                        {
                            continue;
                        }

                        if (this.m_attackerData.GetDamage2Radius() > 0)
                        {
                            int targetType = this.GetTrackAirTargets(this.m_parent.GetLevel().GetCurrentLayout(), false)
                                ? this.GetTrackGroundTargets(this.m_parent.GetLevel().GetCurrentLayout(), false) ? 2 : 0
                                : 1;

                            if (this.m_attackerData.GetDamage2Min() <= 0 || this.m_attackerData.GetDamage2Min() == this.m_attackerData.GetDamage2())
                            {
                                this.m_parent.GetLevel().AreaDamage(this.m_parent.GetGlobalID(), this.m_damage2X[i], this.m_damage2Y[i], this.m_attackerData.GetDamage2Radius(),
                                                                    this.m_attackerData.GetDamage2(), this.m_preferredTarget, this.m_preferredTargetDamageMod, null,
                                                                    this.m_parent.GetHitpointComponent().GetTeam(), null, targetType, 0, this.m_attackerData.GetPushBack(), true,
                                                                    false,
                                                                    100, 0, this.m_parent, 100, 0);
                            }
                        }
                    }

                    if (destructTarget)
                    {
                        if (this.m_alerted)
                        {
                            this.m_targets[0] = null;
                            this.m_originalTarget = null;
                            this.m_attackDelay[0] = 0;
                            this.m_alerted = false;
                        }
                    }

                    if (this.m_attackerData.GetSummonTroop() != null && !this.m_attackerData.GetSpawnOnAttack())
                    {
                        int summonCooldown = this.m_attackerData.GetSummonCooldown();

                        if (this.m_summonCooldownTime == 0)
                        {
                            this.m_summonCooldownTime = summonCooldown;
                            this.m_unk497 = false;
                        }

                        if (this.m_summonCooldownTime < summonCooldown / 8 && this.m_targets[0] != null)
                        {
                            int spawnSummonCount = ((LogicCharacter) this.m_parent).GetSummonTroopCount();

                            if (spawnSummonCount >= this.m_attackerData.GetSummonLimit())
                            {
                                this.m_summonCooldownTime = 0;
                            }
                            else
                            {
                                this.m_attackDelay[0] = summonCooldown / 6;
                                this.m_targets[0] = null;
                                this.m_originalTarget = null;
                                this.m_unk497 = true;

                                this.m_parent.SpawnEvent(null, 0, 0);
                            }
                        }
                    }

                    if (this.m_boostTime[0] > 0)
                    {
                        this.m_boostTime[0] -= 1;

                        if (this.m_boostTime[0] == 0)
                        {
                            this.m_boostTime[0] = this.m_boostTime[1];
                            this.m_boostDamage[0] = this.m_boostDamage[1];

                            this.m_boostTime[1] = 0;
                            this.m_boostDamage[1] = 0;
                        }
                    }

                    if (this.m_slowTime > 0)
                    {
                        this.m_slowTime -= 1;

                        if (this.m_slowTime == 0)
                        {
                            this.m_slowDamage = 0;
                        }
                    }

                    if (this.m_attackSpeedBoostTime > 0)
                    {
                        this.m_attackSpeedBoostTime -= 1;

                        if (this.m_attackSpeedBoostTime == 0)
                        {
                            this.m_attackSpeedBoost = 0;
                        }
                    }

                    if (this.m_undergroundTime > 0)
                    {
                        this.m_undergroundTime -= 1;

                        LogicMovementComponent movementComponent = this.m_parent.GetMovementComponent();

                        if (movementComponent != null)
                        {
                            LogicMovementSystem movementSystem = movementComponent.GetMovementSystem();

                            if (movementSystem.NotMoving())
                            {
                                this.m_undergroundTime = 0;
                                movementComponent.CheckTriggers();
                            }
                        }

                        if (this.m_undergroundTime <= 0)
                        {
                            // Listener.
                        }
                    }
                }
            }
        }

        public void AttackedBy(LogicGameObject gameObject)
        {
            if (this.m_parent.GetHitpointComponent().GetTeam() != 1 && this.m_alertTime <= 0 && this.GetComponentType() == LogicComponentType.COMBAT &&
                gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicArrayList<LogicGameObject> characters = this.m_parent.GetGameObjectManager().GetGameObjects(LogicGameObjectType.CHARACTER);

                this.m_rangePosition.m_x = this.m_parent.GetMidX();
                this.m_rangePosition.m_y = this.m_parent.GetMidY();

                int allianceAlertRadiusSquared = LogicDataTables.GetGlobals().GetAllianceAlertRadius() * LogicDataTables.GetGlobals().GetAllianceAlertRadius();

                for (int i = 0; i < characters.Size(); i++)
                {
                    LogicCharacter character = (LogicCharacter) characters[i];

                    if (this.m_rangePosition.GetDistanceSquared(character.GetPosition()) < allianceAlertRadiusSquared)
                    {
                        LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();

                        if (hitpointComponent != null && hitpointComponent.GetTeam() == 0)
                        {
                            LogicCombatComponent combatComponent = character.GetCombatComponent();

                            if (combatComponent != null)
                            {
                                combatComponent.StartAllianceAlert(gameObject, this.m_parent);
                            }
                        }
                    }
                }
            }
        }

        public void Hit(int idx)
        {
            if (this.m_attackerData.GetAmmoCount() > 0 && this.m_ammo <= 0)
            {
                return;
            }

            LogicGameObject target = this.m_targets[idx];

            if (this.m_attackerData.GetTargetGroups() && this.m_parent.GetMovementComponent() == null && this.m_targetGroup != null)
            {
                target = this.m_targetGroup;
            }

            // Listener.

            if (this.m_spawnOnAttack)
            {
                this.m_parent.SpawnEvent(null, 0, 0);
                this.HitCompleted();

                return;
            }

            bool altAttackMode = this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()];
            bool flyingTarget = target != null && target.IsFlying();

            int damage = this.GetDamage();

            if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;

                if (character.GetSpecialAbilityAvailable())
                {
                    LogicCharacterData data = character.GetCharacterData();

                    if (data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_SPECIAL_PROJECTILE)
                    {
                        altAttackMode = true;
                    }

                    if (data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_BIG_FIRST_HIT)
                    {
                        damage = damage * data.GetSpecialAbilityAttribute(character.GetUpgradeLevel()) / 100;
                    }
                }
            }

            // Listener.

            if (this.m_attackerData.GetProjectile(altAttackMode) == null)
            {
                if (damage >= 0 || !this.m_parent.IsPreventsHealing())
                {
                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        LogicVector2 shield = new LogicVector2(target.GetMidX(), target.GetMidY());

                        if (this.m_parent.GetLevel().GetAreaShield(target.GetMidX(), target.GetMidY(), shield))
                        {
                            damage = shield.m_y * damage / 100;
                        }
                    }

                    if (this.m_attackerData.GetDamageRadius() > 0)
                    {
                        target.GetListener().PlayEffect(this.m_attackerData.GetHitEffect2());

                        int midX;
                        int midY;

                        if (this.m_attackerData.IsSelfAsAoeCenter())
                        {
                            midX = this.m_parent.GetMidX();
                            midY = this.m_parent.GetMidY();
                        }
                        else
                        {
                            midX = target.GetMidX();
                            midY = target.GetMidY();
                        }

                        this.m_parent.GetLevel().AreaDamage(this.m_parent.GetGlobalID(), midX, midY, this.m_attackerData.GetDamageRadius(), damage, this.m_preferredTarget,
                                                            this.m_preferredTargetDamageMod, null, this.m_parent.GetHitpointComponent().GetTeam(), null, flyingTarget ? 0 : 1, 0,
                                                            this.m_attackerData.GetPushBack(), true, false, 100, 0, this.m_parent, 100, 0);
                    }
                    else
                    {
                        if (LogicCombatComponent.IsPreferredTarget(this.m_preferredTarget, target))
                        {
                            damage = this.m_preferredTargetDamageMod * damage / 100;
                        }

                        if (damage < 0)
                        {
                            LogicGameObjectData targetData = target.GetData();

                            if (targetData.GetDataType() == LogicDataType.HERO)
                            {
                                damage = LogicDataTables.GetGlobals().GetHeroHealMultiplier() * damage / 100;
                            }
                        }

                        if (this.m_skeletonSpell)
                        {
                            LogicGameObjectData targetData = target.GetData();

                            if (targetData.GetDataType() == LogicDataType.BUILDING)
                            {
                                LogicBuildingData buildingData = (LogicBuildingData) targetData;

                                if (buildingData.GetMaxStoredGold(0) > 0 ||
                                    buildingData.GetMaxStoredElixir(0) > 0 ||
                                    buildingData.GetMaxStoredDarkElixir(0) > 0 ||
                                    buildingData.IsTownHall() ||
                                    buildingData.IsTownHallVillage2())
                                {
                                    damage = LogicDataTables.GetGlobals().GetSkeletonSpellStorageMultipler() * damage / 100;
                                }
                            }
                        }

                        // Listener.

                        LogicHitpointComponent targetHitpointComponent = target.GetHitpointComponent();

                        targetHitpointComponent.CauseDamage(damage, this.m_parent.GetGlobalID(), this.m_parent);

                        if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()] &&
                            this.m_altMultiTargets &&
                            targetHitpointComponent.GetHitpoints() == 0)
                        {
                            this.m_hideTime[idx] = this.m_attackerData.GetAlternatePickNewTargetDelay();
                        }

                        if (targetHitpointComponent.GetHitpoints() == 0 && this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                        {
                            LogicCharacter character = (LogicCharacter) this.m_parent;
                            LogicCharacterData data = character.GetCharacterData();

                            if (data.IsUnderground())
                            {
                                this.m_hideTime[idx] = LogicDataTables.GetGlobals().GetMinerHideTime() +
                                                       this.m_parent.Rand(0) % LogicDataTables.GetGlobals().GetMinerHideTimeRandom();
                            }
                        }

                        if (this.m_attackerData.GetPreventsHealing() && targetHitpointComponent.GetInvulnerabilityTime() <= 0)
                        {
                            target.SetPreventsHealingTime(30);
                        }

                        if (this.m_attackerData.GetChainAttackDistance() > 0)
                        {
                            this.m_targets[1] = null;
                            this.m_targets[2] = null;
                            this.m_targets[3] = null;
                            this.m_targets[4] = null;

                            LogicGameObject chainAttackTarget = this.GetChainAttackTarget(target.GetX(), target.GetY(), this.m_attackerData.GetChainAttackDistance());

                            if (chainAttackTarget != null)
                            {
                                this.m_targets[1] = chainAttackTarget;

                                // Listener.

                                chainAttackTarget.GetHitpointComponent().CauseDamage(damage, this.m_parent.GetGlobalID(), this.m_parent);
                            }
                        }
                    }
                }

                if (this.m_attackerData.GetDamage2() != 0)
                {
                    this.m_damage2Time[idx] = this.m_attackerData.GetDamage2Delay();
                    this.m_damage2X[idx] = target.GetMidX();
                    this.m_damage2Y[idx] = target.GetMidY();
                }
            }
            else
            {
                LogicProjectileData projectileData = this.m_attackerData.GetProjectile(altAttackMode);

                if (this.m_attackerData.GetRageProjectile() != null)
                {
                    if (this.m_parent.IsHero())
                    {
                        if (this.m_parent.IsStealthy())
                        {
                            projectileData = this.m_attackerData.GetRageProjectile();
                        }
                    }
                    else
                    {
                        if (this.m_boostTime[0] > 0)
                        {
                            projectileData = this.m_attackerData.GetRageProjectile();
                        }
                    }
                }

                int burstCount = 1;

                if (this.m_attackerData.GetBurstCount() > 0 && this.m_attackerData.GetBurstDelay() == 0)
                {
                    burstCount = this.m_attackerData.GetBurstCount();
                }

                if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                {
                    burstCount = 1;

                    if (this.m_attackerData.GetAltBurstCount() > 0 && this.m_attackerData.GetAltBurstDelay() == 0)
                    {
                        burstCount = this.m_attackerData.GetAltBurstCount();
                    }
                }

                for (int i = 0; i < burstCount; i++)
                {
                    LogicProjectile projectile =
                        (LogicProjectile) LogicGameObjectFactory.CreateGameObject(projectileData, this.m_parent.GetLevel(), this.m_parent.GetVillageType());

                    projectile.SetInitialPosition(this.m_parent, this.m_parent.GetMidX(), this.m_parent.GetMidY());
                    projectile.SetBounceCount(this.m_attackerData.GetProjectileBounces());

                    if (i >= burstCount - this.m_attackerData.GetDummyProjectileCount())
                    {
                        projectile.SetDummyProjectile(true);
                    }

                    int team = this.m_parent.GetHitpointComponent()?.GetTeam() ?? -1;

                    if (target != null || this.m_targetGroups.Size() <= 0 && this.m_burstTime[0] <= 0)
                    {
                        if (this.m_attackerData.GetShockwavePushStrength() != 0 || this.m_attackerData.IsPenetratingProjectile())
                        {
                            int attackRange = this.GetAttackRange(this.m_parent.GetLevel().GetCurrentLayout(), false) + this.m_attackerData.GetPenetratingExtraRange() + 256;

                            LogicVector2 penetrating = new LogicVector2(attackRange, 0);

                            penetrating.Rotate(this.m_parent.GetDirection());

                            if (this.m_attackerData.GetShockwavePushStrength() != 0)
                            {
                                projectile.SetTargetPos(this.m_parent.GetMidX(), this.m_parent.GetMidY(), this.m_parent.GetMidX() + penetrating.m_x,
                                                        this.m_parent.GetMidY() + penetrating.m_y, this.m_attackerData.GetMinAttackRange(), attackRange + 256,
                                                        this.m_parent.GetDirection(), this.m_attackerData.GetShockwavePushStrength(), this.m_attackerData.GetShockwaveArcLength(),
                                                        this.m_attackerData.GetShockwaveExpandRadius(), team, flyingTarget);
                            }
                            else if (this.m_attackerData.IsPenetratingProjectile())
                            {
                                projectile.SetTargetPos(this.m_parent.GetMidX() + penetrating.m_x, this.m_parent.GetMidY() + penetrating.m_y, team, flyingTarget);
                                projectile.SetPenetratingRadius(this.m_attackerData.GetPenetratingRadius());
                            }
                        }
                        else if (projectileData.GetTrackTarget())
                        {
                            projectile.SetTarget(this.m_parent.GetMidX(), this.m_parent.GetMidY(), this.m_hitCount, target, projectileData.GetRandomHitPosition());
                        }
                        else
                        {
                            int midX = target.GetMidX();
                            int midY = target.GetMidY();

                            if (projectileData.GetTargetPosRandomRadius() != 0)
                            {
                                int randomRadius = projectileData.GetTargetPosRandomRadius();

                                if (this.m_attackerData.GetDummyProjectileCount() > 0 && i == 0)
                                {
                                    randomRadius = 0;
                                }

                                midX += this.m_random.Rand(2 * randomRadius) - randomRadius;
                                midY += this.m_random.Rand(2 * randomRadius) - randomRadius;
                            }

                            projectile.SetTargetPos(midX, midY, team, flyingTarget);
                        }
                    }
                    else
                    {
                        projectile.SetTargetPos(this.m_unk604.m_x, this.m_unk604.m_y, team, flyingTarget);
                    }

                    projectile.SetInitialPosition(this.m_parent, this.m_parent.GetMidX(), this.m_parent.GetMidY());

                    if (target != null)
                    {
                        if (damage < 0 && target.IsPreventsHealing())
                        {
                            damage = 0;
                        }
                    }

                    bool specialProjectile = false;

                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) this.m_parent;
                        LogicCharacterData characterData = character.GetCharacterData();

                        if (characterData.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_SPECIAL_PROJECTILE && character.GetSpecialAbilityAvailable())
                        {
                            damage = damage * characterData.GetSpecialAbilityAttribute3(character.GetUpgradeLevel()) / 100;
                            projectile.SetHitEffect(characterData.GetSpecialAbilityEffect(character.GetUpgradeLevel()), null);
                            specialProjectile = true;
                        }
                    }

                    projectile.SetDamage(damage);
                    projectile.SetPreferredTargetDamageMod(this.m_preferredTarget, this.m_preferredTargetDamageMod);
                    projectile.SetDamageRadius(this.m_attackerData.GetDamageRadius());
                    projectile.SetPushBack(this.m_attackerData.GetPushBack(), true);

                    if (!specialProjectile || projectile.GetHitEffect() == null)
                    {
                        projectile.SetHitEffect(this.m_attackerData.GetHitEffect(), this.m_attackerData.GetHitEffect2());
                    }

                    projectile.SetSpeedMod(this.m_attackerData.GetSpeedMod());
                    projectile.SetStatusEffectTime(this.m_attackerData.GetStatusEffectTime());
                    projectile.SetMyTeam(team);

                    this.m_parent.GetGameObjectManager().AddGameObject(projectile, -1);

                    if (target != null && this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) this.m_parent;
                        LogicCharacterData data = character.GetCharacterData();

                        int chainShootingDistance = data.GetChainShootingDistance();

                        int distanceX = target.GetMidX() - this.m_parent.GetMidX();
                        int distanceY = target.GetMidY() - this.m_parent.GetMidY();

                        int distance = LogicMath.Sqrt(distanceX * distanceX + distanceY * distanceY);

                        if (chainShootingDistance > 0 && distance > 0)
                        {
                            int chainedProjectileBounceCount = LogicDataTables.GetGlobals().GetChainedProjectileBounceCount();
                            int chainShootingDistanceTile = (chainShootingDistance << 9) / 100;

                            if (chainedProjectileBounceCount > 1)
                            {
                                distanceX = 255 * distanceX / distance;
                                distanceY = 255 * distanceY / distance;

                                int offsetX = chainShootingDistanceTile * distanceX / 255;
                                int offsetY = chainShootingDistanceTile * distanceY / 255;

                                int posX = target.GetMidX() + offsetX;
                                int posY = target.GetMidY() + offsetY;

                                for (int j = 0; j < chainedProjectileBounceCount - 1; j++)
                                {
                                    projectile.SetBouncePosition(new LogicVector2(posX, posY));

                                    posX += offsetX;
                                    posY += offsetY;
                                }
                            }
                        }
                    }
                }
            }

            if (this.m_attackerData.GetHitSpell() != null)
            {
                LogicSpell spell =
                    (LogicSpell) LogicGameObjectFactory.CreateGameObject(this.m_attackerData.GetHitSpell(), this.m_parent.GetLevel(), this.m_parent.GetVillageType());

                spell.SetUpgradeLevel(this.m_attackerData.GetHitSpellLevel());
                spell.SetInitialPosition(target.GetMidX(), target.GetMidY());
                spell.SetTeam(this.m_parent.GetHitpointComponent().GetTeam());

                this.m_parent.GetGameObjectManager().AddGameObject(spell, -1);
            }

            if (this.m_attackerData.GetAmmoCount() > 0 && this.m_ammoReloadingTime <= 0)
            {
                this.m_ammo -= 1;
                this.m_ammoReloadingTime = this.m_ammoReloadingTotalTime;

                if (this.m_ammo == 0)
                {
                    // Listener.
                }
            }

            if (target != null)
            {
                LogicCombatComponent combatComponent = target.GetCombatComponent();

                if (combatComponent != null)
                {
                    combatComponent.AttackedBy(this.m_parent);
                }
            }

            this.HitCompleted();

            if (this.m_troopChild)
            {
                this.ForceNewTarget();
            }

            if (!this.m_unk502)
            {
                if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) this.m_parent;

                    if (character.GetParent() != null)
                    {
                        character.GetParent().GetCombatComponent().HitCompleted();
                    }
                }
            }
        }

        public void HitCompleted()
        {
            this.m_hitCount += 1;

            if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;
                LogicCharacterData data = character.GetCharacterData();

                if (data.GetSpecialAbilityType() == LogicCharacterData.SPECIAL_ABILITY_TYPE_SPECIAL_PROJECTILE)
                {
                    if (this.m_targets[0] != null && !this.IsInRange(this.m_targets[0]))
                    {
                        this.StopAttack();
                        this.RefreshTarget(true);
                    }
                }
            }
        }

        public LogicGameObject GetChainAttackTarget(int midX, int midY, int attackDistance)
        {
            this.m_enemyList.Clear();
            this.m_parent.GetGameObjectManager().GetGameObjects(this.m_enemyList, this.m_enemyFilter);

            int minDistance = 0x7FFFFFFF;
            LogicGameObject closestGameObject = null;

            for (int i = 0; i < this.m_enemyList.Size(); i++)
            {
                LogicGameObject gameObject = this.m_enemyList[i];

                if (!gameObject.IsHidden() && this.CanAttackHeightCheck(gameObject))
                {
                    if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                    {
                        LogicCharacter character = (LogicCharacter) gameObject;

                        if (character.GetSpawnDelay() > 0 || character.GetSpawnIdleTime() > 0 || character.GetCombatComponent()?.GetUndergroundTime() > 0 ||
                            character.GetChildTroops() != null)
                        {
                            continue;
                        }
                    }

                    bool isTarget = false;

                    for (int j = 0; j < 5; j++)
                    {
                        if (gameObject == this.m_targets[j])
                        {
                            isTarget = true;
                        }
                    }

                    if (!isTarget)
                    {
                        int distanceX = (gameObject.GetMidX() - midX) >> 9;
                        int distanceY = (gameObject.GetMidY() - midY) >> 9;
                        int distance = distanceX * distanceX + distanceY * distanceY;

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestGameObject = gameObject;
                        }
                    }
                }
            }

            if (minDistance < attackDistance * attackDistance)
            {
                return closestGameObject;
            }

            return null;
        }

        public int GetDamage()
        {
            int damage = this.m_damage;

            if (this.m_altMultiTargets)
            {
                if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
                {
                    damage = this.m_attackerData.GetDamage(0, this.m_attackerData.GetMultiTargets(true));
                    goto CONTINUE;
                }
            }
            else if (this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
            {
                damage = this.m_attackerData.GetAltDamage(0, this.m_attackerData.GetMultiTargets(true));
                goto CONTINUE;
            }

            if (this.m_attackerData.IsIncreasingDamage())
            {
                int damageType = this.GetDamageLevel();

                if (damageType != 0)
                {
                    damage = this.m_attackerData.GetDamage(damageType, false);
                }
            }

            CONTINUE:

            int slowDamage = 0;
            int boostDamage = 0;
            int mergeDamage = 0;

            if (this.m_boostTime[0] > 0)
            {
                boostDamage = (int) ((long) this.m_boostDamage[0] * damage / 100L);
            }

            if (this.m_slowTime > 0)
            {
                slowDamage = (int) ((long) this.m_slowDamage * damage / 100L);
            }

            if (this.m_mergeDamage > 0)
            {
                mergeDamage = (int) ((long) this.m_mergeDamage * damage / 100L);
            }

            return damage + boostDamage + slowDamage + mergeDamage;
        }

        public int GetDamageLevel()
        {
            int damageTime = 0;

            if (this.m_unk502)
            {
                damageTime = this.m_damageTime;
            }
            else if (this.m_parent.GetGameObjectType() == LogicGameObjectType.CHARACTER)
            {
                LogicCharacter character = (LogicCharacter) this.m_parent;
                LogicGameObject gameObject = character.GetParent() == null ? this.m_parent : character.GetParent();

                damageTime = gameObject.GetCombatComponent().m_hitCount;
            }

            if (!this.m_attackerData.IsIncreasingDamage() || this.m_altMultiTargets && this.m_useAltAttackMode[this.m_parent.GetLevel().GetCurrentLayout()])
            {
                return 0;
            }

            int damageLevel = 0;

            if (damageTime >= this.m_attackerData.GetSwitchTimeLv2())
            {
                damageLevel = 1;
            }

            if (damageTime >= this.m_attackerData.GetSwitchTimeLv3())
            {
                damageLevel = 2;
            }

            return damageLevel;
        }

        public void SetSkeletonSpell()
        {
            this.m_skeletonSpell = true;
        }

        public int GetWakeUpTime()
        {
            return this.m_wakeUpTime;
        }

        public int GetDeployedHousingSpace()
        {
            return this.m_deployedHousingSpace;
        }

        public void SetMergeDamage(int damage)
        {
            this.m_mergeDamage = damage;
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.COMBAT;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            if (this.m_hasAltAttackMode)
            {
                for (int i = 0; i < 8; i++)
                {
                    LogicJSONBoolean attackModeObject = jsonObject.GetJSONBoolean(this.GetLayoutVariableNameAttackMode(i, false));

                    if (attackModeObject != null)
                    {
                        this.m_useAltAttackMode[i] = attackModeObject.IsTrue();
                    }

                    LogicJSONBoolean draftAttackModeObject = jsonObject.GetJSONBoolean(this.GetLayoutVariableNameAttackMode(i, true));

                    if (draftAttackModeObject != null)
                    {
                        this.m_draftUseAltAttackMode[i] = draftAttackModeObject.IsTrue();
                    }
                }
            }

            if (this.m_attackerData.GetAmmoCount() > 0)
            {
                LogicJSONNumber ammoObject = jsonObject.GetJSONNumber("ammo");
                this.m_ammo = ammoObject != null ? ammoObject.GetIntValue() : 0;
            }

            if (this.m_attackerData.GetTargetingConeAngle() > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    LogicJSONNumber aimAngleObject = jsonObject.GetJSONNumber(this.GetLayoutVariableNameAimAngle(i, false));

                    if (aimAngleObject != null)
                    {
                        this.m_aimAngle[i] = aimAngleObject.GetIntValue();
                    }

                    LogicJSONNumber draftAimAngleObject = jsonObject.GetJSONNumber(this.GetLayoutVariableNameAimAngle(i, true));

                    if (draftAimAngleObject != null)
                    {
                        this.m_draftAimAngle[i] = draftAimAngleObject.GetIntValue();
                    }
                }
            }
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            if (this.m_hasAltAttackMode)
            {
                for (int i = 0; i < 8; i++)
                {
                    LogicJSONBoolean attackModeObject = jsonObject.GetJSONBoolean(this.GetLayoutVariableNameAttackMode(i, false));

                    if (attackModeObject != null)
                    {
                        this.m_useAltAttackMode[i] = attackModeObject.IsTrue();
                    }
                }
            }

            if (this.m_attackerData.GetAmmoCount() > 0)
            {
                this.m_ammo = this.m_attackerData.GetAmmoCount();
            }

            if (this.m_attackerData.GetTargetingConeAngle() > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    LogicJSONNumber aimAngleObject = jsonObject.GetJSONNumber(this.GetLayoutVariableNameAimAngle(i, false));

                    if (aimAngleObject != null)
                    {
                        this.m_aimAngle[i] = aimAngleObject.GetIntValue();
                    }
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            if (this.m_hasAltAttackMode)
            {
                for (int i = 0; i < 8; i++)
                {
                    jsonObject.Put(this.GetLayoutVariableNameAttackMode(i, false), new LogicJSONBoolean(this.m_useAltAttackMode[i]));
                    jsonObject.Put(this.GetLayoutVariableNameAttackMode(i, true), new LogicJSONBoolean(this.m_draftUseAltAttackMode[i]));
                }
            }

            if (this.m_ammo > 0)
            {
                jsonObject.Put("ammo", new LogicJSONNumber(this.m_ammo));
            }

            if (this.m_attackerData.GetTargetingConeAngle() > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    jsonObject.Put(this.GetLayoutVariableNameAimAngle(i, false), new LogicJSONNumber(this.m_aimAngle[i]));
                    jsonObject.Put(this.GetLayoutVariableNameAimAngle(i, true), new LogicJSONNumber(this.m_draftAimAngle[i]));
                }
            }
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            if (this.m_hasAltAttackMode)
            {
                for (int i = 0; i < 8; i++)
                {
                    jsonObject.Put(this.GetLayoutVariableNameAttackMode(i, false), new LogicJSONBoolean(this.m_useAltAttackMode[layoutId]));
                }
            }

            if (this.m_ammo > 0)
            {
                jsonObject.Put("ammo", new LogicJSONNumber(this.m_ammo));
            }

            if (this.m_attackerData.GetTargetingConeAngle() > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    jsonObject.Put(this.GetLayoutVariableNameAimAngle(i, false), new LogicJSONNumber(this.m_aimAngle[layoutId]));
                }
            }
        }

        public string GetLayoutVariableNameAttackMode(int idx, bool draftMode)
        {
            if (draftMode)
            {
                switch (idx)
                {
                    case 0: return "attack_mode_draft";
                    case 1: return "attack_mode_d1";
                    case 2: return "attack_mode_d2";
                    case 3: return "attack_mode_d3";
                    case 4: return "attack_mode_d4";
                    case 5: return "attack_mode_d5";
                    case 6: return "attack_mode_dchal";
                    case 7: return "attack_mode_draft_arrw";
                    default:
                        Debugger.Error("Layout index out of bounds");
                        return "attack_mode_draft";
                }
            }

            switch (idx)
            {
                case 0: return "attack_mode";
                case 1: return "attack_mode1";
                case 2: return "attack_mode2";
                case 3: return "attack_mode3";
                case 4: return "attack_mode4";
                case 5: return "attack_mode5";
                case 6: return "attack_mode_chal";
                case 7: return "attack_mode_arrw";
                default:
                    Debugger.Error("Layout index out of bounds");
                    return "attack_mode";
            }
        }

        public string GetLayoutVariableNameAimAngle(int idx, bool draftMode)
        {
            if (draftMode)
            {
                switch (idx)
                {
                    case 0: return "aim_angle_draft";
                    case 1: return "aim_angle_d1";
                    case 2: return "aim_angle_d2";
                    case 3: return "aim_angle_d3";
                    case 4: return "aim_angle_d4";
                    case 5: return "aim_angle_d5";
                    case 6: return "aim_angle_dchal";
                    case 7: return "aim_angle_draft_arrw";
                    default:
                        Debugger.Error("Layout index out of bounds");
                        return "aim_angle_draft";
                }
            }

            switch (idx)
            {
                case 0: return "aim_angle";
                case 1: return "aim_angle1";
                case 2: return "aim_angle2";
                case 3: return "aim_angle3";
                case 4: return "aim_angle4";
                case 5: return "aim_angle5";
                case 6: return "aim_angle5_chal";
                case 7: return "aim_angle_arrw";
                default:
                    Debugger.Error("Layout index out of bounds");
                    return "aim_angle";
            }
        }

        public static bool IsPreferredTarget(LogicData target, LogicGameObject gameObject)
        {
            if (target != null && gameObject != null)
            {
                LogicGameObjectData data = gameObject.GetData();

                if (target.GetDataType() != LogicDataType.BUILDING_CLASS || data.GetDataType() != LogicDataType.BUILDING)
                {
                    if (target.GetDataType() != LogicDataType.CHARACTER || data.GetDataType() != LogicDataType.CHARACTER ||
                        ((LogicCharacterData) data).IsSecondaryTroop() ||
                        ((LogicCharacter) gameObject).GetSecondaryTroopTeam() == 0)
                    {
                        return data == target;
                    }
                }
                else
                {
                    LogicBuildingData buildingData = (LogicBuildingData) data;

                    if (buildingData.GetBuildingClass() == target)
                    {
                        return true;
                    }

                    if (buildingData.GetSecondaryTargetingClass() != null)
                    {
                        return buildingData.GetSecondaryTargetingClass() == target;
                    }
                }
            }

            return false;
        }
    }
}