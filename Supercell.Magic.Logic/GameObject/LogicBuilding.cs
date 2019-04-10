namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Calendar;
    using Supercell.Magic.Logic.Command.Battle;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Mode;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicBuilding : LogicGameObject
    {
        private int m_gear;
        private int m_upgLevel;
        private int m_wallIndex;
        private int m_wallBlockX;
        private int m_direction;
        private int m_selectedWallTime;
        private int m_hitWallDelay;
        private int m_dieTime;

        private bool m_wallPoint;
        private bool m_locked;
        private bool m_hidden;
        private bool m_hasAreaOfEffectSpell;
        private bool m_gearing;
        private bool m_upgrading;
        private bool m_boostPause;

        private LogicTimer m_constructionTimer;
        private LogicTimer m_boostCooldownTimer;
        private LogicTimer m_boostTimer;
        private LogicAttackerItemData m_shareHeroCombatData;
        private LogicSpell m_auraSpell; // 188
        private LogicSpell m_areaOfEffectSpell; // 160
        private LogicGameObjectFilter m_filter;

        public LogicBuilding(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            LogicBuildingData buildingData = this.GetBuildingData();

            this.m_locked = buildingData.IsLocked();

            if (buildingData.GetHitpoints(0) > 0)
            {
                LogicHitpointComponent hitpointComponent = new LogicHitpointComponent(this, buildingData.GetHitpoints(0), 1);
                hitpointComponent.SetMaxRegenerationTime(buildingData.GetRegenerationTime(0));
                this.AddComponent(hitpointComponent);
            }

            LogicAttackerItemData attackerItemData = buildingData.GetAttackerItemData(0);

            if (buildingData.GetHeroData() != null)
            {
                LogicHeroBaseComponent heroBaseComponent = new LogicHeroBaseComponent(this, buildingData.GetHeroData());

                this.AddComponent(heroBaseComponent);

                if (buildingData.GetShareHeroCombatData())
                {
                    this.m_shareHeroCombatData = buildingData.GetHeroData().GetAttackerItemData(0).Clone();
                    this.m_shareHeroCombatData.AddAttackRange(buildingData.GetWidth() * 72704 / 200);

                    heroBaseComponent.SetSharedHeroCombatData(true);
                    attackerItemData = this.m_shareHeroCombatData;
                }
            }

            if (buildingData.IsLaboratory())
            {
                LogicUnitUpgradeComponent unitUpgradeComponent = new LogicUnitUpgradeComponent(this);
                unitUpgradeComponent.SetEnabled(false);
                this.AddComponent(unitUpgradeComponent);
            }

            if (buildingData.GetVillage2Housing() > 0)
            {
                LogicVillage2UnitComponent village2UnitComponent = new LogicVillage2UnitComponent(this);
                village2UnitComponent.SetEnabled(false);
                this.AddComponent(village2UnitComponent);
            }

            if (buildingData.GetUnitProduction(0) > 0)
            {
                LogicUnitProductionComponent unitProductionComponent = new LogicUnitProductionComponent(this);
                unitProductionComponent.SetEnabled(false);
                this.AddComponent(unitProductionComponent);
            }

            if (buildingData.GetUnitStorageCapacity(0) > 0)
            {
                if (buildingData.IsAllianceCastle())
                {
                    this.AddComponent(new LogicBunkerComponent(this, 0));
                }
                else
                {
                    this.AddComponent(new LogicUnitStorageComponent(this, 0));
                }
            }

            if (attackerItemData.GetDamage(0, false) > 0 ||
                attackerItemData.GetDamage2() > 0 ||
                buildingData.GetAreaOfEffectSpell() != null ||
                attackerItemData.GetShockwavePushStrength() > 0 ||
                attackerItemData.GetHitSpell() != null)
            {
                LogicCombatComponent combatComponent = new LogicCombatComponent(this);

                combatComponent.SetAttackValues(attackerItemData, 100);
                combatComponent.SetEnabled(false);

                this.AddComponent(combatComponent);
            }

            if (buildingData.GetProduceResource() != null)
            {
                LogicResourceProductionComponent resourceProductionComponent = new LogicResourceProductionComponent(this, buildingData.GetProduceResource());
                resourceProductionComponent.SetEnabled(false);
                this.AddComponent(resourceProductionComponent);
            }

            if (buildingData.StoresResources())
            {
                if (buildingData.IsAllianceCastle())
                {
                    this.AddComponent(new LogicWarResourceStorageComponent(this));
                }
                else
                {
                    this.AddComponent(new LogicResourceStorageComponent(this));
                }
            }

            if (buildingData.GetDefenceTroopCharacter(0) != null)
            {
                this.AddComponent(new LogicDefenceUnitProductionComponent(this));
            }

            this.AddComponent(new LogicLayoutComponent(this));

            this.InitHidden(buildingData);
            this.InitAoeSpell(buildingData);
        }

        public LogicBuildingData GetBuildingData()
        {
            return (LogicBuildingData) this.m_data;
        }

        public void InitHidden(LogicBuildingData data)
        {
            if (data.IsHidden())
            {
                if (this.m_level.IsInCombatState())
                {
                    this.m_hidden = true;
                    this.m_filter = new LogicGameObjectFilter();
                    this.m_filter.AddGameObjectType(LogicGameObjectType.CHARACTER);
                    this.m_filter.PassEnemyOnly(this);
                }
            }
        }

        public void InitAoeSpell(LogicBuildingData data)
        {
            if (data.GetAreaOfEffectSpell() != null)
            {
                if (this.m_level.IsInCombatState())
                {
                    this.m_hasAreaOfEffectSpell = true;

                    if (this.m_filter == null)
                    {
                        this.m_filter = new LogicGameObjectFilter();
                        this.m_filter.AddGameObjectType(LogicGameObjectType.CHARACTER);
                        this.m_filter.PassEnemyOnly(this);
                    }
                }
            }
        }

        public LogicHeroBaseComponent GetHeroBaseComponent()
        {
            return (LogicHeroBaseComponent) this.GetComponent(LogicComponentType.HERO_BASE);
        }

        public LogicUnitProductionComponent GetUnitProductionComponent()
        {
            return (LogicUnitProductionComponent) this.GetComponent(LogicComponentType.UNIT_PRODUCTION);
        }

        public LogicUnitStorageComponent GetUnitStorageComponent()
        {
            return (LogicUnitStorageComponent) this.GetComponent(LogicComponentType.UNIT_STORAGE);
        }

        public LogicResourceStorageComponent GetResourceStorageComponentComponent()
        {
            return (LogicResourceStorageComponent) this.GetComponent(LogicComponentType.RESOURCE_STORAGE);
        }

        public LogicUnitUpgradeComponent GetUnitUpgradeComponent()
        {
            return (LogicUnitUpgradeComponent) this.GetComponent(LogicComponentType.UNIT_UPGRADE);
        }

        public LogicWarResourceStorageComponent GetWarResourceStorageComponent()
        {
            return (LogicWarResourceStorageComponent) this.GetComponent(LogicComponentType.WAR_RESOURCE_STORAGE);
        }

        public LogicVillage2UnitComponent GetVillage2UnitComponent()
        {
            return (LogicVillage2UnitComponent) this.GetComponent(LogicComponentType.VILLAGE2_UNIT);
        }

        public int GetRemainingConstructionTime()
        {
            if (this.m_constructionTimer != null)
            {
                return this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetRemainingConstructionTimeMS()
        {
            if (this.m_constructionTimer != null)
            {
                return this.m_constructionTimer.GetRemainingMS(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetRemainingBoostCooldownTime()
        {
            if (this.m_boostCooldownTimer != null)
            {
                return this.m_boostCooldownTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetGearLevel()
        {
            return this.m_gear;
        }

        public void SetGearLevel(int value)
        {
            this.m_gear = value;
        }

        public LogicAttackerItemData GetAttackerItemData()
        {
            return this.GetBuildingData().GetAttackerItemData(this.m_upgLevel);
        }

        public override int GetRemainingBoostTime()
        {
            if (this.m_boostTimer != null)
            {
                return this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public override int GetMaxFastForwardTime()
        {
            if (this.m_constructionTimer != null && !LogicDataTables.GetGlobals().CompleteConstructionOnlyHome())
            {
                return LogicMath.Max(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime()), 1);
            }

            LogicVillage2UnitComponent village2UnitComponent = (LogicVillage2UnitComponent) this.GetComponent(LogicComponentType.VILLAGE2_UNIT);

            if (village2UnitComponent != null && village2UnitComponent.GetCurrentlyTrainedUnit() != null)
            {
                int remainingSecs = village2UnitComponent.GetRemainingSecs();

                if (remainingSecs > 0)
                {
                    return remainingSecs;
                }

                return -1;
            }

            LogicUnitProductionComponent unitProductionComponent = this.GetUnitProductionComponent();

            if (unitProductionComponent != null)
            {
                if (this.GetRemainingBoostTime() <= 0 || this.GetBoostMultiplier() <= 0 || this.m_boostPause)
                {
                    // TODO: Implement this.
                }
            }

            return -1;
        }

        public int GetMaxBoostTime()
        {
            if (this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION) != null)
            {
                return LogicDataTables.GetGlobals().GetResourceProductionBoostSecs();
            }

            if (this.GetComponent(LogicComponentType.UNIT_PRODUCTION) != null)
            {
                if (this.GetUnitProductionComponent().GetProductionType() == 0)
                {
                    return LogicDataTables.GetGlobals().GetBarracksBoostSecs();
                }

                return LogicDataTables.GetGlobals().GetSpellFactoryBoostSecs();
            }

            if (this.GetComponent(LogicComponentType.HERO_BASE) != null)
            {
                LogicHeroBaseComponent heroBaseComponent = (LogicHeroBaseComponent) this.GetComponent(LogicComponentType.HERO_BASE);

                if (!heroBaseComponent.IsUpgrading())
                {
                    return LogicDataTables.GetGlobals().GetHeroRestBoostSecs();
                }
            }

            if (this.GetBuildingData().IsClockTower())
            {
                return LogicDataTables.GetGlobals().GetClockTowerBoostSecs(this.m_upgLevel);
            }

            return 0;
        }

        public override void DeathEvent()
        {
            base.DeathEvent();

            LogicBuildingData data = this.GetBuildingData();

            // DestructEffect.

            this.m_level.GetBattleLog().IncrementDestroyedBuildingCount(data);
            this.m_level.GetTileMap().GetPathFinder().InvalidateCache();
            this.m_level.GetVisitorAvatar().XpGainHelper(data.GetDestructionXP(this.m_upgLevel));

            LogicCalendarBuildingDestroyedSpawnUnit buildingDestroyedSpawnUnit = this.m_level.GetCalendar().GetBuildingDestroyedSpawnUnit();

            if (buildingDestroyedSpawnUnit != null && buildingDestroyedSpawnUnit.GetBuildingData() == this.m_data)
            {
                LogicCharacterData characterData = buildingDestroyedSpawnUnit.GetCharacterData();

                for (int i = 0, c = buildingDestroyedSpawnUnit.GetCount(), offsetX = 0, offsetY = 0; i < c; i++, offsetX += 721, offsetY += 1051)
                {
                    LogicCharacter character = (LogicCharacter) LogicGameObjectFactory.CreateGameObject(characterData, this.m_level, this.GetVillageType());

                    character.SetInitialPosition(this.GetMidX() + offsetX % 512, this.GetMidY() + offsetY % 512);

                    LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();

                    if (hitpointComponent != null)
                    {
                        hitpointComponent.SetTeam(1);
                        hitpointComponent.SetInvulnerabilityTime(64);
                    }

                    if (LogicDataTables.GetGlobals().EnableDefendingAllianceTroopJump())
                    {
                        this.GetMovementComponent().EnableJump(3600000);
                    }

                    this.m_level.GetGameObjectManager().AddGameObject(character, -1);
                    this.GetCombatComponent().SetSearchRadius(LogicDataTables.GetGlobals().GetClanCastleRadius() >> 9);
                    this.GetMovementComponent().GetMovementSystem().CreatePatrolArea(this, this.m_level, true, i);
                }
            }
        }

        public bool IsConstructing()
        {
            return this.m_constructionTimer != null;
        }

        public bool IsUpgrading()
        {
            return this.m_constructionTimer != null && this.m_upgrading;
        }

        public bool IsGearing()
        {
            return this.m_constructionTimer != null && this.m_gearing;
        }

        public bool IsLocked()
        {
            return this.m_locked;
        }

        public void SetLocked(bool locked)
        {
            this.m_locked = locked;
        }

        public void Lock()
        {
            this.m_locked = true;
            this.SetUpgradeLevel(0);
        }

        public override bool IsBuilding()
        {
            return true;
        }

        public override bool IsBoostPaused()
        {
            return this.m_boostPause;
        }

        public void SetBoostPause(bool state)
        {
            this.m_boostPause = state;
        }

        public int GetUpgradeLevel()
        {
            return this.m_upgLevel;
        }

        public int GetWallIndex()
        {
            return this.m_wallIndex;
        }

        public override bool IsWall()
        {
            return this.GetBuildingData().IsWall();
        }

        public override bool IsPassable()
        {
            if (!LogicDataTables.GetGlobals().RemoveUntriggeredTesla() && !LogicDataTables.GetGlobals().UseTeslaTriggerCommand() || !this.GetBuildingData().IsHidden())
            {
                int state = this.m_level.GetState();

                if (state != 1 && state != 4)
                {
                    LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                    if (hitpointComponent != null && hitpointComponent.GetHitpoints() <= 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public bool IsConnectedWall()
        {
            if (this.IsWall())
            {
                int connectedCount = 0;
                int tileX = this.GetTileX();
                int tileY = this.GetTileY();

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if ((i | j) != 0)
                        {
                            LogicTile tile = this.m_level.GetTileMap().GetTile(tileX + i, tileY + j);

                            if (tile != null)
                            {
                                for (int k = 0; k < tile.GetGameObjectCount(); k++)
                                {
                                    if (tile.GetGameObject(k).IsWall())
                                    {
                                        connectedCount += 1;
                                    }
                                }
                            }
                        }
                    }
                }

                return connectedCount > 1;
            }

            return false;
        }

        public override int GetWidthInTiles()
        {
            return this.GetBuildingData().GetWidth();
        }

        public override int GetHeightInTiles()
        {
            return this.GetBuildingData().GetHeight();
        }

        public override int PassableSubtilesAtEdge()
        {
            if (!this.IsWall())
            {
                return LogicMath.Max(1, this.GetBuildingData().GetWidth() - this.GetBuildingData().GetBuildingW());
            }

            return 0;
        }

        public override int PathFinderCost()
        {
            if (this.IsWall() && this.IsAlive())
            {
                LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                if (hitpointComponent != null)
                {
                    if (this.m_hitWallDelay <= 0)
                    {
                        int hp = hitpointComponent.GetHitpoints() / 100;
                        int maxHp = hitpointComponent.GetMaxHitpoints() / 100;
                        int wallCostBase = LogicDataTables.GetGlobals().GetWallCostBase();
                        int hpMultiplier = 4000 - wallCostBase;

                        if (this.m_selectedWallTime > 0)
                        {
                            hpMultiplier = (3 * hpMultiplier) >> 2;
                        }

                        return wallCostBase + this.Rand(0) % 256 + hp * hpMultiplier / maxHp;
                    }

                    return 100;
                }
            }

            return 0;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            if (this.m_boostCooldownTimer != null)
            {
                this.m_boostCooldownTimer.Destruct();
                this.m_boostCooldownTimer = null;
            }

            if (this.m_boostTimer != null)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (this.m_hitWallDelay > 0)
            {
                this.m_hitWallDelay = LogicMath.Max(this.m_hitWallDelay - 64, 0);

                if (this.m_hitWallDelay == 0)
                {
                    this.RefreshPassable();
                }
            }

            if (this.m_selectedWallTime > 0)
            {
                this.m_selectedWallTime = LogicMath.Max(this.m_selectedWallTime - 64, 0);

                if (this.m_selectedWallTime == 0)
                {
                    this.RefreshPassable();
                }
            }

            if (this.m_constructionTimer != null)
            {
                if (this.m_level.GetRemainingClockTowerBoostTime() > 0 && this.GetBuildingData().GetVillageType() == 1)
                {
                    this.m_constructionTimer.SetFastForward(this.m_constructionTimer.GetFastForward() + 4 * LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 4);
                }

                if (this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) <= 0)
                {
                    this.FinishConstruction(false, true);
                }
            }

            if (this.m_boostTimer != null)
            {
                if (this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) <= 0)
                {
                    this.m_boostTimer.Destruct();
                    this.m_boostTimer = null;

                    if (this.GetBuildingData().IsClockTower())
                    {
                        this.m_boostCooldownTimer = new LogicTimer();
                        this.m_boostCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetClockTowerBoostCooldownSecs(), this.m_level.GetLogicTime(), false, -1);

                        if (this.m_level.GetGameListener() != null)
                        {
                            // LogicGameListener.
                        }
                    }

                    if (this.m_listener != null)
                    {
                        this.m_listener.RefreshState();
                    }
                }
            }

            LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

            if (hitpointComponent != null)
            {
                hitpointComponent.EnableRegeneration(this.m_level.GetState() == 1);
            }

            if (this.m_level.IsInCombatState())
            {
                if (this.m_hidden)
                {
                    this.UpdateHidden();
                }

                if (this.m_hasAreaOfEffectSpell)
                {
                    this.UpdateAreaOfEffectSpell();
                }

                if (!this.IsAlive())
                {
                    LogicBuildingData data = this.GetBuildingData();

                    int dieDamageDelay = data.GetDieDamageDelay();
                    int prevDieDamage = this.m_dieTime;

                    this.m_dieTime += 64;

                    if (dieDamageDelay >= prevDieDamage && dieDamageDelay < this.m_dieTime)
                    {
                        this.UpdateDieDamage(data.GetDieDamage(this.m_upgLevel), data.GetDieDamageRadius());
                    }
                }

                this.UpdateAuraSpell();
            }
        }

        public override void SubTick()
        {
            LogicCombatComponent combatComponent = this.GetCombatComponent();

            if (combatComponent != null)
            {
                combatComponent.SubTick();

                if (combatComponent.GetTarget(0) != null)
                {
                    LogicBuildingData buildingData = this.GetBuildingData();

                    int angleToTarget = this.GetAngleToTarget();

                    if (angleToTarget <= 0)
                    {
                        if (angleToTarget < 0)
                        {
                            this.m_direction -= 16 * buildingData.GetTurnSpeed();
                        }
                    }
                    else
                    {
                        this.m_direction += 16 * buildingData.GetTurnSpeed();
                    }

                    if (angleToTarget * this.GetAngleToTarget() < 0)
                    {
                        this.m_direction = 1000 * (this.GetDirection() + this.GetAngleToTarget());
                    }

                    if (this.m_direction < 360000)
                    {
                        if (this.m_direction < 0)
                        {
                            this.m_direction += 360000;
                        }
                    }
                    else
                    {
                        this.m_direction -= 360000;
                    }

                    LogicAttackerItemData attackerItemData = combatComponent.GetAttackerItemData();

                    if (attackerItemData.GetTargetingConeAngle() > 0)
                    {
                        int aimAngle = 1000 * combatComponent.GetAimAngle(this.m_level.GetActiveLayout(this.m_level.GetVillageType()), false);
                        int newDirection = this.m_direction - aimAngle;

                        if (newDirection < 180000)
                        {
                            if (newDirection < -180000)
                            {
                                newDirection += 360000;
                            }
                        }
                        else
                        {
                            newDirection -= 360000;
                        }

                        this.m_direction = aimAngle + LogicMath.Clamp(newDirection, -500 * attackerItemData.GetTargetingConeAngle(),
                                                                      500 * attackerItemData.GetTargetingConeAngle());

                        if (this.m_direction < 360000)
                        {
                            if (this.m_direction < 0)
                            {
                                this.m_direction += 360000;
                            }
                        }
                        else
                        {
                            this.m_direction -= 360000;
                        }
                    }
                }
            }

            if (this.m_boostCooldownTimer != null && this.m_boostPause)
            {
                this.m_boostCooldownTimer.StartTimer(this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime()), this.m_level.GetLogicTime(), false, -1);
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            if (this.m_upgLevel != 0 || this.m_constructionTimer == null || this.m_upgrading)
            {
                jsonObject.Put("lvl", new LogicJSONNumber(this.m_upgLevel));
            }
            else
            {
                jsonObject.Put("lvl", new LogicJSONNumber(-1));
            }

            if (this.m_gearing)
            {
                jsonObject.Put("gearing", new LogicJSONBoolean(true));
            }

            if (this.m_constructionTimer != null)
            {
                jsonObject.Put("const_t", new LogicJSONNumber(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));

                if (this.m_constructionTimer.GetEndTimestamp() != -1)
                {
                    jsonObject.Put("const_t_end", new LogicJSONNumber(this.m_constructionTimer.GetEndTimestamp()));
                }

                if (this.m_constructionTimer.GetFastForward() > 0)
                {
                    jsonObject.Put("con_ff", new LogicJSONNumber(this.m_constructionTimer.GetFastForward()));
                }
            }

            if (this.m_locked)
            {
                jsonObject.Put("locked", new LogicJSONBoolean(true));
            }

            if (this.m_boostTimer != null)
            {
                jsonObject.Put("boost_t", new LogicJSONNumber(this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));
            }

            if (this.m_boostPause)
            {
                jsonObject.Put("boost_pause", new LogicJSONBoolean(true));
            }

            if (this.GetRemainingBoostCooldownTime() > 0)
            {
                jsonObject.Put("bcd", new LogicJSONNumber(this.GetRemainingBoostCooldownTime()));
            }

            if (this.m_gear > 0)
            {
                jsonObject.Put("gear", new LogicJSONNumber(this.m_gear));
            }

            if (this.IsWall())
            {
                jsonObject.Put("wI", new LogicJSONNumber(this.m_wallIndex));

                if (this.m_wallPoint)
                {
                    jsonObject.Put("wP", new LogicJSONNumber(1));
                }

                jsonObject.Put("wX", new LogicJSONNumber(this.m_wallBlockX));
            }

            base.Save(jsonObject, villageType);
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            if (this.m_upgLevel != 0 || this.m_constructionTimer == null || this.m_upgrading)
            {
                jsonObject.Put("lvl", new LogicJSONNumber(this.m_upgLevel));
            }
            else
            {
                jsonObject.Put("lvl", new LogicJSONNumber(-1));
            }

            if (this.m_gearing)
            {
                jsonObject.Put("gearing", new LogicJSONBoolean(true));
            }

            if (this.m_constructionTimer != null)
            {
                jsonObject.Put("const_t", new LogicJSONNumber(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));
            }

            if (this.m_locked)
            {
                jsonObject.Put("locked", new LogicJSONBoolean(true));
            }

            if (this.m_gear > 0)
            {
                jsonObject.Put("gear", new LogicJSONNumber(this.m_gear));
            }

            base.SaveToSnapshot(jsonObject, layoutId);
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            this.LoadUpgradeLevel(jsonObject);

            LogicJSONNumber constTimeObject = jsonObject.GetJSONNumber("const_t");

            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            LogicJSONBoolean gearingObject = jsonObject.GetJSONBoolean("gearing");

            if (gearingObject != null)
            {
                this.m_gearing = gearingObject.IsTrue();
            }

            if (constTimeObject != null)
            {
                int secs = constTimeObject.GetIntValue();

                if (!LogicDataTables.GetGlobals().ClampBuildingTimes())
                {
                    if (this.m_upgLevel < this.GetBuildingData().GetUpgradeLevelCount() - 1)
                    {
                        secs = LogicMath.Min(secs, this.GetBuildingData().GetConstructionTime(this.m_upgLevel + 1, this.m_level, 0));
                    }
                }

                this.m_constructionTimer = new LogicTimer();
                this.m_constructionTimer.StartTimer(secs, this.m_level.GetLogicTime(), false, -1);

                LogicJSONNumber constTimeEndObject = jsonObject.GetJSONNumber("const_t_end");

                if (constTimeEndObject != null)
                {
                    this.m_constructionTimer.SetEndTimestamp(constTimeEndObject.GetIntValue());
                }

                LogicJSONNumber conFFObject = jsonObject.GetJSONNumber("con_ff");

                if (conFFObject != null)
                {
                    this.m_constructionTimer.SetFastForward(conFFObject.GetIntValue());
                }

                if (this.m_gearing)
                {
                    this.m_level.GetWorkerManagerAt(1).AllocateWorker(this);
                }
                else
                {
                    this.m_level.GetWorkerManagerAt(this.m_villageType).AllocateWorker(this);
                    this.m_upgrading = this.m_upgLevel != -1;
                }
            }

            LogicJSONNumber boostTimeObject = jsonObject.GetJSONNumber("boost_t");

            if (this.m_boostTimer != null)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;
            }

            if (boostTimeObject != null)
            {
                this.m_boostTimer = new LogicTimer();
                this.m_boostTimer.StartTimer(boostTimeObject.GetIntValue(), this.m_level.GetLogicTime(), false, -1);
            }

            LogicJSONNumber boostCooldownObject = jsonObject.GetJSONNumber("bcd");

            if (boostCooldownObject != null)
            {
                this.m_boostCooldownTimer = new LogicTimer();
                this.m_boostCooldownTimer.StartTimer(boostCooldownObject.GetIntValue(), this.m_level.GetLogicTime(), false, -1);
            }

            LogicJSONBoolean boostPauseObject = jsonObject.GetJSONBoolean("boost_pause");

            if (boostPauseObject != null)
            {
                this.m_boostPause = boostPauseObject.IsTrue();
            }

            if (this.m_boostTimer == null)
            {
                if (LogicDataTables.GetGlobals().StopBoostPauseWhenBoostTimeZeroOnLoad())
                {
                    this.m_boostPause = false;
                }
            }

            if (this.IsWall())
            {
                LogicJSONNumber wallIndexObject = jsonObject.GetJSONNumber("wI");

                if (wallIndexObject != null)
                {
                    this.m_wallIndex = wallIndexObject.GetIntValue();
                }

                LogicJSONNumber wallXObject = jsonObject.GetJSONNumber("wX");

                if (wallXObject != null)
                {
                    this.m_wallBlockX = wallXObject.GetIntValue();
                }

                LogicJSONNumber wallPositionObject = jsonObject.GetJSONNumber("wP");

                if (wallPositionObject != null)
                {
                    this.m_wallPoint = wallPositionObject.GetIntValue() != 0;
                }
            }

            if (LogicDataTables.GetGlobals().FixMergeOldBarrackBoostPausing())
            {
                if (LogicDataTables.GetGlobals().UseNewTraining())
                {
                    if (this.GetBuildingData().GetUnitProduction(0) > 0)
                    {
                        if (this.m_boostTimer != null)
                        {
                            this.m_boostTimer.Destruct();
                            this.m_boostTimer = null;

                            if (this.m_boostCooldownTimer != null)
                            {
                                this.m_boostCooldownTimer.Destruct();
                                this.m_boostCooldownTimer = null;
                            }
                        }
                    }
                }
            }

            this.SetUpgradeLevel(LogicMath.Clamp(this.m_upgLevel, 0, this.GetBuildingData().GetUpgradeLevelCount() - 1));
            base.Load(jsonObject);
        }

        public void LoadUpgradeLevel(LogicJSONObject jsonObject)
        {
            LogicJSONNumber lvlObject = jsonObject.GetJSONNumber("lvl");

            if (lvlObject != null)
            {
                this.m_upgLevel = lvlObject.GetIntValue();
                int maxUpgLevel = this.GetBuildingData().GetUpgradeLevelCount();

                if (this.m_upgLevel >= maxUpgLevel)
                {
                    Debugger.Warning(string.Format("LogicBuilding::load() - Loaded upgrade level {0} is over max! (max = {1}) id {2} data id {3}", this.m_upgLevel, maxUpgLevel,
                                                   this.m_globalId, this.m_data.GetGlobalID()));
                    this.m_upgLevel = maxUpgLevel - 1;
                }
                else
                {
                    if (this.m_upgLevel < -1)
                    {
                        Debugger.Error("LogicBuilding::load() - Loaded an illegal upgrade level!");
                    }
                }
            }
            else
            {
                Debugger.Error("LogicBuilding::load - Upgrade level was not found!");
                this.m_upgLevel = 0;
            }

            this.m_level.GetWorkerManagerAt(1).DeallocateWorker(this);
            this.m_level.GetWorkerManagerAt(this.m_villageType).DeallocateWorker(this);

            LogicJSONNumber gearObject = jsonObject.GetJSONNumber("gear");

            if (gearObject != null)
            {
                this.m_gear = gearObject.GetIntValue();
            }

            LogicJSONBoolean lockedObject = jsonObject.GetJSONBoolean("locked");

            if (lockedObject != null)
            {
                this.m_locked = lockedObject.IsTrue();
            }
            else
            {
                this.m_locked = false;
            }
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            if (this.m_data.GetVillageType() == 1)
            {
                this.Load(jsonObject);
                return;
            }

            this.LoadUpgradeLevel(jsonObject);
            this.SetUpgradeLevel(LogicMath.Clamp(this.m_upgLevel, 0, this.GetBuildingData().GetUpgradeLevelCount() - 1));
            base.LoadFromSnapshot(jsonObject);
        }

        public override void StopBoost()
        {
            if (this.m_boostTimer != null && this.CanStopBoost() && !this.m_boostPause)
            {
                this.m_boostPause = true;
            }
        }

        public override void FastForwardTime(int secs)
        {
            if (this.m_constructionTimer != null)
            {
                if (this.m_constructionTimer.GetEndTimestamp() == -1)
                {
                    int remainingTime = this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());

                    if (remainingTime > secs)
                    {
                        base.FastForwardTime(secs);
                        this.m_constructionTimer.StartTimer(remainingTime - secs, this.m_level.GetLogicTime(), false, -1);
                    }
                    else
                    {
                        if (LogicDataTables.GetGlobals().CompleteConstructionOnlyHome())
                        {
                            base.FastForwardTime(secs);
                            this.m_constructionTimer.StartTimer(0, this.m_level.GetLogicTime(), false, -1);
                        }
                        else
                        {
                            base.FastForwardTime(remainingTime);
                            this.FinishConstruction(true, true);
                            base.FastForwardTime(secs - remainingTime);
                        }

                        return;
                    }
                }
                else
                {
                    this.m_constructionTimer.AdjustEndSubtick(this.m_level);

                    if (this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) == 0)
                    {
                        if (LogicDataTables.GetGlobals().CompleteConstructionOnlyHome())
                        {
                            base.FastForwardTime(secs);
                            this.m_constructionTimer.StartTimer(0, this.m_level.GetLogicTime(), false, -1);
                        }
                        else
                        {
                            base.FastForwardTime(0);
                            this.FinishConstruction(true, true);
                            base.FastForwardTime(secs);
                        }

                        return;
                    }

                    base.FastForwardTime(secs);
                }

                int maxClockTowerFastForward = this.m_level.GetUpdatedClockTowerBoostTime();

                if (maxClockTowerFastForward > 0 && !this.m_level.IsClockTowerBoostPaused())
                {
                    if (this.m_data.GetVillageType() == 1)
                    {
                        this.m_constructionTimer.SetFastForward(this.m_constructionTimer.GetFastForward() +
                                                                60 * LogicMath.Min(secs, maxClockTowerFastForward) *
                                                                (LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 1));
                    }
                }
            }
            else
            {
                base.FastForwardTime(secs);
            }
        }

        public override void FastForwardBoost(int secs)
        {
            base.FastForwardBoost(secs);

            if (this.GetBuildingData().IsClockTower())
            {
                if (this.m_boostCooldownTimer != null)
                {
                    int remainingSecs = this.m_boostCooldownTimer.GetRemainingSeconds(this.m_level.GetLogicTime());

                    if (remainingSecs <= secs)
                    {
                        this.m_boostCooldownTimer.Destruct();
                        this.m_boostCooldownTimer = null;

                        if (this.m_listener != null)
                        {
                            this.m_listener.RefreshState();
                        }
                    }
                    else
                    {
                        this.m_boostCooldownTimer.StartTimer(remainingSecs - secs, this.m_level.GetLogicTime(), false, -1);
                    }
                }
            }

            if (this.m_boostTimer != null)
            {
                if (!this.m_boostPause)
                {
                    int remainingSecs = this.m_boostTimer.GetRemainingSeconds(this.m_level.GetLogicTime());

                    if (remainingSecs <= secs)
                    {
                        this.m_boostTimer.Destruct();
                        this.m_boostTimer = null;

                        if (this.GetBuildingData().IsClockTower())
                        {
                            int passedSecs = secs - remainingSecs;

                            if (passedSecs < 0)
                            {
                                Debugger.Warning("boost timer run out during FF -> start cooldown, but timeToFF < 0");
                            }

                            this.m_boostCooldownTimer = new LogicTimer();
                            this.m_boostCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetClockTowerBoostCooldownSecs() - passedSecs, this.m_level.GetLogicTime(), false,
                                                                 -1);
                        }

                        if (this.m_listener != null)
                        {
                            this.m_listener.RefreshState();
                        }
                    }
                    else
                    {
                        this.m_boostTimer.StartTimer(remainingSecs - secs, this.m_level.GetLogicTime(), false, -1);
                    }
                }

                if (this.m_boostTimer != null)
                {
                    if (this.GetBuildingData().IsClockTower())
                    {
                        this.m_boostPause = false;
                    }
                }
            }
        }

        public override int GetStrengthWeight()
        {
            return this.GetBuildingData().GetStrengthWeight(this.m_upgLevel);
        }

        public bool IsValidTarget(LogicGameObject target)
        {
            LogicCombatComponent combatComponent = this.GetCombatComponent();

            if (combatComponent != null && combatComponent.GetAttackerItemData().GetTargetingConeAngle() != 0)
            {
                return LogicMath.Abs(this.GetAngleToTarget(target)) <= combatComponent.GetAttackerItemData().GetTargetingConeAngle() / 2;
            }

            if (this.GetBuildingData().IsNeedsAim())
            {
                return LogicMath.Abs(this.GetAngleToTarget(target)) < 5;
            }

            return true;
        }

        public int GetAngleToTarget()
        {
            LogicCombatComponent combatComponent = this.GetCombatComponent();

            if (combatComponent == null || combatComponent.GetTarget(0) == null)
            {
                return 0;
            }

            return this.GetAngleToTarget(combatComponent.GetTarget(0));
        }

        public int GetAngleToTarget(LogicGameObject gameObject)
        {
            int x = gameObject.GetMidX() - this.GetMidX();
            int y = gameObject.GetMidY() - this.GetMidY();

            if (x != 0 || y != 0)
            {
                return LogicMath.NormalizeAngle180(LogicMath.GetAngle(x, y) - this.GetDirection());
            }

            return 0;
        }

        public void UpdateHidden()
        {
            if (((this.m_level.GetLogicTime().GetTick() / 4) & 7) == 0)
            {
                if (this.m_constructionTimer != null)
                {
                    this.m_hidden = false;
                }

                LogicBuildingData data = this.GetBuildingData();
                LogicGameObjectManager gameObjectManager = this.GetGameObjectManager();
                LogicGameObject gameObject = gameObjectManager.GetClosestGameObject(this.GetMidX(), this.GetMidY(), this.m_filter);

                bool isInArea = false;

                if (gameObject != null)
                {
                    LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                    if (combatComponent == null || combatComponent.GetUndergroundTime() <= 0)
                    {
                        if (LogicDataTables.GetGlobals().SkeletonTriggerTesla() || !LogicDataTables.IsSkeleton((LogicCharacterData) this.m_data))
                        {
                            isInArea = gameObject.GetPosition().GetDistanceSquaredTo(this.GetMidX(), this.GetMidY()) < data.GetTriggerRadius() * data.GetTriggerRadius();
                        }
                    }
                }

                if (isInArea || this.m_level.GetBattleLog().GetDestructionPercentage() > LogicDataTables.GetGlobals().GetHiddenBuildingAppearDestructionPercentage())
                {
                    if (LogicDataTables.GetGlobals().UseTeslaTriggerCommand())
                    {
                        if (this.m_level.GetState() != 5)
                        {
                            LogicTriggerTeslaCommand triggerTeslaCommand = new LogicTriggerTeslaCommand(this);
                            triggerTeslaCommand.SetExecuteSubTick(this.m_level.GetLogicTime().GetTick() + 1);
                            this.m_level.GetGameMode().GetCommandManager().AddCommand(triggerTeslaCommand);
                        }
                    }
                    else
                    {
                        this.Trigger();
                    }
                }
            }
        }

        public void UpdateAreaOfEffectSpell()
        {
            if (this.IsAlive())
            {
                LogicGameObject gameObject = this.GetGameObjectManager().GetClosestGameObject(this.GetMidX(), this.GetMidY(), this.m_filter);

                if (gameObject != null)
                {
                    bool altAttackMode = this.GetCombatComponent(false).UseAltAttackMode(this.m_level.GetActiveLayout(this.m_level.GetVillageType()), false);

                    int distanceSquared = gameObject.GetPosition().GetDistanceSquaredTo(this.GetMidX(), this.GetMidY());
                    int radius =
                        (this.m_shareHeroCombatData ?? this.GetBuildingData().GetAttackerItemData(this.m_upgLevel)).GetAttackRange(altAttackMode);

                    if ((uint) distanceSquared <= radius * radius)
                    {
                        if (this.m_areaOfEffectSpell == null)
                        {
                            this.m_areaOfEffectSpell = (LogicSpell) LogicGameObjectFactory.CreateGameObject(this.GetBuildingData().GetAreaOfEffectSpell(), this.m_level,
                                                                                                            this.m_villageType);
                            this.m_areaOfEffectSpell.SetUpgradeLevel(this.m_upgLevel);
                            this.m_areaOfEffectSpell.SetInitialPosition(this.GetMidX(), this.GetMidY());

                            this.GetGameObjectManager().AddGameObject(this.m_areaOfEffectSpell, -1);
                        }

                        return;
                    }
                }
            }

            if (this.m_areaOfEffectSpell != null)
            {
                this.m_upgrading = true;
                this.m_areaOfEffectSpell = null;
            }
        }

        public void UpdateAuraSpell()
        {
            if (this.IsAlive())
            {
                LogicBuildingData data = this.GetBuildingData();

                if (data.GetShareHeroCombatData())
                {
                    LogicHeroBaseComponent heroBaseComponent = this.GetHeroBaseComponent();

                    if (heroBaseComponent != null)
                    {
                        LogicHeroData heroData = heroBaseComponent.GetHeroData();
                        LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

                        int heroUpgLevel = homeOwnerAvatar.GetUnitUpgradeLevel(heroData);

                        if (homeOwnerAvatar.IsHeroAvailableForAttack(heroData))
                        {
                            LogicSpellData auraSpell = heroData.GetAuraSpell(heroUpgLevel);

                            if (auraSpell != null)
                            {
                                if (this.m_auraSpell != null)
                                {
                                    if (!this.m_auraSpell.GetHitsCompleted())
                                    {
                                        return;
                                    }

                                    this.GetGameObjectManager().RemoveGameObject(this.m_auraSpell);
                                    this.m_auraSpell = null;
                                }

                                if (this.m_level.GetBattleLog().GetBattleStarted())
                                {
                                    LogicCombatComponent combatComponent = this.GetCombatComponent();

                                    if (combatComponent != null && combatComponent.GetDeployedHousingSpace() >= combatComponent.GetAttackerItemData().GetWakeUpSpace() &&
                                        combatComponent.GetWakeUpTime() == 0)
                                    {
                                        this.m_auraSpell = (LogicSpell) LogicGameObjectFactory.CreateGameObject(auraSpell, this.m_level, this.m_villageType);
                                        this.m_auraSpell.SetUpgradeLevel(heroData.GetAuraSpellLevel(heroUpgLevel));
                                        this.m_auraSpell.SetInitialPosition(this.GetMidX(), this.GetMidY());
                                        this.m_auraSpell.AllowDestruction(false);
                                        this.m_auraSpell.SetTeam(this.GetHitpointComponent().GetTeam());

                                        this.GetGameObjectManager().AddGameObject(this.m_auraSpell, -1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (this.m_auraSpell != null)
            {
                this.GetGameObjectManager().RemoveGameObject(this.m_auraSpell);
                this.m_auraSpell = null;
            }
        }

        public void UpdateDieDamage(int damage, int radius)
        {
            if (damage > 0 && radius > 0 && this.GetHitpointComponent() != null && this.m_constructionTimer == null)
            {
                this.m_level.AreaDamage(0, this.GetMidX(), this.GetMidY(), radius, damage, null, 0, null, this.GetHitpointComponent().GetTeam(), null, 1, 0, 0, true, false, 100, 0,
                                        this, 100, 0);
                // Listener.
            }
        }

        public bool IsMaxUpgradeLevel()
        {
            LogicBuildingData buildingData = this.GetBuildingData();

            if (buildingData.IsTownHallVillage2())
            {
                return this.m_upgLevel >= this.m_level.GetGameMode().GetConfiguration().GetMaxTownHallLevel() - 1;
            }

            if (buildingData.GetVillageType() == 1)
            {
                if (this.GetRequiredTownHallLevelForUpgrade() >= this.m_level.GetGameMode().GetConfiguration().GetMaxTownHallLevel())
                {
                    return true;
                }
            }

            return this.m_upgLevel >= buildingData.GetUpgradeLevelCount() - 1;
        }

        public int GetRequiredTownHallLevelForUpgrade()
        {
            return this.GetBuildingData().GetRequiredTownHallLevel(LogicMath.Min(this.m_upgLevel + 1, this.GetBuildingData().GetUpgradeLevelCount() - 1));
        }

        public int GetBoostMultiplier()
        {
            if (this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION) != null)
            {
                return LogicDataTables.GetGlobals().GetResourceProductionBoostMultiplier();
            }

            if (this.GetComponent(LogicComponentType.UNIT_PRODUCTION) == null)
            {
                if (this.GetComponent(LogicComponentType.HERO_BASE) != null)
                {
                    return LogicDataTables.GetGlobals().GetHeroRestBoostMultiplier();
                }

                if (this.GetBuildingData().IsClockTower())
                {
                    return LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier();
                }
            }
            else
            {
                LogicUnitProductionComponent unitProductionComponent = (LogicUnitProductionComponent) this.GetComponent(LogicComponentType.UNIT_PRODUCTION);

                if (unitProductionComponent.GetProductionType() == 1)
                {
                    if (LogicDataTables.GetGlobals().UseNewTraining())
                    {
                        return LogicDataTables.GetGlobals().GetSpellFactoryBoostMultiplier();
                    }

                    return LogicDataTables.GetGlobals().GetSpellFactoryBoostMultiplier();
                }

                if (unitProductionComponent.GetProductionType() == 0)
                {
                    if (LogicDataTables.GetGlobals().UseNewTraining())
                    {
                        return LogicDataTables.GetGlobals().GetBarracksBoostNewMultiplier();
                    }

                    return LogicDataTables.GetGlobals().GetBarracksBoostMultiplier();
                }
            }

            return 1;
        }

        public bool CanUnlock(bool canCallListener)
        {
            if (this.m_constructionTimer != null || this.m_upgLevel != 0 || !this.m_locked)
            {
                return false;
            }

            bool canUnlock = this.m_level.GetTownHallLevel(this.m_level.GetVillageType()) >= this.GetBuildingData().GetRequiredTownHallLevel(0);

            if (!canUnlock)
            {
                this.m_level.GetGameListener().TownHallLevelTooLow(this.GetRequiredTownHallLevelForUpgrade());
            }

            return canUnlock;
        }

        public bool CanUpgrade(bool canCallListener)
        {
            if (this.m_constructionTimer == null && !this.IsMaxUpgradeLevel())
            {
                if (this.m_level.GetTownHallLevel(this.m_villageType) >= this.GetRequiredTownHallLevelForUpgrade())
                {
                    return true;
                }

                if (canCallListener)
                {
                    this.m_level.GetGameListener().TownHallLevelTooLow(this.GetRequiredTownHallLevelForUpgrade());
                }
            }

            return false;
        }

        public bool CanSell()
        {
            return false;
        }

        public bool CanBeBoosted()
        {
            if (this.m_boostCooldownTimer != null && this.m_boostCooldownTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) > 0)
            {
                return false;
            }

            if (this.m_data.GetVillageType() == 1)
            {
                if (this.GetBuildingData().IsClockTower())
                {
                    return true;
                }
            }
            else
            {
                if (this.m_boostTimer == null)
                {
                    int maxBoostTime = this.GetMaxBoostTime();

                    if (maxBoostTime > 0)
                    {
                        return this.m_level.GetGameMode().GetCalendar().GetBuildingBoostCost(this.GetBuildingData(), this.m_upgLevel) > 0 || this.GetBuildingData().IsFreeBoost();
                    }
                }
            }

            return false;
        }

        public bool CanStopBoost()
        {
            return this.GetComponent(LogicComponentType.UNIT_PRODUCTION) != null || this.GetComponent(LogicComponentType.HERO_BASE) != null ||
                   this.GetBuildingData().IsClockTower();
        }

        public int GetBoostCost()
        {
            return this.m_level.GetGameMode().GetCalendar().GetBuildingBoostCost(this.GetBuildingData(), this.m_upgLevel);
        }

        public LogicResourceData GetSellResource()
        {
            return this.GetBuildingData().GetBuildResource(this.m_upgLevel);
        }

        public int GetSelectedWallTime()
        {
            return this.m_selectedWallTime;
        }

        public void StartSelectedWallTime()
        {
            int selectedWallTime = LogicDataTables.GetGlobals().GetSelectedWallTime();

            if (selectedWallTime > 0 && this.IsWall())
            {
                if (this.m_selectedWallTime == 0)
                {
                    this.m_selectedWallTime = selectedWallTime;
                    this.RefreshPassable();
                    this.m_level.GetTileMap().GetPathFinder().InvalidateCache();
                }

                this.m_selectedWallTime = selectedWallTime;
            }
        }

        public int GetHitWallDelay()
        {
            return this.m_hitWallDelay;
        }

        public void SetHitWallDelay(int time)
        {
            if (time > 0 && this.IsWall())
            {
                if (this.m_hitWallDelay == 0)
                {
                    this.m_hitWallDelay = time;
                    this.RefreshPassable();
                }

                this.m_hitWallDelay = time;
            }
        }

        public void OnSell()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar.IsClientAvatar())
            {
                if (this.GetComponent(LogicComponentType.RESOURCE_STORAGE) != null)
                {
                    this.EnableComponent(LogicComponentType.RESOURCE_STORAGE, false);
                    this.m_level.RefreshResourceCaps();
                }

                if (this.GetComponent(LogicComponentType.UNIT_STORAGE) != null)
                {
                    LogicUnitStorageComponent unitStorageComponent = (LogicUnitStorageComponent) this.GetComponent(LogicComponentType.UNIT_STORAGE);

                    for (int i = 0; i < unitStorageComponent.GetUnitTypeCount(); i++)
                    {
                        homeOwnerAvatar.CommodityCountChangeHelper(0, unitStorageComponent.GetUnitType(i), -unitStorageComponent.GetUnitCount(i));
                    }
                }
            }
        }

        public int GetSellPrice()
        {
            if (this.m_constructionTimer == null)
            {
                return this.GetBuildingData().GetBuildCost(this.m_upgLevel, this.m_level) / 5;
            }

            if (!this.m_gearing)
            {
                return this.GetBuildingData().GetBuildCost(this.m_upgLevel, this.m_level);
            }

            return this.GetBuildingData().GetBuildCost(this.m_upgLevel + 1, this.m_level) + this.GetBuildingData().GetBuildCost(this.m_upgLevel, this.m_level) / 5;
        }

        public bool SpeedUpConstruction()
        {
            if (this.m_constructionTimer != null)
            {
                LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime()), 0, this.m_villageType);

                if (playerAvatar.HasEnoughDiamonds(speedUpCost, true, this.m_level))
                {
                    playerAvatar.UseDiamonds(speedUpCost);
                    playerAvatar.GetChangeListener().DiamondPurchaseMade(0, this.m_data.GetGlobalID(), this.m_upgLevel + (this.m_upgrading ? 2 : 1), speedUpCost,
                                                                         this.m_level.GetVillageType());

                    this.FinishConstruction(false, true);

                    return true;
                }
            }

            return false;
        }

        public bool SpeedUpBoostCooldown()
        {
            if (this.m_boostCooldownTimer != null)
            {
                int cooldownSecs = this.m_boostCooldownTimer.GetRemainingSeconds(this.m_level.GetLogicTime());

                if (cooldownSecs > 0)
                {
                    LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                    int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(cooldownSecs, this.GetBuildingData().IsClockTower() ? 6 : 5, this.m_villageType);

                    if (playerAvatar.HasEnoughDiamonds(speedUpCost, true, this.m_level))
                    {
                        playerAvatar.UseDiamonds(speedUpCost);
                        playerAvatar.GetChangeListener().DiamondPurchaseMade(17, this.m_data.GetGlobalID(), this.m_upgLevel, speedUpCost, this.m_level.GetVillageType());

                        this.m_boostCooldownTimer.Destruct();
                        this.m_boostCooldownTimer = null;

                        this.Boost();

                        return true;
                    }
                }
            }

            return false;
        }

        public void StartConstructing(bool updateListener)
        {
            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            int constructionTime = this.GetBuildingData().GetConstructionTime(this.m_upgLevel, this.m_level, 1);

            if (constructionTime <= 0)
            {
                this.FinishConstruction(updateListener, updateListener);
            }
            else
            {
                this.m_constructionTimer = new LogicTimer();
                this.m_constructionTimer.StartTimer(constructionTime, this.m_level.GetLogicTime(), true, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());

                this.m_level.GetWorkerManagerAt(this.GetBuildingData().GetVillageType()).AllocateWorker(this);
            }

            if (this.m_villageType == 1 && this.m_locked)
            {
                // this.m_level.GetGameListener.???
            }
        }

        public void DestructBoost()
        {
            if (this.m_boostTimer != null)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;

                if (this.GetBuildingData().IsClockTower())
                {
                    this.m_boostCooldownTimer = new LogicTimer();
                    this.m_boostCooldownTimer.StartTimer(LogicDataTables.GetGlobals().GetClockTowerBoostCooldownSecs(), this.m_level.GetLogicTime(), false, -1);
                }
            }
        }

        public void StartUpgrading(bool updateListener, bool gearup)
        {
            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            this.DestructBoost();

            int constructionTime;

            if (gearup)
            {
                constructionTime = this.GetBuildingData().GetGearUpTime(this.m_upgLevel);
                this.m_gearing = true;
            }
            else
            {
                constructionTime = this.GetBuildingData().GetConstructionTime(this.m_upgLevel + 1, this.m_level, 0);
                this.m_upgrading = true;
            }

            if (constructionTime <= 0)
            {
                this.FinishConstruction(false, updateListener);
            }
            else
            {
                this.m_level.GetWorkerManagerAt(this.m_gearing ? 1 : this.GetBuildingData().GetVillageType()).AllocateWorker(this);

                if (this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION) != null)
                {
                    this.GetResourceProductionComponent().CollectResources(false);
                }

                this.EnableComponent(LogicComponentType.RESOURCE_PRODUCTION, false);
                this.EnableComponent(LogicComponentType.UNIT_PRODUCTION, false);
                this.EnableComponent(LogicComponentType.UNIT_UPGRADE, false);

                this.m_constructionTimer = new LogicTimer();
                this.m_constructionTimer.StartTimer(constructionTime, this.m_level.GetLogicTime(), true, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
            }
        }

        public void FinishConstruction(bool ignoreState, bool updateListener)
        {
            int state = this.m_level.GetState();

            if (state == 1 || !LogicDataTables.GetGlobals().CompleteConstructionOnlyHome() && ignoreState)
            {
                if (this.m_level.GetHomeOwnerAvatar() != null)
                {
                    if (this.m_level.GetHomeOwnerAvatar().IsClientAvatar())
                    {
                        LogicClientAvatar homeOwnerAvatar = (LogicClientAvatar) this.m_level.GetHomeOwnerAvatar();

                        if (this.m_constructionTimer != null)
                        {
                            this.m_constructionTimer.Destruct();
                            this.m_constructionTimer = null;
                        }

                        this.m_level.GetWorkerManagerAt(this.m_gearing ? 1 : this.GetBuildingData().GetVillageType()).DeallocateWorker(this);
                        this.m_locked = false;

                        if (this.m_gearing)
                        {
                            this.m_gear += 1;

                            LogicCombatComponent combatComponent = this.GetCombatComponent(false);

                            if (combatComponent != null)
                            {
                                combatComponent.ToggleAttackMode(this.m_level.GetActiveLayout(), false);
                            }
                        }
                        else
                        {
                            if (this.m_upgLevel != 0 || this.m_upgrading)
                            {
                                int newUpgLevel = this.m_upgLevel + 1;

                                if (this.m_upgLevel >= this.GetBuildingData().GetUpgradeLevelCount() - 1)
                                {
                                    Debugger.Warning("LogicBuilding - Trying to upgrade to level that doesn't exist! - " + this.GetBuildingData().GetName());
                                    newUpgLevel = this.GetBuildingData().GetUpgradeLevelCount() - 1;
                                }

                                int constructionTime = this.GetBuildingData().GetConstructionTime(newUpgLevel, this.m_level, 0);
                                int xpGain = LogicGamePlayUtil.TimeToExp(constructionTime);
                                this.SetUpgradeLevel(newUpgLevel);
                                this.XpGainHelper(xpGain, homeOwnerAvatar, ignoreState);
                            }
                            else
                            {
                                int constructionTime = this.GetBuildingData().GetConstructionTime(0, this.m_level, 0);
                                int xpGain = LogicGamePlayUtil.TimeToExp(constructionTime);
                                this.SetUpgradeLevel(this.m_upgLevel);
                                this.XpGainHelper(xpGain, homeOwnerAvatar, ignoreState);

                                LogicCombatComponent combatComponent = this.GetCombatComponent();

                                if (combatComponent != null)
                                {
                                    if (combatComponent.UseAmmo())
                                    {
                                        combatComponent.LoadAmmo();
                                    }
                                }

                                if (this.GetComponent(LogicComponentType.HERO_BASE) != null)
                                {
                                    LogicHeroData heroData = this.GetBuildingData().GetHeroData();

                                    if (heroData != null)
                                    {
                                        int heroState = heroData.HasNoDefence() ? 2 : 3;

                                        homeOwnerAvatar.SetHeroState(heroData, heroState);
                                        homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, heroData, heroState);
                                    }
                                    else
                                    {
                                        Debugger.Warning("No hero data in herobase/altar building");
                                    }
                                }
                            }
                        }

                        if (this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION) != null)
                        {
                            ((LogicResourceProductionComponent) this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION)).RestartTimer();
                        }

                        this.m_upgrading = false;
                        this.m_gearing = false;

                        if (this.m_listener != null)
                        {
                            this.m_listener.RefreshState();
                        }

                        if (state == 1)
                        {
                            this.m_level.GetAchievementManager().RefreshStatus();
                        }

                        LogicBuildingClassData buildingClassData = this.GetBuildingData().GetBuildingClass();

                        if (buildingClassData.IsTownHall() || buildingClassData.IsTownHall2())
                        {
                            this.m_level.RefreshNewShopUnlocksTH(this.m_data.GetVillageType());

                            if (buildingClassData.IsTownHall2())
                            {
                                this.m_level.GetGameObjectManagerAt(1).Village2TownHallFixed();
                            }
                        }

                        return;
                    }
                }

                Debugger.Warning("LogicBuilding::finishCostruction failed - Avatar is null or not client avatar");
            }
        }

        public void CancelConstruction()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null && homeOwnerAvatar.IsClientAvatar())
            {
                if (this.m_constructionTimer != null)
                {
                    this.m_constructionTimer.Destruct();
                    this.m_constructionTimer = null;

                    int upgLevel = this.m_upgLevel;

                    if (this.m_upgrading)
                    {
                        this.SetUpgradeLevel(this.m_upgLevel);
                        upgLevel += 1;
                    }

                    LogicBuildingData data = this.GetBuildingData();
                    LogicResourceData buildResourceData = data.GetBuildResource(upgLevel);

                    int buildCost = data.GetBuildCost(upgLevel, this.m_level);

                    if (this.m_gearing)
                    {
                        buildResourceData = data.GetGearUpResource();
                        buildCost = data.GetGearUpCost(this.m_upgLevel);
                    }

                    homeOwnerAvatar.CommodityCountChangeHelper(0, buildResourceData, LogicMath.Max(LogicDataTables.GetGlobals().GetBuildCancelMultiplier() * buildCost / 100, 0));

                    if (this.m_gearing)
                    {
                        this.m_level.GetWorkerManagerAt(1).DeallocateWorker(this);
                    }
                    else
                    {
                        this.m_level.GetWorkerManagerAt(this.m_data.GetVillageType()).DeallocateWorker(this);

                        if (upgLevel == 0)
                        {
                            this.GetGameObjectManager().RemoveGameObject(this);
                            return;
                        }
                    }

                    if (this.m_listener != null)
                    {
                        this.m_listener.RefreshState();
                    }
                }
            }
        }

        public void SetWallObjectId(int wallIdx, int wallBlockX, bool wallPoint)
        {
            if (!this.IsWall())
            {
                Debugger.Error("setWallObjectId called for non Wall");
            }

            this.m_wallIndex = wallIdx;
            this.m_wallPoint = wallPoint;
            this.m_wallBlockX = wallBlockX;
        }

        public void Boost()
        {
            if (this.m_boostTimer != null)
            {
                this.m_boostTimer.Destruct();
                this.m_boostTimer = null;
            }

            this.m_boostTimer = new LogicTimer();
            this.m_boostTimer.StartTimer(this.GetMaxBoostTime(), this.m_level.GetLogicTime(), false, -1);

            if (this.GetBuildingData().IsClockTower())
            {
                LogicGameListener gameListener = this.m_level.GetGameListener();

                if (gameListener != null)
                {
                    // Listener.
                }
            }
        }

        public void SetUpgradeLevel(int level)
        {
            LogicBuildingData buildingData = (LogicBuildingData) this.m_data;

            this.m_upgLevel = LogicMath.Clamp(level, 0, buildingData.GetUpgradeLevelCount() - 1);

            if (this.m_level.GetHomeOwnerAvatar() != null)
            {
                if (buildingData.IsAllianceCastle() && !this.m_locked)
                {
                    this.m_level.GetHomeOwnerAvatar().GetChangeListener().SetAllianceCastleLevel(this.m_upgLevel);
                    this.m_level.GetHomeOwnerAvatar().SetAllianceCastleLevel(this.m_upgLevel);

                    LogicBuilding building = this.m_level.GetGameObjectManagerAt(0).GetAllianceCastle();

                    if (building != null)
                    {
                        building.SetTreasurySize();
                    }
                }
                else if (buildingData.IsTownHall())
                {
                    this.m_level.GetHomeOwnerAvatar().GetChangeListener().SetTownHallLevel(this.m_upgLevel);
                    this.m_level.GetHomeOwnerAvatar().SetTownHallLevel(this.m_upgLevel);

                    LogicBuilding building = this.m_level.GetGameObjectManagerAt(0).GetAllianceCastle();

                    if (building != null)
                    {
                        building.SetTreasurySize();
                    }
                }
                else if (buildingData.IsTownHallVillage2())
                {
                    this.m_level.GetHomeOwnerAvatar().GetChangeListener().SetVillage2TownHallLevel(this.m_upgLevel);
                    this.m_level.GetHomeOwnerAvatar().SetVillage2TownHallLevel(this.m_upgLevel);
                }
                else if (buildingData.IsBarrackVillage2())
                {
                    this.m_level.GetHomeOwnerAvatar().SetVillage2BarrackLevel(this.m_upgLevel);
                }
            }

            if (this.m_upgLevel != 0 || this.m_upgrading || this.m_constructionTimer == null)
            {
                bool enable = this.m_constructionTimer == null;

                this.EnableComponent(LogicComponentType.UNIT_PRODUCTION, enable);
                this.EnableComponent(LogicComponentType.UNIT_UPGRADE, enable);
                this.EnableComponent(LogicComponentType.COMBAT, enable);
                this.EnableComponent(LogicComponentType.VILLAGE2_UNIT, enable);
                this.EnableComponent(LogicComponentType.RESOURCE_PRODUCTION, enable);

                LogicUnitStorageComponent unitStorageComponent = this.GetUnitStorageComponent();

                if (unitStorageComponent != null)
                {
                    unitStorageComponent.SetMaxCapacity(buildingData.GetUnitStorageCapacity(this.m_upgLevel));
                }

                LogicBunkerComponent bunkerComponent = this.GetBunkerComponent();

                if (bunkerComponent != null)
                {
                    bunkerComponent.SetMaxCapacity(buildingData.GetUnitStorageCapacity(this.m_upgLevel));
                }

                LogicDefenceUnitProductionComponent defenceUnitProductionComponent = this.GetDefenceUnitProduction();

                if (defenceUnitProductionComponent != null)
                {
                    defenceUnitProductionComponent.SetDefenceTroops(buildingData.GetDefenceTroopCharacter(this.m_upgLevel), buildingData.GetDefenceTroopCharacter2(this.m_upgLevel),
                                                     buildingData.GetDefenceTroopCount(this.m_upgLevel), buildingData.GetDefenceTroopLevel(this.m_upgLevel), 1);
                }

                LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                if (hitpointComponent != null)
                {
                    if (this.m_locked)
                    {
                        hitpointComponent.SetMaxHitpoints(0);
                        hitpointComponent.SetHitpoints(0);
                        hitpointComponent.SetMaxRegenerationTime(100);
                    }
                    else
                    {
                        hitpointComponent.SetMaxHitpoints(buildingData.GetHitpoints(this.m_upgLevel));
                        hitpointComponent.SetHitpoints(buildingData.GetHitpoints(this.m_upgLevel));
                        hitpointComponent.SetMaxRegenerationTime(buildingData.GetRegenerationTime(this.m_upgLevel));
                    }
                }

                LogicCombatComponent combatComponent = this.GetCombatComponent();
                LogicAttackerItemData attackerItemData = buildingData.GetAttackerItemData(this.m_upgLevel);

                if (buildingData.GetHeroData() != null && buildingData.GetShareHeroCombatData())
                {
                    this.m_shareHeroCombatData = buildingData.GetHeroData().GetAttackerItemData(0).Clone();
                    this.m_shareHeroCombatData.AddAttackRange(buildingData.GetWidth() * 72704 / 200);

                    attackerItemData = this.m_shareHeroCombatData;
                }

                if (combatComponent != null)
                {
                    combatComponent.SetAttackValues(attackerItemData, 100);
                }

                LogicResourceProductionComponent resourceProductionComponent = this.GetResourceProductionComponent();

                if (resourceProductionComponent != null)
                {
                    resourceProductionComponent.SetProduction(buildingData.GetResourcePer100Hours(this.m_upgLevel), buildingData.GetResourceMax(this.m_upgLevel));
                }

                LogicResourceStorageComponent resourceStorageComponent = this.GetResourceStorageComponentComponent();

                if (resourceStorageComponent != null)
                {
                    resourceStorageComponent.SetMaxArray(buildingData.GetMaxStoredResourceCounts(this.m_upgLevel));
                    resourceStorageComponent.SetMaxPercentageArray(buildingData.GetMaxPercentageStoredResourceCounts(this.m_upgLevel));
                }

                this.SetTreasurySize();
            }
        }

        public void SetTreasurySize()
        {
            LogicBuildingData data = this.GetBuildingData();
            LogicWarResourceStorageComponent component = this.GetWarResourceStorageComponent();

            if (data.IsAllianceCastle() && LogicDataTables.GetGlobals().TreasurySizeBasedOnTownHall())
            {
                LogicTownhallLevelData townhallLevelData = LogicDataTables.GetTownHallLevel(this.m_level.GetTownHallLevel(0));

                if (townhallLevelData != null)
                {
                    component.SetMaxArray(townhallLevelData.GetTreasuryCaps());
                    return;
                }
            }

            if (component != null)
            {
                component.SetMaxArray(data.GetMaxStoredResourceCounts(this.m_upgLevel));
            }
        }

        public override void LoadingFinished()
        {
            base.LoadingFinished();

            if (LogicDataTables.GetGlobals().ClampBuildingTimes())
            {
                if (this.m_constructionTimer != null)
                {
                    LogicBuildingData buildingData = this.GetBuildingData();

                    int remainingSecs = this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
                    int totalSecs = 0;

                    if (this.m_gearing)
                    {
                        totalSecs = buildingData.GetGearUpTime(this.m_upgLevel);
                    }
                    else
                    {
                        int upgLevel = this.m_upgrading ? this.m_upgLevel : -1;

                        if (upgLevel < buildingData.GetUpgradeLevelCount() - 1)
                        {
                            totalSecs = buildingData.GetConstructionTime(upgLevel + 1, this.m_level, 0);
                        }
                    }

                    if (remainingSecs > totalSecs)
                    {
                        this.m_constructionTimer.StartTimer(totalSecs, this.m_level.GetLogicTime(), true, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    }
                }
            }

            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar != null)
            {
                LogicBuildingData buildingData = this.GetBuildingData();

                if (buildingData.IsAllianceCastle() && !this.m_locked)
                {
                    if (homeOwnerAvatar.GetAllianceCastleLevel() != this.m_upgLevel)
                    {
                        this.m_level.GetHomeOwnerAvatar().GetChangeListener().SetAllianceCastleLevel(this.m_upgLevel);
                        this.m_level.GetHomeOwnerAvatar().SetAllianceCastleLevel(this.m_upgLevel);

                        LogicBuilding building = this.m_level.GetGameObjectManagerAt(0).GetAllianceCastle();

                        if (building != null)
                        {
                            building.SetTreasurySize();
                        }
                    }
                }
                else if (buildingData.IsTownHall())
                {
                    if (homeOwnerAvatar.GetTownHallLevel() != this.m_upgLevel)
                    {
                        this.m_level.GetHomeOwnerAvatar().GetChangeListener().SetTownHallLevel(this.m_upgLevel);
                        this.m_level.GetHomeOwnerAvatar().SetTownHallLevel(this.m_upgLevel);

                        LogicBuilding building = this.m_level.GetGameObjectManagerAt(0).GetAllianceCastle();

                        if (building != null)
                        {
                            building.SetTreasurySize();
                        }
                    }
                }
                else if (buildingData.IsTownHallVillage2())
                {
                    if (homeOwnerAvatar.GetVillage2TownHallLevel() != this.m_upgLevel)
                    {
                        this.m_level.GetHomeOwnerAvatar().GetChangeListener().SetVillage2TownHallLevel(this.m_upgLevel);
                        this.m_level.GetHomeOwnerAvatar().SetVillage2TownHallLevel(this.m_upgLevel);
                    }
                }
            }

            LogicCombatComponent combatComponent = this.GetCombatComponent();

            if (combatComponent != null)
            {
                if (combatComponent.GetAttackerItemData().GetTargetingConeAngle() <= 0)
                {
                    LogicBuilding townHall = this.GetGameObjectManager().GetTownHall();

                    if (townHall != null)
                    {
                        int distanceX = this.GetMidX() - townHall.GetMidX();
                        int distanceY = this.GetMidY() - townHall.GetMidY();

                        this.m_direction = 1000 * LogicMath.GetAngle(distanceX, distanceY);
                    }
                }
                else
                {
                    this.m_direction = 1000 * combatComponent.GetAimAngle(this.m_level.GetActiveLayout(this.m_level.GetVillageType()), false);
                }

                LogicBuildingData buildingData = this.GetBuildingData();
                LogicAttackerItemData attackerItemData = buildingData.GetAttackerItemData(this.m_upgLevel);
                LogicHeroData heroData = buildingData.GetHeroData();

                if (heroData != null && buildingData.GetShareHeroCombatData())
                {
                    int heroUpgLevel = homeOwnerAvatar.GetUnitUpgradeLevel(heroData);

                    this.m_shareHeroCombatData = heroData.GetAttackerItemData(heroUpgLevel).Clone();
                    this.m_shareHeroCombatData.AddAttackRange(buildingData.GetWidth() * 72704 / 200);

                    attackerItemData = this.m_shareHeroCombatData;

                    if (homeOwnerAvatar.IsHeroAvailableForAttack(heroData))
                    {
                        if (!this.m_locked && this.m_level.IsInCombatState())
                        {
                            LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                            hitpointComponent.SetMaxHitpoints(heroData.GetHitpoints(heroUpgLevel));
                            hitpointComponent.SetHitpoints(heroData.GetHitpoints(heroUpgLevel));
                            hitpointComponent.SetMaxRegenerationTime(buildingData.GetRegenerationTime(this.m_upgLevel));
                        }
                    }
                    else
                    {
                        combatComponent.SetEnabled(false);
                    }
                }

                combatComponent.SetAttackValues(attackerItemData, 100);
            }
        }

        public override void GetChecksum(ChecksumHelper checksum, bool includeGameObjects)
        {
            checksum.StartObject("LogicBuilding");

            base.GetChecksum(checksum, includeGameObjects);

            if (this.GetComponent(LogicComponentType.RESOURCE_STORAGE) != null)
            {
                this.GetComponent(LogicComponentType.RESOURCE_STORAGE).GetChecksum(checksum);
            }

            if (this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION) != null)
            {
                this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION).GetChecksum(checksum);
            }

            checksum.EndObject();
        }

        public override int GetDirection()
        {
            return this.m_direction / 1000;
        }

        public override bool IsHidden()
        {
            return this.m_hidden;
        }

        public void Trigger()
        {
            this.m_hidden = false;

            if (this.GetCombatComponent() != null && !this.m_hidden)
            {
                int attackRange = this.GetCombatComponent().GetAttackRange(0, false);

                LogicGameObjectManager gameObjectManager = this.GetGameObjectManager();
                LogicVector2 position = new LogicVector2(this.GetMidX(), this.GetMidY());

                LogicArrayList<LogicGameObject> gameObjects = gameObjectManager.GetGameObjects(LogicGameObjectType.CHARACTER);

                for (int i = 0; i < gameObjects.Size(); i++)
                {
                    LogicCharacter character = (LogicCharacter) gameObjects[i];
                    LogicCombatComponent combatComponent = character.GetCombatComponent();

                    int distanceSquared = position.GetDistanceSquared(character.GetPosition());

                    if (combatComponent != null && (distanceSquared < attackRange * attackRange ||
                                                    LogicCombatComponent.IsPreferredTarget(combatComponent.GetPreferredTarget(), this)))
                    {
                        LogicHitpointComponent hitpointComponent = character.GetHitpointComponent();

                        if (hitpointComponent != null && hitpointComponent.GetTeam() == 0)
                        {
                            combatComponent.ForceNewTarget();
                        }
                    }
                }

                position.Destruct();
            }
        }

        public override LogicGameObjectType GetGameObjectType()
        {
            return LogicGameObjectType.BUILDING;
        }
    }
}