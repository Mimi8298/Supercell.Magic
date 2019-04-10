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
    using Supercell.Magic.Titan.Util;

    public sealed class LogicObstacle : LogicGameObject
    {
        private LogicTimer m_clearTimer;

        private int m_lootMultiplyVersion;
        private int m_fadeTime;

        public LogicObstacle(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            LogicObstacleData obstacleData = this.GetObstacleData();

            if (obstacleData.GetSpawnObstacle() != null)
            {
                this.AddComponent(new LogicSpawnerComponent(this, obstacleData.GetSpawnObstacle(), obstacleData.GetSpawnRadius(), obstacleData.GetSpawnIntervalSeconds(),
                                                          obstacleData.GetSpawnCount(), obstacleData.GetMaxSpawned(), obstacleData.GetMaxLifetimeSpawns()));
            }

            if (obstacleData.IsLootCart())
            {
                LogicLootCartComponent logicLootCartComponent = new LogicLootCartComponent(this);
                LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);
                LogicBuilding townHall = this.GetGameObjectManager().GetTownHall();

                LogicArrayList<int> capacityCount = new LogicArrayList<int>();

                for (int i = 0, cap = 0; i < resourceTable.GetItemCount(); i++, cap = 0)
                {
                    LogicResourceData resourceData = (LogicResourceData) resourceTable.GetItemAt(i);

                    if (townHall != null)
                    {
                        if (!resourceData.IsPremiumCurrency() && resourceData.GetWarResourceReferenceData() == null)
                        {
                            cap = LogicDataTables.GetTownHallLevel(townHall.GetUpgradeLevel()).GetCartLootCap(resourceData);
                        }
                    }

                    capacityCount.Add(cap);
                }

                logicLootCartComponent.SetCapacityCount(capacityCount);

                this.AddComponent(logicLootCartComponent);
            }
        }

        public LogicObstacleData GetObstacleData()
        {
            return (LogicObstacleData) this.m_data;
        }

        public LogicLootCartComponent GetLootCartComponent()
        {
            return (LogicLootCartComponent) this.GetComponent(LogicComponentType.LOOT_CART);
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_clearTimer != null)
            {
                this.m_clearTimer.Destruct();
                this.m_clearTimer = null;
            }

            this.m_fadeTime = 0;
        }

        public override bool IsPassable()
        {
            return this.GetObstacleData().IsPassable();
        }

        public override void FastForwardTime(int time)
        {
            base.FastForwardTime(time);

            if (this.m_clearTimer != null)
            {
                int remainingSeconds = this.m_clearTimer.GetRemainingSeconds(this.m_level.GetLogicTime());

                if (remainingSeconds <= time)
                {
                    if (LogicDataTables.GetGlobals().CompleteConstructionOnlyHome())
                    {
                        this.m_clearTimer.StartTimer(0, this.m_level.GetLogicTime(), false, -1);
                    }
                    else
                    {
                        this.ClearingFinished(true);
                    }
                }
                else
                {
                    this.m_clearTimer.StartTimer(remainingSeconds - time, this.m_level.GetLogicTime(), false, -1);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (this.m_clearTimer != null)
            {
                if (this.m_clearTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) > 0 && this.m_level.GetRemainingClockTowerBoostTime() > 0 &&
                    this.GetObstacleData().GetVillageType() == 1)
                {
                    this.m_clearTimer.SetFastForward(this.m_clearTimer.GetFastForward() + 4 * LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 4);
                }
            }

            if (this.m_fadeTime < 1)
            {
                if (this.m_clearTimer != null)
                {
                    if (this.m_clearTimer.GetRemainingSeconds(this.m_level.GetLogicTime()) <= 0)
                    {
                        this.ClearingFinished(false);
                    }
                }
            }
            else
            {
                this.m_fadeTime = LogicMath.Min(this.m_fadeTime + 64, this.GetFadingOutTime());
            }
        }

        public override bool ShouldDestruct()
        {
            return this.m_fadeTime >= this.GetFadingOutTime();
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            base.Save(jsonObject, villageType);

            if (this.m_clearTimer != null)
            {
                jsonObject.Put("clear_t", new LogicJSONNumber(this.m_clearTimer.GetRemainingSeconds(this.m_level.GetLogicTime())));
                jsonObject.Put("clear_ff", new LogicJSONNumber(this.m_clearTimer.GetFastForward()));
            }

            if (this.m_lootMultiplyVersion != 1)
            {
                jsonObject.Put("lmv", new LogicJSONNumber(this.m_lootMultiplyVersion));
            }
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            jsonObject.Put("x", new LogicJSONNumber(this.GetTileX() & 63));
            jsonObject.Put("y", new LogicJSONNumber(this.GetTileY() & 63));
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            base.Load(jsonObject);

            LogicJSONNumber clearTimeObject = jsonObject.GetJSONNumber("clear_t");

            if (clearTimeObject != null)
            {
                if (this.m_clearTimer != null)
                {
                    this.m_clearTimer.Destruct();
                    this.m_clearTimer = null;
                }

                this.m_clearTimer = new LogicTimer();
                this.m_clearTimer.StartTimer(clearTimeObject.GetIntValue(), this.m_level.GetLogicTime(), false, -1);
                this.m_level.GetWorkerManagerAt(this.m_villageType).AllocateWorker(this);
            }

            LogicJSONNumber clearFastForwardObject = jsonObject.GetJSONNumber("clear_ff");

            if (clearFastForwardObject != null)
            {
                if (this.m_clearTimer != null)
                {
                    this.m_clearTimer.SetFastForward(clearFastForwardObject.GetIntValue());
                }
            }

            LogicJSONNumber lootMultiplyVersionObject = jsonObject.GetJSONNumber("loot_multiply_ver");

            if (lootMultiplyVersionObject == null)
            {
                lootMultiplyVersionObject = jsonObject.GetJSONNumber("lmv");

                if (lootMultiplyVersionObject == null)
                {
                    this.m_lootMultiplyVersion = 1;
                    return;
                }
            }

            this.m_lootMultiplyVersion = lootMultiplyVersionObject.GetIntValue();
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            base.LoadFromSnapshot(jsonObject);
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
            return LogicGameObjectType.OBSTACLE;
        }

        public override bool IsUnbuildable()
        {
            return this.GetObstacleData().IsTombstone() || this.GetObstacleData().IsLootCart();
        }

        public override int GetWidthInTiles()
        {
            return this.GetObstacleData().GetWidth();
        }

        public override int GetHeightInTiles()
        {
            return this.GetObstacleData().GetHeight();
        }

        public bool IsTombstone()
        {
            return this.GetObstacleData().IsTombstone();
        }

        public int GetTombGroup()
        {
            return this.GetObstacleData().GetTombGroup();
        }

        public int GetFadeTime()
        {
            return this.m_fadeTime;
        }

        public int GetFadingOutTime()
        {
            LogicObstacleData data = this.GetObstacleData();

            if (!data.IsTallGrass())
            {
                return data.IsLootCart() ? 4000 : 2000;
            }

            return 1000;
        }

        public int GetLootMultiplyVersion()
        {
            return this.m_lootMultiplyVersion;
        }

        public void SetLootMultiplyVersion(int version)
        {
            this.m_lootMultiplyVersion = version;
        }

        public int GetRemainingClearingTime()
        {
            if (this.m_clearTimer != null)
            {
                return this.m_clearTimer.GetRemainingSeconds(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public int GetRemainingClearingTimeMS()
        {
            if (this.m_clearTimer != null)
            {
                return this.m_clearTimer.GetRemainingMS(this.m_level.GetLogicTime());
            }

            return 0;
        }

        public bool IsFadingOut()
        {
            return this.m_fadeTime > 0;
        }

        public bool CanStartClearing()
        {
            return this.m_clearTimer == null && this.m_fadeTime == 0;
        }

        public bool IsClearingOnGoing()
        {
            return this.m_clearTimer != null;
        }

        public void StartClearing()
        {
            if (this.m_clearTimer == null && this.m_fadeTime == 0)
            {
                if (this.GetObstacleData().GetClearTime() != 0)
                {
                    this.m_level.GetWorkerManagerAt(this.m_villageType).AllocateWorker(this);

                    this.m_clearTimer = new LogicTimer();
                    this.m_clearTimer.StartTimer(this.GetObstacleData().GetClearTime(), this.m_level.GetLogicTime(), false, -1);

                    if (this.m_listener != null)
                    {
                        // Listener.
                    }
                }
                else
                {
                    this.ClearingFinished(false);
                }
            }
        }

        public void CancelClearing()
        {
            this.m_level.GetWorkerManagerAt(this.m_data.GetVillageType()).DeallocateWorker(this);

            if (this.m_clearTimer != null)
            {
                this.m_clearTimer.Destruct();
                this.m_clearTimer = null;
            }
        }

        public void ClearingFinished(bool ignoreState)
        {
            int state = this.m_level.GetState();

            if (state == 1 || !LogicDataTables.GetGlobals().CompleteConstructionOnlyHome() && ignoreState)
            {
                if (this.m_level.GetHomeOwnerAvatar().IsClientAvatar())
                {
                    LogicClientAvatar homeOwnerAvatar = (LogicClientAvatar) this.m_level.GetHomeOwnerAvatar();
                    LogicObstacleData obstacleData = this.GetObstacleData();
                    LogicResourceData lootResourceData = obstacleData.GetLootResourceData();

                    int lootCount = obstacleData.GetLootCount();

                    if (obstacleData.IsLootCart())
                    {
                        LogicLootCartComponent lootCartComponent = (LogicLootCartComponent) this.GetComponent(LogicComponentType.LOOT_CART);

                        if (lootCartComponent != null)
                        {
                            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                            bool empty = true;

                            for (int i = 0; i < resourceTable.GetItemCount(); i++)
                            {
                                LogicResourceData resourceData = (LogicResourceData) resourceTable.GetItemAt(i);

                                if (!resourceData.IsPremiumCurrency() && resourceData.GetWarResourceReferenceData() == null)
                                {
                                    int resourceCount = lootCartComponent.GetResourceCount(i);
                                    int rewardCount = LogicMath.Min(homeOwnerAvatar.GetUnusedResourceCap(resourceData), resourceCount);
                                    int remainingCount = resourceCount - rewardCount;

                                    if (rewardCount > 0)
                                    {
                                        homeOwnerAvatar.CommodityCountChangeHelper(0, resourceData, rewardCount);
                                        lootCartComponent.SetResourceCount(i, remainingCount);
                                    }

                                    if (remainingCount > 0)
                                    {
                                        empty = false;
                                    }
                                }
                            }

                            if (!empty)
                            {
                                return;
                            }
                        }
                    }

                    if (!obstacleData.IsTombstone() && !obstacleData.IsLootCart())
                    {
                        this.m_level.GetAchievementManager().ObstacleCleared();
                    }

                    this.m_level.GetWorkerManagerAt(this.m_villageType).DeallocateWorker(this);
                    this.XpGainHelper(LogicGamePlayUtil.TimeToExp(obstacleData.GetClearTime()), homeOwnerAvatar, ignoreState || state == 1);

                    if (lootResourceData != null && lootCount > 0)
                    {
                        if (homeOwnerAvatar != null)
                        {
                            if (lootResourceData.IsPremiumCurrency())
                            {
                                int lootMultipler = 1;

                                if (this.m_lootMultiplyVersion >= 2)
                                {
                                    lootMultipler = obstacleData.GetLootMultiplierVersion2();
                                }

                                int diamondsCount = obstacleData.GetName().Equals("Bonus Gembox")
                                    ? lootCount * lootMultipler
                                    : this.m_level.GetGameObjectManagerAt(this.m_villageType).IncreaseObstacleClearCounter(lootMultipler);

                                if (diamondsCount > 0)
                                {
                                    homeOwnerAvatar.SetDiamonds(homeOwnerAvatar.GetDiamonds() + diamondsCount);
                                    homeOwnerAvatar.SetFreeDiamonds(homeOwnerAvatar.GetFreeDiamonds() + diamondsCount);
                                    homeOwnerAvatar.GetChangeListener().FreeDiamondsAdded(diamondsCount, 6);
                                }
                            }
                            else
                            {
                                int gainCount = LogicMath.Min(homeOwnerAvatar.GetUnusedResourceCap(lootResourceData), lootCount);

                                if (gainCount > 0)
                                {
                                    homeOwnerAvatar.CommodityCountChangeHelper(0, lootResourceData, gainCount);
                                }
                            }
                        }
                        else
                        {
                            Debugger.Error("LogicObstacle::clearingFinished - Home owner avatar is NULL!");
                        }
                    }

                    if (obstacleData.IsEnabledInVillageType(this.m_level.GetVillageType()))
                    {
                        // ?
                    }

                    if (this.m_clearTimer != null)
                    {
                        this.m_clearTimer.Destruct();
                        this.m_clearTimer = null;
                    }

                    this.m_fadeTime = 1;
                }
            }
        }

        public bool SpeedUpClearing()
        {
            if (this.m_clearTimer != null)
            {
                LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                int speedUpCost = LogicGamePlayUtil.GetSpeedUpCost(this.m_clearTimer.GetRemainingSeconds(this.m_level.GetLogicTime()), 0, this.m_villageType);

                if (playerAvatar.HasEnoughDiamonds(speedUpCost, true, this.m_level))
                {
                    playerAvatar.UseDiamonds(speedUpCost);
                    playerAvatar.GetChangeListener().DiamondPurchaseMade(3, this.m_data.GetGlobalID(), 0, speedUpCost, this.m_level.GetVillageType());

                    this.ClearingFinished(false);
                    return true;
                }
            }

            return false;
        }

        public void ReengageLootCart(int secs)
        {
            LogicObstacleData obstacleData = this.GetObstacleData();
            LogicLootCartComponent lootCartComponent = (LogicLootCartComponent) this.GetComponent(LogicComponentType.LOOT_CART);
            LogicBuilding townHall = this.m_level.GetGameObjectManagerAt(0).GetTownHall();

            Debugger.DoAssert(obstacleData.IsLootCart(), string.Empty);
            Debugger.DoAssert(lootCartComponent != null, string.Empty);
            Debugger.DoAssert(townHall != null, string.Empty);

            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < resourceTable.GetItemCount(); i++)
            {
                LogicResourceData resourceData = (LogicResourceData) resourceTable.GetItemAt(i);
                LogicTownhallLevelData townhallLevelData = LogicDataTables.GetTownHallLevel(townHall.GetUpgradeLevel());

                int cap = secs * townhallLevelData.GetCartLootReengagement(resourceData) / 100;

                if (cap > lootCartComponent.GetResourceCount(i))
                {
                    lootCartComponent.SetResourceCount(i, cap);
                }
            }
        }
    }
}