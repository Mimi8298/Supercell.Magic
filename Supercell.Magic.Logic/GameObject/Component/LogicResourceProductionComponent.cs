namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicResourceProductionComponent : LogicComponent
    {
        private readonly LogicResourceData m_resourceData;
        private readonly LogicTimer m_resourceTimer;

        private int m_availableLoot;
        private int m_maxResources;
        private int m_productionPer100Hour;

        public LogicResourceProductionComponent(LogicGameObject gameObject, LogicResourceData data) : base(gameObject)
        {
            this.m_resourceTimer = new LogicTimer();
            this.m_resourceData = data;
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.RESOURCE_PRODUCTION;
        }

        public LogicResourceData GetResourceData()
        {
            return this.m_resourceData;
        }

        public void RestartTimer()
        {
            int totalTime = 0;

            if (this.m_productionPer100Hour >= 1)
            {
                totalTime = (int) (360000L * this.m_maxResources / this.m_productionPer100Hour);
            }

            this.m_resourceTimer.StartTimer(totalTime, this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public void SetProduction(int productionPer100Hour, int maxResources)
        {
            this.m_productionPer100Hour = productionPer100Hour;
            this.m_maxResources = maxResources;

            this.RestartTimer();
        }

        public int GetStealableResourceCount()
        {
            return LogicMath.Min(this.GetResourceCount(), this.m_availableLoot);
        }

        public int GetResourceCount()
        {
            if (this.m_productionPer100Hour > 0)
            {
                int totalTime = (int) (360000L * this.m_maxResources / this.m_productionPer100Hour);

                if (totalTime > 0)
                {
                    int remainingTime = this.m_resourceTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());

                    if (remainingTime != 0)
                    {
                        return (int) ((long) this.m_productionPer100Hour * (totalTime - remainingTime) / 360000L);
                    }

                    return this.m_maxResources;
                }
            }

            return 0;
        }

        public void DecreaseResources(int count)
        {
            int resourceCount = this.GetResourceCount();
            int removeCount = LogicMath.Min(count, resourceCount);

            if (this.m_productionPer100Hour != 0)
            {
                int totalTime = (int) (360000L * this.m_maxResources / this.m_productionPer100Hour);
                int skipTime = (int) (360000L * (resourceCount - removeCount) / this.m_productionPer100Hour);

                this.m_resourceTimer.StartTimer(totalTime - skipTime, this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }
        }

        public int CollectResources(bool updateListener)
        {
            if (this.m_parent.GetLevel().GetHomeOwnerAvatar() != null)
            {
                int resourceCount = this.GetResourceCount();

                if (this.m_parent.GetLevel().GetHomeOwnerAvatar().IsNpcAvatar())
                {
                    Debugger.Error("LogicResourceProductionComponent::collectResources() called for Npc avatar");
                }
                else
                {
                    LogicClientAvatar clientAvatar = (LogicClientAvatar) this.m_parent.GetLevel().GetHomeOwnerAvatar();

                    if (resourceCount != 0)
                    {
                        if (this.m_resourceData.IsPremiumCurrency())
                        {
                            this.DecreaseResources(resourceCount);

                            clientAvatar.SetDiamonds(clientAvatar.GetDiamonds() + resourceCount);
                            clientAvatar.SetFreeDiamonds(clientAvatar.GetFreeDiamonds() + resourceCount);
                            clientAvatar.GetChangeListener().FreeDiamondsAdded(resourceCount, 10);
                        }
                        else
                        {
                            int unusedResourceCap = clientAvatar.GetUnusedResourceCap(this.m_resourceData);

                            if (unusedResourceCap != 0)
                            {
                                if (resourceCount > unusedResourceCap)
                                {
                                    resourceCount = unusedResourceCap;
                                }

                                this.DecreaseResources(resourceCount);

                                clientAvatar.CommodityCountChangeHelper(0, this.m_resourceData, resourceCount);
                            }
                            else
                            {
                                resourceCount = 0;
                            }
                        }

                        return resourceCount;
                    }
                }
            }

            return 0;
        }

        public override void FastForwardTime(int time)
        {
            int remainingSeconds = this.m_resourceTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime());
            int boostTime = this.m_parent.GetRemainingBoostTime();

            if (boostTime > 0 && LogicDataTables.GetGlobals().GetResourceProductionBoostMultiplier() > 1 && !this.m_parent.IsBoostPaused())
            {
                time += (LogicDataTables.GetGlobals().GetResourceProductionBoostMultiplier() - 1) * LogicMath.Min(time, boostTime);
            }

            int clockBoostTime = this.m_parent.GetLevel().GetUpdatedClockTowerBoostTime();

            if (clockBoostTime > 0 && !this.m_parent.GetLevel().IsClockTowerBoostPaused())
            {
                if (this.m_parent.GetData().GetDataType() == LogicDataType.BUILDING && this.m_parent.GetData().GetVillageType() == 1)
                {
                    time += (LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 1) * LogicMath.Min(time, clockBoostTime);
                }
            }

            this.m_resourceTimer.StartTimer(remainingSeconds <= time ? 0 : remainingSeconds - time, this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public override void GetChecksum(ChecksumHelper checksum)
        {
            checksum.StartObject("LogicResourceProductionComponent");
            checksum.WriteValue("resourceTimer", this.m_resourceTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()));
            checksum.WriteValue("m_availableLoot", this.m_availableLoot);
            checksum.WriteValue("m_maxResources", this.m_maxResources);
            checksum.WriteValue("m_productionPer100Hour", this.m_productionPer100Hour);
            checksum.EndObject();
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONNumber resourceTimeObject = jsonObject.GetJSONNumber("res_time");
            int time = this.m_productionPer100Hour > 0 ? (int) (360000L * this.m_maxResources / this.m_productionPer100Hour) : 0;

            if (resourceTimeObject != null)
            {
                time = LogicMath.Min(resourceTimeObject.GetIntValue(), time);
            }

            this.m_resourceTimer.StartTimer(time, this.m_parent.GetLevel().GetLogicTime(), false, -1);
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            jsonObject.Put("res_time", new LogicJSONNumber(this.m_resourceTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime())));
        }

        public void RecalculateAvailableLoot()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();

            if (!homeOwnerAvatar.IsNpcAvatar())
            {
                int matchType = this.m_parent.GetLevel().GetMatchType();

                if (matchType >= 10 || matchType != 3 && matchType != 5)
                {
                    int resourceProductionLootPercentage = LogicDataTables.GetGlobals().GetResourceProductionLootPercentage(this.m_resourceData);

                    if (homeOwnerAvatar.IsClientAvatar())
                    {
                        LogicAvatar visitorAvatar = this.m_parent.GetLevel().GetVisitorAvatar();

                        if (visitorAvatar != null && visitorAvatar.IsClientAvatar())
                        {
                            resourceProductionLootPercentage = resourceProductionLootPercentage *
                                                               LogicDataTables.GetGlobals().GetLootMultiplierByTownHallDiff(visitorAvatar.GetTownHallLevel(),
                                                                                                                            homeOwnerAvatar.GetTownHallLevel()) / 100;
                        }
                    }

                    if (resourceProductionLootPercentage > 100)
                    {
                        resourceProductionLootPercentage = 100;
                    }

                    this.m_availableLoot = (int) ((long) this.GetResourceCount() * resourceProductionLootPercentage / 100L);
                }
                else
                {
                    this.m_availableLoot = 0;
                }
            }
            else
            {
                this.m_availableLoot = 0;
            }
        }

        public void ResourcesStolen(int damage, int hp)
        {
            if (damage > 0 && hp > 0)
            {
                int stealableResource = damage * this.GetStealableResourceCount() / hp;

                if (stealableResource > 0)
                {
                    this.m_parent.GetLevel().GetBattleLog().IncreaseStolenResourceCount(this.m_resourceData, stealableResource);
                    this.DecreaseResources(stealableResource);
                    this.m_parent.GetLevel().GetVisitorAvatar().CommodityCountChangeHelper(0, this.m_resourceData, stealableResource);
                    this.m_availableLoot -= stealableResource;
                }
            }
        }

        public override void Tick()
        {
            if (this.m_parent.GetRemainingBoostTime() > 0 && !this.m_parent.IsBoostPaused())
            {
                this.m_resourceTimer.FastForwardSubticks(4 * LogicDataTables.GetGlobals().GetResourceProductionBoostMultiplier() - 4);
            }

            if (this.m_parent.GetLevel().GetRemainingClockTowerBoostTime() > 0)
            {
                if (this.m_parent.GetData().GetDataType() == LogicDataType.BUILDING && this.m_parent.GetData().GetVillageType() == 1)
                {
                    this.m_resourceTimer.FastForwardSubticks(4 * LogicDataTables.GetGlobals().GetClockTowerBoostMultiplier() - 4);
                }
            }
        }
    }
}