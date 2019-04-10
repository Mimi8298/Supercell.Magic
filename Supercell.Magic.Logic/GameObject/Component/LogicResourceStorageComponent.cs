namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicResourceStorageComponent : LogicComponent
    {
        protected LogicArrayList<int> m_resourceCount;
        protected LogicArrayList<int> m_maxResourceCount;
        protected LogicArrayList<int> m_maxPercentageResourceCount;
        protected LogicArrayList<int> m_stealableResourceCount;

        public LogicResourceStorageComponent(LogicGameObject gameObject) : base(gameObject)
        {
            int resourceCount = LogicDataTables.GetTable(LogicDataType.RESOURCE).GetItemCount();

            this.m_maxResourceCount = new LogicArrayList<int>(resourceCount);
            this.m_stealableResourceCount = new LogicArrayList<int>(resourceCount);
            this.m_resourceCount = new LogicArrayList<int>(resourceCount);
            this.m_maxPercentageResourceCount = new LogicArrayList<int>(resourceCount);

            for (int i = 0; i < resourceCount; i++)
            {
                this.m_resourceCount.Add(0);
                this.m_maxResourceCount.Add(0);
                this.m_stealableResourceCount.Add(0);
                this.m_maxPercentageResourceCount.Add(100);
            }
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_maxResourceCount = null;
            this.m_stealableResourceCount = null;
            this.m_resourceCount = null;
            this.m_maxPercentageResourceCount = null;
        }

        public int GetStealableResourceCount(int idx)
        {
            return LogicMath.Min(this.m_resourceCount[idx], this.m_stealableResourceCount[idx]);
        }

        public virtual void ResourcesStolen(int damage, int hp)
        {
            if (damage > 0 && hp > 0)
            {
                LogicDataTable table = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                for (int i = 0; i < this.m_stealableResourceCount.Size(); i++)
                {
                    LogicResourceData data = (LogicResourceData) table.GetItemAt(i);

                    int stealableResource = this.GetStealableResourceCount(i);

                    if (damage < hp)
                    {
                        stealableResource = damage * stealableResource / hp;
                    }

                    if (stealableResource > 0)
                    {
                        this.m_parent.GetLevel().GetBattleLog().IncreaseStolenResourceCount(data, stealableResource);
                        this.m_resourceCount[i] -= stealableResource;

                        LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
                        LogicAvatar visitorAvatar = this.m_parent.GetLevel().GetVisitorAvatar();

                        homeOwnerAvatar.CommodityCountChangeHelper(0, data, -stealableResource);
                        visitorAvatar.CommodityCountChangeHelper(0, data, stealableResource);

                        if (homeOwnerAvatar.IsNpcAvatar())
                        {
                            LogicNpcData npcData = ((LogicNpcAvatar) homeOwnerAvatar).GetNpcData();

                            if (data == LogicDataTables.GetGoldData())
                            {
                                visitorAvatar.CommodityCountChangeHelper(1, npcData, stealableResource);
                            }
                            else if (data == LogicDataTables.GetElixirData())
                            {
                                visitorAvatar.CommodityCountChangeHelper(2, npcData, stealableResource);
                            }
                        }

                        this.m_stealableResourceCount[i] = LogicMath.Max(this.m_stealableResourceCount[i] - stealableResource, 0);
                    }
                }
            }
        }

        public void SetCount(int idx, int count)
        {
            this.m_resourceCount[idx] = LogicMath.Clamp(count, 0, this.GetMax(idx));

            if (this.m_parent.GetListener() != null)
                this.m_parent.GetListener().RefreshResourceCount();
        }

        public virtual int GetMax(int idx)
        {
            if (this.m_parent.GetData() != LogicDataTables.GetTownHallData() || !this.m_parent.GetLevel().IsNpcVillage())
                return this.m_maxResourceCount[idx];
            return 10000000;
        }

        public int GetCount(int idx)
        {
            return this.m_resourceCount[idx];
        }

        public int GetRecommendedMax(int idx)
        {
            int max = this.GetMax(idx);

            LogicHitpointComponent hitpointComponent = this.m_parent.GetHitpointComponent();

            if (hitpointComponent != null)
            {
                int hp = hitpointComponent.GetHitpoints();
                int maxHp = hitpointComponent.GetMaxHitpoints();

                if (hp <= 10000)
                {
                    if (hp <= 1000)
                        return max * hp / maxHp;
                    return 100 * (max * (hp / 100) / maxHp);
                }
                return 1000 * (max * (hp / 1000) / maxHp);
            }

            return max;
        }

        public int GetRecommendedMax(int idx, int count)
        {
            int max = this.GetRecommendedMax(idx);

            if (this.m_maxPercentageResourceCount[idx] != 0)
                return LogicMath.Min(this.m_maxPercentageResourceCount[idx] * count / 100, max);
            return max;
        }

        public void SetMaxArray(LogicArrayList<int> max)
        {
            for (int i = 0; i < max.Size(); i++)
            {
                this.m_maxResourceCount[i] = max[i];
            }

            this.m_parent.GetLevel().RefreshResourceCaps();
        }

        public void SetMaxPercentageArray(LogicArrayList<int> max)
        {
            for (int i = 0; i < max.Size(); i++)
            {
                this.m_maxPercentageResourceCount[i] = max[i];
            }
        }

        public virtual void RecalculateAvailableLoot()
        {
            int matchType = this.m_parent.GetLevel().GetMatchType();

            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
            LogicAvatar visitorAvatar = this.m_parent.GetLevel().GetVisitorAvatar();
            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < this.m_resourceCount.Size(); i++)
            {
                LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);
                int resourceCount = this.m_resourceCount[i];

                if (!homeOwnerAvatar.IsNpcAvatar())
                {
                    if (matchType == 5 && this.m_parent.GetLevel().IsArrangedWar())
                    {
                        if (resourceCount >= 0)
                        {
                            resourceCount = 0;
                        }
                    }
                    else if (LogicDataTables.GetGlobals().UseTownHallLootPenaltyInWar() || matchType != 5)
                    {
                        if (matchType != 8 && matchType != 9)
                        {
                            int multiplier = 100;
                            int calculateAvailableLootCount = 0;

                            if (homeOwnerAvatar != null && homeOwnerAvatar.IsClientAvatar() &&
                                visitorAvatar != null && visitorAvatar.IsClientAvatar())
                            {
                                multiplier = LogicDataTables.GetGlobals().GetLootMultiplierByTownHallDiff(visitorAvatar.GetTownHallLevel(), homeOwnerAvatar.GetTownHallLevel());
                            }

                            if (this.m_parent.GetData() == LogicDataTables.GetTownHallData() && LogicDataTables.GetGlobals().GetTownHallLootPercentage() != -1)
                            {
                                calculateAvailableLootCount = resourceCount * (multiplier * LogicDataTables.GetGlobals().GetTownHallLootPercentage() / 100) / 100;
                            }
                            else if (!data.IsPremiumCurrency())
                            {
                                int townHallLevel = homeOwnerAvatar.GetTownHallLevel();
                                int lootableResourceCount = 0;

                                if (matchType != 3)
                                {
                                    if (matchType == 5)
                                    {
                                        lootableResourceCount = resourceCount;
                                    }
                                    else if (matchType != 7)
                                    {
                                        lootableResourceCount = (int) ((long) resourceCount * LogicDataTables.GetTownHallLevel(townHallLevel).GetStorageLootPercentage(data) / 100);
                                    }
                                }

                                int storageLootCap = LogicDataTables.GetTownHallLevel(townHallLevel).GetStorageLootCap(data);
                                int maxResourceCount = LogicMath.Min(homeOwnerAvatar.GetResourceCount(data), homeOwnerAvatar.GetResourceCap(data));

                                if (maxResourceCount > storageLootCap && maxResourceCount > 0)
                                {
                                    int clampedValue;

                                    if (storageLootCap < 1000000)
                                    {
                                        if (storageLootCap < 100000)
                                        {
                                            if (storageLootCap < 10000)
                                            {
                                                if (storageLootCap < 1000)
                                                {
                                                    clampedValue = (resourceCount * storageLootCap + (maxResourceCount >> 1)) / maxResourceCount;
                                                }
                                                else
                                                {
                                                    if (!LogicDataTables.GetGlobals().UseMoreAccurateLootCap())
                                                    {
                                                        clampedValue = 100 * ((resourceCount * (storageLootCap / 100) + (maxResourceCount >> 1)) / maxResourceCount);
                                                    }
                                                    else
                                                    {
                                                        if (resourceCount / 100 > maxResourceCount / storageLootCap)
                                                        {
                                                            clampedValue = 100 * ((resourceCount * (storageLootCap / 100) + (maxResourceCount >> 1)) / maxResourceCount);
                                                        }
                                                        else
                                                        {
                                                            clampedValue = (resourceCount * storageLootCap + (maxResourceCount >> 1)) / maxResourceCount;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!LogicDataTables.GetGlobals().UseMoreAccurateLootCap())
                                                {
                                                    clampedValue = 1000 * ((resourceCount * (storageLootCap / 1000) + (maxResourceCount >> 1)) / maxResourceCount);
                                                }
                                                else
                                                {
                                                    if (resourceCount / 1000 > maxResourceCount / storageLootCap)
                                                    {
                                                        clampedValue = 1000 * ((resourceCount * (storageLootCap / 1000) + (maxResourceCount >> 1)) / maxResourceCount);
                                                    }
                                                    else
                                                    {
                                                        if (resourceCount / 100 > maxResourceCount / storageLootCap)
                                                        {
                                                            clampedValue = 100 * ((resourceCount * (storageLootCap / 100) + (maxResourceCount >> 1)) / maxResourceCount);
                                                        }
                                                        else
                                                        {
                                                            clampedValue = (resourceCount * storageLootCap + (maxResourceCount >> 1)) / maxResourceCount;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!LogicDataTables.GetGlobals().UseMoreAccurateLootCap())
                                            {
                                                clampedValue = 10000 * ((resourceCount * (storageLootCap / 10000) + (maxResourceCount >> 1)) / maxResourceCount);
                                            }
                                            else
                                            {
                                                if (resourceCount / 10000 > maxResourceCount / storageLootCap)
                                                {
                                                    clampedValue = 10000 * ((resourceCount * (storageLootCap / 10000) + (maxResourceCount >> 1)) / maxResourceCount);
                                                }
                                                else
                                                {
                                                    if (resourceCount / 1000 > maxResourceCount / storageLootCap)
                                                    {
                                                        clampedValue = 1000 * ((resourceCount * (storageLootCap / 1000) + (maxResourceCount >> 1)) / maxResourceCount);
                                                    }
                                                    else
                                                    {
                                                        if (resourceCount / 100 > maxResourceCount / storageLootCap)
                                                        {
                                                            clampedValue = 100 * ((resourceCount * (storageLootCap / 100) + (maxResourceCount >> 1)) / maxResourceCount);
                                                        }
                                                        else
                                                        {
                                                            clampedValue = (resourceCount * storageLootCap + (maxResourceCount >> 1)) / maxResourceCount;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        clampedValue = 40000 * ((resourceCount * (storageLootCap / 40000) + (maxResourceCount >> 1)) / maxResourceCount);
                                    }

                                    if (lootableResourceCount > clampedValue)
                                    {
                                        lootableResourceCount = clampedValue;
                                    }
                                }

                                calculateAvailableLootCount = multiplier * lootableResourceCount / 100;
                            }

                            if (calculateAvailableLootCount <= resourceCount)
                            {
                                resourceCount = calculateAvailableLootCount;
                            }
                        }
                    }
                }

                this.m_stealableResourceCount[i] = resourceCount;
            }
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.RESOURCE_STORAGE;
        }
    }
}