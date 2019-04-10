namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicHeroBaseComponent : LogicComponent
    {
        public const int PATROL_PATHS = 8;

        private LogicTimer m_timer;
        private LogicArrayList<LogicVector2> m_patrolPath;

        private readonly LogicHeroData m_hero;

        private int m_healthTime;
        private int m_upgLevel;

        private bool m_sharedHeroCombatData;

        public LogicHeroBaseComponent(LogicGameObject gameObject, LogicHeroData data) : base(gameObject)
        {
            this.m_hero = data;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }
        }

        public override void Tick()
        {
            this.m_healthTime += 64;

            int regenTime = 1000;

            if (this.m_parent.GetRemainingBoostTime() > 0 && !this.m_parent.IsBoostPaused())
            {
                regenTime /= LogicDataTables.GetGlobals().GetHeroRestBoostMultiplier();
            }

            if (this.m_parent.GetLevel().GetRemainingClockTowerBoostTime() > 0)
            {
                LogicGameObjectData data = this.m_parent.GetData();

                if (data.GetDataType() == LogicDataType.BUILDING && data.GetVillageType() == 1)
                {
                    regenTime /= LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier();

                    if (this.m_timer != null)
                    {
                        this.m_timer.SetFastForward(this.m_timer.GetFastForward() + 4 * LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 4);
                    }
                }
            }

            if (this.m_healthTime > regenTime)
            {
                if (this.m_parent.GetLevel().GetPlayerAvatar().FastForwardHeroHealth(this.m_hero, 1) && this.GetParentListener() != null)
                {
                    // LOAD EFFECT.
                }

                this.m_healthTime -= regenTime;
            }

            if (this.m_timer != null)
            {
                if (this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) == 0)
                {
                    this.FinishUpgrading(true);
                }
            }
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.HERO_BASE;
        }

        public void FinishUpgrading(bool tick)
        {
            if (this.m_timer != null)
            {
                LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

                if (homeOwnerAvatar.GetUnitUpgradeLevel(this.m_hero) < this.m_upgLevel || this.m_upgLevel == 0)
                {
                    homeOwnerAvatar.CommodityCountChangeHelper(1, this.m_hero, 1);
                }

                this.m_parent.GetLevel().GetWorkerManagerAt(this.m_parent.GetData().GetVillageType()).DeallocateWorker(this.m_parent);

                homeOwnerAvatar.SetHeroState(this.m_hero, 3);
                homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, 3);

                this.SetFullHealth();

                this.m_timer.Destruct();
                this.m_timer = null;
            }
            else
            {
                Debugger.Warning("LogicHeroBaseComponent::finishUpgrading called and m_pHero is NULL");
            }
        }

        public bool IsUpgrading()
        {
            return this.m_timer != null;
        }

        public void SetFullHealth()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            homeOwnerAvatar.SetHeroHealth(this.m_hero, 0);
            homeOwnerAvatar.GetChangeListener().CommodityCountChanged(0, this.m_hero, 0);
        }

        public int GetRemainingUpgradeSeconds()
        {
            if (this.m_timer != null)
            {
                return this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetRemainingMS()
        {
            if (this.m_timer != null)
            {
                return this.m_timer.GetRemainingMS(this.m_parent.GetLevel().GetLogicTime());
            }

            return 0;
        }

        public int GetTotalSeconds()
        {
            if (this.m_timer != null)
            {
                return this.m_hero.GetUpgradeTime(this.m_parent.GetLevel().GetHomeOwnerAvatar().GetUnitUpgradeLevel(this.m_hero));
            }

            return 0;
        }

        public LogicHeroData GetHeroData()
        {
            return this.m_hero;
        }

        public void SetSharedHeroCombatData(bool value)
        {
            this.m_sharedHeroCombatData = value;
        }

        public override void Save(LogicJSONObject root, int villageType)
        {
            if (this.m_timer != null && this.m_hero != null)
            {
                LogicJSONObject jsonObject = new LogicJSONObject();

                jsonObject.Put("level", new LogicJSONNumber(this.m_upgLevel));
                jsonObject.Put("t", new LogicJSONNumber(this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime())));

                if (this.m_timer.GetEndTimestamp() != -1)
                {
                    jsonObject.Put("t_end", new LogicJSONNumber(this.m_timer.GetEndTimestamp()));
                }

                if (this.m_timer.GetFastForward() > 0)
                {
                    jsonObject.Put("t_ff", new LogicJSONNumber(this.m_timer.GetFastForward()));
                }

                root.Put("hero_upg", jsonObject);
            }
        }

        public override void SaveToSnapshot(LogicJSONObject root, int layoutId)
        {
            if (this.m_timer != null && this.m_hero != null)
            {
                LogicJSONObject jsonObject = new LogicJSONObject();

                jsonObject.Put("level", new LogicJSONNumber(this.m_upgLevel));
                jsonObject.Put("t", new LogicJSONNumber(this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime())));

                root.Put("hero_upg", jsonObject);
            }
        }

        public override void Load(LogicJSONObject root)
        {
            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            LogicJSONObject jsonObject = root.GetJSONObject("hero_upg");

            if (jsonObject != null)
            {
                LogicJSONNumber levelObject = jsonObject.GetJSONNumber("level");
                LogicJSONNumber timerObject = jsonObject.GetJSONNumber("t");
                LogicJSONNumber timerEndObject = jsonObject.GetJSONNumber("t_end");
                LogicJSONNumber timerFastForwardObject = jsonObject.GetJSONNumber("t_ff");

                if (levelObject != null)
                {
                    this.m_upgLevel = levelObject.GetIntValue();
                }

                if (timerObject != null)
                {
                    this.m_timer = new LogicTimer();
                    this.m_timer.StartTimer(timerObject.GetIntValue(), this.m_parent.GetLevel().GetLogicTime(), false, -1);

                    if (timerEndObject != null)
                    {
                        this.m_timer.SetEndTimestamp(timerEndObject.GetIntValue());
                    }

                    if (timerFastForwardObject != null)
                    {
                        this.m_timer.SetFastForward(timerFastForwardObject.GetIntValue());
                    }

                    this.m_parent.GetLevel().GetWorkerManagerAt(this.m_parent.GetVillageType()).AllocateWorker(this.m_parent);
                }
            }
        }

        public override void LoadFromSnapshot(LogicJSONObject root)
        {
            if (this.m_timer != null)
            {
                this.m_timer.Destruct();
                this.m_timer = null;
            }

            LogicJSONObject jsonObject = root.GetJSONObject("hero_upg");

            if (jsonObject != null)
            {
                LogicJSONNumber levelObject = jsonObject.GetJSONNumber("level");

                if (levelObject != null)
                {
                    this.m_upgLevel = levelObject.GetIntValue();
                }
            }
        }

        public override void LoadingFinished()
        {
            if (this.m_parent.GetLevel().IsInCombatState())
            {
                if (this.m_parent.GetVillageType() == this.m_parent.GetLevel().GetVillageType())
                {
                    if (this.m_parent.GetLevel().GetVillageType() == this.m_parent.GetVillageType())
                    {
                        this.m_patrolPath = this.CreatePatrolPath();
                    }
                }
            }

            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
            LogicBuilding building = (LogicBuilding) this.m_parent;

            if (!building.IsLocked() && homeOwnerAvatar.GetHeroState(this.m_hero) == 0)
            {
                homeOwnerAvatar.SetHeroState(this.m_hero, 3);
                homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, 3);
            }

            if (this.m_timer != null)
            {
                int remainingSecs = this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
                int totalSecs = this.GetTotalSeconds();

                if (LogicDataTables.GetGlobals().ClampUpgradeTimes())
                {
                    if (remainingSecs > totalSecs)
                    {
                        this.m_timer.StartTimer(totalSecs, this.m_parent.GetLevel().GetLogicTime(), true,
                                                this.m_parent.GetLevel().GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                    }
                }
                else
                {
                    this.m_timer.StartTimer(LogicMath.Min(remainingSecs, totalSecs), this.m_parent.GetLevel().GetLogicTime(), false, -1);
                }

                if (!building.IsLocked() && homeOwnerAvatar.GetHeroState(this.m_hero) != 1)
                {
                    homeOwnerAvatar.SetHeroState(this.m_hero, 1);
                    homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, 1);
                }
            }
            else
            {
                if (!building.IsLocked() && homeOwnerAvatar.GetHeroState(this.m_hero) == 1)
                {
                    homeOwnerAvatar.SetHeroState(this.m_hero, 3);
                    homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, 3);
                }
            }

            if (this.m_hero.HasNoDefence() && !this.m_parent.GetLevel().IsInCombatState() && homeOwnerAvatar.GetHeroState(this.m_hero) == 3)
            {
                homeOwnerAvatar.SetHeroState(this.m_hero, 2);
                homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, 2);
            }

            if (homeOwnerAvatar.GetHeroState(this.m_hero) == 3)
            {
                if (this.m_parent.GetLevel().IsInCombatState())
                {
                    if (!this.m_sharedHeroCombatData && !this.m_hero.HasNoDefence())
                    {
                        if (this.m_parent.GetVillageType() == this.m_parent.GetLevel().GetVillageType())
                        {
                            this.AddDefendingHero();
                        }
                    }
                }
            }

            int heroHealth = homeOwnerAvatar.GetHeroHealth(this.m_hero);
            int fullRegenerationTime = this.m_hero.GetFullRegenerationTimeSec(homeOwnerAvatar.GetUnitUpgradeLevel(this.m_hero));

            if (fullRegenerationTime < heroHealth)
            {
                homeOwnerAvatar.GetChangeListener().CommodityCountChanged(0, this.m_hero, fullRegenerationTime);
                homeOwnerAvatar.SetHeroHealth(this.m_hero, fullRegenerationTime);
            }
        }

        public override void FastForwardTime(int time)
        {
            int heroHealthTime = time;
            int constructionBoostTime = 0;
            int remainingBoostTime = this.m_parent.GetRemainingBoostTime();

            if (remainingBoostTime > 0)
            {
                if (!this.m_parent.IsBoostPaused())
                {
                    heroHealthTime += LogicMath.Min(remainingBoostTime, time) * (LogicDataTables.GetGlobals().GetHeroRestBoostMultiplier() - 1);
                }
            }

            int clockTowerBoostTime = this.m_parent.GetLevel().GetUpdatedClockTowerBoostTime();

            if (clockTowerBoostTime > 0 && this.m_parent.GetLevel().IsClockTowerBoostPaused())
            {
                LogicGameObjectData data = this.m_parent.GetData();

                if (data.GetDataType() == LogicDataType.BUILDING && data.GetVillageType() == 1)
                {
                    int boost = LogicMath.Min(clockTowerBoostTime, time) * (LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 1);

                    heroHealthTime += boost;
                    constructionBoostTime += boost;
                }
            }

            this.m_parent.GetLevel().GetHomeOwnerAvatar().FastForwardHeroHealth(this.m_hero, heroHealthTime);

            if (this.m_timer != null)
            {
                if (this.m_timer.GetEndTimestamp() == -1)
                {
                    this.m_timer.StartTimer(this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) - time, this.m_parent.GetLevel().GetLogicTime(), false, -1);
                }
                else
                {
                    this.m_timer.AdjustEndSubtick(this.m_parent.GetLevel());
                }

                if (constructionBoostTime > 0)
                {
                    this.m_timer.SetFastForward(this.m_timer.GetFastForward() + 60 * constructionBoostTime);
                }
            }
        }

        public LogicArrayList<LogicVector2> GetPatrolPath()
        {
            return this.m_patrolPath;
        }

        public LogicArrayList<LogicVector2> CreatePatrolPath()
        {
            int parentWidth = this.m_parent.GetWidthInTiles() << 8;
            int parentHeight = this.m_parent.GetHeightInTiles() << 8;
            int patrolRadius = this.m_hero.GetPatrolRadius();

            if (patrolRadius * patrolRadius >= parentWidth * parentWidth + parentHeight * parentHeight)
            {
                LogicVector2 tmp1 = new LogicVector2();
                LogicVector2 tmp2 = new LogicVector2();
                LogicVector2 tmp3 = new LogicVector2();
                LogicVector2 tmp4 = new LogicVector2();

                int parentMidX = this.m_parent.GetMidX();
                int parentMidY = this.m_parent.GetMidY();

                tmp2.Set(parentMidX, parentMidY);

                LogicArrayList<LogicVector2> wayPoints = new LogicArrayList<LogicVector2>(LogicHeroBaseComponent.PATROL_PATHS);

                for (int i = 0, j = 22; i < LogicHeroBaseComponent.PATROL_PATHS; i++, j += 45)
                {
                    tmp1.Set(parentMidX + LogicMath.Cos(j, patrolRadius), parentMidY + LogicMath.Sin(j, patrolRadius));
                    LogicHeroBaseComponent.FindPoint(this.m_parent.GetLevel().GetTileMap(), tmp3, tmp2, tmp1, tmp4);
                    wayPoints.Add(new LogicVector2(tmp4.m_x, tmp4.m_y));
                }

                tmp1.Destruct();
                tmp2.Destruct();
                tmp3.Destruct();
                tmp4.Destruct();

                return wayPoints;
            }
            else
            {
                int startX = this.m_parent.GetX() + (this.m_parent.GetWidthInTiles() << 9) - 128;
                int startY = this.m_parent.GetY() + (this.m_parent.GetWidthInTiles() << 9) - 128;
                int endX = this.m_parent.GetX() + 128;
                int endY = this.m_parent.GetY() + 128;

                LogicArrayList<LogicVector2> wayPoints = new LogicArrayList<LogicVector2>(4);

                wayPoints.Add(new LogicVector2(startX, startY));
                wayPoints.Add(new LogicVector2(endX, startY));
                wayPoints.Add(new LogicVector2(endX, endY));
                wayPoints.Add(new LogicVector2(startX, endY));

                return wayPoints;
            }
        }

        public void AddDefendingHero()
        {
            LogicAvatar visitorAvatar = this.m_parent.GetLevel().GetVisitorAvatar();
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            int randomPatrolPoint = visitorAvatar != null
                ? (int) (((visitorAvatar.GetResourceCount(LogicDataTables.GetGoldData()) + 10 * this.m_hero.GetGlobalID()) & 0x7FFFFFFFu) % this.m_patrolPath.Size())
                : 0;
            int upgLevel = homeOwnerAvatar.GetUnitUpgradeLevel(this.m_hero);
            int heroHitpoints = this.m_hero.GetHeroHitpoints(homeOwnerAvatar.GetHeroHealth(this.m_hero), upgLevel);

            if (this.m_hero.HasEnoughHealthForAttack(heroHitpoints, upgLevel))
            {
                LogicVector2 patrolPoint = this.m_patrolPath[randomPatrolPoint];
                LogicCharacter hero = (LogicCharacter) LogicGameObjectFactory.CreateGameObject(this.m_hero, this.m_parent.GetLevel(), this.m_parent.GetVillageType());

                hero.GetMovementComponent().SetBaseBuilding((LogicBuilding) this.m_parent);
                hero.GetHitpointComponent().SetTeam(1);
                hero.SetUpgradeLevel(upgLevel);
                hero.GetHitpointComponent().SetHitpoints(heroHitpoints);

                hero.SetInitialPosition(patrolPoint.m_x, patrolPoint.m_y);

                this.m_parent.GetGameObjectManager().AddGameObject(hero, -1);

                hero.GetCombatComponent().SetSearchRadius(this.m_hero.GetMaxSearchRadiusForDefender() / 512);

                if (LogicDataTables.GetGlobals().EnableDefendingAllianceTroopJump())
                {
                    hero.GetMovementComponent().EnableJump(3600000);
                }
            }
        }

        public bool SpeedUp()
        {
            if (this.m_timer != null)
            {
                int remainingSecs = this.m_timer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
                int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(remainingSecs, 0, this.m_parent.GetVillageType());

                LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

                if (homeOwnerAvatar.IsClientAvatar())
                {
                    LogicClientAvatar clientAvatar = (LogicClientAvatar) homeOwnerAvatar;

                    if (clientAvatar.HasEnoughDiamonds(speedUpCost, true, this.m_parent.GetLevel()))
                    {
                        clientAvatar.UseDiamonds(speedUpCost);
                        clientAvatar.GetChangeListener().DiamondPurchaseMade(10, this.m_hero.GetGlobalID(), clientAvatar.GetUnitUpgradeLevel(this.m_hero) + 1, speedUpCost,
                                                                             this.m_parent.GetLevel().GetVillageType());
                        this.FinishUpgrading(true);

                        return true;
                    }
                }
            }

            return false;
        }

        public void CancelUpgrade()
        {
            if (this.m_timer != null)
            {
                LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
                int upgradeLevel = homeOwnerAvatar.GetUnitUpgradeLevel(this.m_hero);
                int upgradeCost = this.m_hero.GetUpgradeCost(upgradeLevel);
                LogicResourceData upgradeResourceData = this.m_hero.GetUpgradeResource(upgradeLevel);

                homeOwnerAvatar.CommodityCountChangeHelper(0, upgradeResourceData, LogicDataTables.GetGlobals().GetHeroUpgradeCancelMultiplier() * upgradeCost / 100);

                this.m_parent.GetLevel().GetWorkerManagerAt(this.m_parent.GetData().GetVillageType()).DeallocateWorker(this.m_parent);

                homeOwnerAvatar.SetHeroState(this.m_hero, 3);
                homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, 3);

                this.m_timer.Destruct();
                this.m_timer = null;
            }
            else
            {
                Debugger.Warning("LogicHeroBaseComponent::cancelUpgrade called even upgrade is not on going!");
            }
        }

        public bool CanStartUpgrading(bool callListener)
        {
            if (this.m_timer == null)
            {
                if (!this.IsMaxLevel())
                {
                    LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

                    int requiredTownHallLevel = this.m_hero.GetRequiredTownHallLevel(homeOwnerAvatar.GetUnitUpgradeLevel(this.m_hero) + 1);
                    int townHallLevel = this.m_parent.GetLevel().GetTownHallLevel(this.m_parent.GetLevel().GetVillageType());

                    if (townHallLevel >= requiredTownHallLevel)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void StartUpgrading()
        {
            if (this.CanStartUpgrading(true))
            {
                ((LogicBuilding) this.m_parent).DestructBoost();

                if (this.m_timer != null)
                {
                    this.m_timer.Destruct();
                    this.m_timer = null;
                }

                LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

                this.m_parent.GetLevel().GetWorkerManagerAt(this.m_parent.GetData().GetVillageType()).AllocateWorker(this.m_parent);

                this.m_timer = new LogicTimer();
                this.m_timer.StartTimer(this.GetTotalSeconds(), this.m_parent.GetLevel().GetLogicTime(), true,
                                        this.m_parent.GetLevel().GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
                this.m_upgLevel = homeOwnerAvatar.GetUnitUpgradeLevel(this.m_hero) + 1;

                homeOwnerAvatar.SetHeroState(this.m_hero, 1);
                homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, 1);
            }
        }

        public bool IsMaxLevel()
        {
            return this.m_parent.GetLevel().GetHomeOwnerAvatar().GetUnitUpgradeLevel(this.m_hero) >= this.m_hero.GetUpgradeLevelCount() - 1;
        }

        public int GetSpeedUpHealthCost()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (homeOwnerAvatar.IsClientAvatar())
            {
                return homeOwnerAvatar.GetHeroHealCost(this.m_hero);
            }

            return 0;
        }

        public bool SpeedUpHealth()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (homeOwnerAvatar.IsClientAvatar())
            {
                LogicClientAvatar clientAvatar = (LogicClientAvatar) homeOwnerAvatar;
                int speedUpCost = this.GetSpeedUpHealthCost();

                if (clientAvatar.HasEnoughDiamonds(speedUpCost, true, this.m_parent.GetLevel()))
                {
                    clientAvatar.UseDiamonds(speedUpCost);
                    clientAvatar.GetChangeListener().DiamondPurchaseMade(9, this.m_hero.GetGlobalID(), clientAvatar.GetUnitUpgradeLevel(this.m_hero) + 1, speedUpCost,
                                                                         this.m_parent.GetLevel().GetVillageType());

                    this.SetFullHealth();

                    return true;
                }
            }

            return false;
        }

        public bool SetSleep(bool enabled)
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
            int state = homeOwnerAvatar.GetHeroState(this.m_hero);

            if (state != 0)
            {
                int newState = enabled ? 2 : 3;

                if (state != newState)
                {
                    homeOwnerAvatar.SetHeroState(this.m_hero, newState);
                    homeOwnerAvatar.GetChangeListener().CommodityCountChanged(2, this.m_hero, newState);

                    return true;
                }
            }

            return false;
        }

        public bool SetHeroMode(int mode)
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (homeOwnerAvatar.GetHeroMode(this.m_hero) == mode)
            {
                return false;
            }

            homeOwnerAvatar.SetHeroMode(this.m_hero, mode);
            homeOwnerAvatar.GetChangeListener().CommodityCountChanged(3, this.m_hero, mode);

            return true;
        }

        public static bool FindPoint(LogicTileMap tileMap, LogicVector2 pos1, LogicVector2 pos2, LogicVector2 pos3, LogicVector2 pos4)
        {
            pos1.Set(pos2.m_x, pos2.m_y);
            pos1.Substract(pos3);

            int length = pos1.GetLength();

            pos1.m_x = (pos1.m_x << 7) / length;
            pos1.m_y = (pos1.m_y << 7) / length;

            pos4.Set(pos3.m_x, pos3.m_y);

            int radius = LogicMath.Clamp(length / 128, 10, 25);

            for (int i = 0; i < radius; i++)
            {
                if (tileMap.IsPassablePathFinder(pos4.m_x >> 8, pos4.m_y >> 8))
                {
                    pos4.m_x = (int) ((pos4.m_x & 0xFFFFFF00) | 128);
                    pos4.m_y = (int) ((pos4.m_y & 0xFFFFFF00) | 128);

                    return true;
                }

                pos4.Add(pos1);
            }

            return false;
        }
    }
}