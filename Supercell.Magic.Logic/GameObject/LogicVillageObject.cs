namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicVillageObject : LogicGameObject
    {
        private LogicTimer m_constructionTimer;

        private bool m_locked;
        private bool m_upgrading;
        private int m_upgLevel;

        public LogicVillageObject(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            // LogicVillageObject.
        }

        public LogicVillageObjectData GetVillageObjectData()
        {
            return (LogicVillageObjectData) this.m_data;
        }

        public bool CanUpgrade(bool callListener)
        {
            if (!this.m_locked)
            {
                LogicVillageObjectData villageObjectData = this.GetVillageObjectData();

                if (this.m_upgLevel < villageObjectData.GetUpgradeLevelCount() - 1)
                {
                    if (this.m_level.GetTownHallLevel(this.m_level.GetVillageType()) >= this.GetRequiredTownHallLevelForUpgrade())
                    {
                        return true;
                    }

                    if (callListener)
                    {
                        this.m_level.GetGameListener().TownHallLevelTooLow(this.GetRequiredTownHallLevelForUpgrade());
                    }

                    return false;
                }
            }

            return false;
        }

        public void StartUpgrading(bool ignoreListener)
        {
            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            LogicVillageObjectData data = this.GetVillageObjectData();
            int constructionTime = data.GetBuildTime(this.GetUpgradeLevel() + 1);

            this.m_upgrading = true;

            if (constructionTime <= 0)
            {
                this.FinishConstruction(false, true);
            }
            else
            {
                if (data.IsRequiresBuilder())
                {
                    this.m_level.GetWorkerManagerAt(data.GetVillageType()).AllocateWorker(this);
                }

                this.EnableComponent(LogicComponentType.RESOURCE_PRODUCTION, false);
                this.EnableComponent(LogicComponentType.UNIT_PRODUCTION, false);
                this.EnableComponent(LogicComponentType.UNIT_UPGRADE, false);

                this.m_constructionTimer = new LogicTimer();
                this.m_constructionTimer.StartTimer(constructionTime, this.m_level.GetLogicTime(), true, this.m_level.GetHomeOwnerAvatarChangeListener().GetCurrentTimestamp());
            }
        }

        public void FinishConstruction(bool ignoreState, bool ignoreListener)
        {
            int state = this.m_level.GetState();

            if (state == 1 || !LogicDataTables.GetGlobals().CompleteConstructionOnlyHome() && ignoreState)
            {
                if (this.m_level.GetHomeOwnerAvatar() != null &&
                    this.m_level.GetHomeOwnerAvatar().IsClientAvatar())
                {
                    if (this.m_constructionTimer != null)
                    {
                        this.m_constructionTimer.Destruct();
                        this.m_constructionTimer = null;
                    }

                    LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
                    LogicVillageObjectData data = this.GetVillageObjectData();

                    if (data.IsRequiresBuilder())
                    {
                        this.m_level.GetWorkerManagerAt(data.GetVillageType()).DeallocateWorker(this);
                    }

                    this.m_locked = false;

                    if (this.m_upgLevel != 0 || this.m_upgrading)
                    {
                        int nextUpgLevel = this.m_upgLevel + 1;

                        if (this.m_upgLevel >= data.GetUpgradeLevelCount() - 1)
                        {
                            Debugger.Warning("LogicVillageObject - Trying to upgrade to level that doesn't exist! - " + data.GetName());
                            nextUpgLevel = data.GetUpgradeLevelCount() - 1;
                        }

                        int constructionTime = data.GetBuildTime(nextUpgLevel);
                        int xpGain = LogicMath.Sqrt(constructionTime);

                        this.m_upgLevel = nextUpgLevel;

                        this.XpGainHelper(xpGain, homeOwnerAvatar, ignoreState);
                    }
                    else
                    {
                        int constructionTime = data.GetBuildTime(0);
                        int xpGain = LogicMath.Sqrt(constructionTime);

                        this.XpGainHelper(xpGain, homeOwnerAvatar, ignoreState);
                    }

                    this.m_upgrading = false;

                    if (this.m_listener != null)
                    {
                        this.m_listener.RefreshState();
                    }

                    if (state == 1)
                    {
                        this.m_level.GetAchievementManager().RefreshStatus();
                    }
                }
                else
                {
                    Debugger.Error("LogicVillageObject::finishCostruction failed - Avatar is null or not client avatar");
                }
            }
        }

        public bool SpeedUpCostruction()
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

        public int GetRequiredTownHallLevelForUpgrade()
        {
            return this.GetVillageObjectData().GetRequiredTownHallLevel(LogicMath.Min(this.m_upgLevel + 1, this.GetVillageObjectData().GetUpgradeLevelCount() - 1));
        }

        public int GetRemainingConstructionTime()
        {
            if (this.m_constructionTimer != null)
            {
                return this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
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

        public override void SubTick()
        {
            LogicCombatComponent combatComponent = this.GetCombatComponent(false);

            if (combatComponent != null)
            {
                combatComponent.SubTick();
            }

            if (this.m_constructionTimer != null)
            {
                if (this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) <= 0)
                {
                    this.FinishConstruction(false, true);
                }
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

            if (this.m_constructionTimer != null)
            {
                jsonObject.Put("const_t", new LogicJSONNumber(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));

                if (this.m_constructionTimer.GetEndTimestamp() != -1)
                {
                    jsonObject.Put("const_t_end", new LogicJSONNumber(this.m_constructionTimer.GetEndTimestamp()));
                }

                if (this.m_constructionTimer.GetFastForward() != -1)
                {
                    jsonObject.Put("con_ff", new LogicJSONNumber(this.m_constructionTimer.GetFastForward()));
                }
            }

            if (this.m_locked)
            {
                jsonObject.Put("locked", new LogicJSONBoolean(true));
            }

            base.Save(jsonObject, villageType);
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            jsonObject.Put("x", new LogicJSONNumber(this.GetTileX() & 63));
            jsonObject.Put("y", new LogicJSONNumber(this.GetTileY() & 63));

            if (this.m_upgLevel != 0 || this.m_constructionTimer == null || this.m_upgrading)
            {
                jsonObject.Put("lvl", new LogicJSONNumber(this.m_upgLevel));
            }
            else
            {
                jsonObject.Put("lvl", new LogicJSONNumber(-1));
            }

            if (this.m_constructionTimer != null)
            {
                jsonObject.Put("const_t", new LogicJSONNumber(this.m_constructionTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));
            }

            base.SaveToSnapshot(jsonObject, layoutId);
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            this.LoadUpgradeLevel(jsonObject);

            if (this.m_constructionTimer != null)
            {
                this.m_constructionTimer.Destruct();
                this.m_constructionTimer = null;
            }

            LogicJSONNumber constTimeObject = jsonObject.GetJSONNumber("const_t");

            if (constTimeObject != null)
            {
                int constTime = constTimeObject.GetIntValue();

                if (!LogicDataTables.GetGlobals().ClampBuildingTimes())
                {
                    if (this.m_upgLevel < this.GetVillageObjectData().GetUpgradeLevelCount() - 1)
                    {
                        constTime = LogicMath.Min(constTime, this.GetVillageObjectData().GetBuildTime(this.m_upgLevel + 1));
                    }
                }

                this.m_constructionTimer = new LogicTimer();
                this.m_constructionTimer.StartTimer(constTime, this.m_level.GetLogicTime(), false, -1);

                LogicJSONNumber constTimeEndObject = jsonObject.GetJSONNumber("const_t_end");

                if (constTimeEndObject != null)
                {
                    this.m_constructionTimer.SetEndTimestamp(constTimeEndObject.GetIntValue());
                }

                LogicJSONNumber conffObject = jsonObject.GetJSONNumber("con_ff");

                if (conffObject != null)
                {
                    this.m_constructionTimer.SetFastForward(conffObject.GetIntValue());
                }

                LogicVillageObjectData villageObjectData = this.GetVillageObjectData();

                if (villageObjectData.IsRequiresBuilder() && !villageObjectData.IsAutomaticUpgrades())
                {
                    this.m_level.GetWorkerManagerAt(this.m_villageType).AllocateWorker(this);
                }

                this.m_upgrading = this.m_upgLevel != -1;
            }

            this.m_upgLevel = LogicMath.Clamp(this.m_upgLevel, 0, this.GetVillageObjectData().GetUpgradeLevelCount() - 1);

            base.Load(jsonObject);

            this.SetPositionXY((this.GetVillageObjectData().GetTileX100() << 9) / 100,
                               (this.GetVillageObjectData().GetTileY100() << 9) / 100);
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            this.LoadUpgradeLevel(jsonObject);
            base.LoadFromSnapshot(jsonObject);

            this.SetPositionXY((this.GetVillageObjectData().GetTileX100() << 9) / 100,
                               (this.GetVillageObjectData().GetTileY100() << 9) / 100);
        }

        public void LoadUpgradeLevel(LogicJSONObject jsonObject)
        {
            LogicJSONNumber lvlObject = jsonObject.GetJSONNumber("lvl");

            if (lvlObject != null)
            {
                this.m_upgLevel = lvlObject.GetIntValue();
                int maxLvl = this.GetVillageObjectData().GetUpgradeLevelCount();

                if (this.m_upgLevel >= maxLvl)
                {
                    Debugger.Warning(string.Format("LogicVillageObject::load() - Loaded upgrade level {0} is over max! (max = {1}) id {2} data id {3}",
                                                   lvlObject.GetIntValue(),
                                                   maxLvl,
                                                   this.m_globalId,
                                                   this.m_data.GetGlobalID()));
                    this.m_upgLevel = maxLvl - 1;
                }
                else
                {
                    if (this.m_upgLevel < -1)
                    {
                        Debugger.Error("LogicVillageObject::load() - Loaded an illegal upgrade level!");
                    }
                }
            }
            else
            {
                Debugger.Error("LogicVillageObject::load - Upgrade level was not found!");
            }

            if (this.GetVillageObjectData().IsRequiresBuilder())
            {
                this.m_level.GetWorkerManagerAt(this.m_villageType).DeallocateWorker(this);
            }

            LogicJSONBoolean lockedObject = jsonObject.GetJSONBoolean("locked");

            if (lockedObject != null)
            {
                this.m_locked = lockedObject.IsTrue();
            }
        }

        public override void LoadingFinished()
        {
            base.LoadingFinished();

            if (this.m_listener != null)
            {
                this.m_listener.LoadedFromJSON();
            }
        }

        public override LogicGameObjectType GetGameObjectType()
        {
            return LogicGameObjectType.VILLAGE_OBJECT;
        }

        public override int GetWidthInTiles()
        {
            return 0;
        }

        public override int GetHeightInTiles()
        {
            return 0;
        }

        public int GetUpgradeLevel()
        {
            return this.m_upgLevel;
        }

        public void SetUpgradeLevel(int upgLevel)
        {
            this.m_upgLevel = upgLevel;
        }

        public bool IsConstructing()
        {
            return this.m_constructionTimer != null;
        }
    }
}