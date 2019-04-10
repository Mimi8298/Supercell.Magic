namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Titan.Math;

    public sealed class LogicWarResourceStorageComponent : LogicResourceStorageComponent
    {
        public LogicWarResourceStorageComponent(LogicGameObject gameObject) : base(gameObject)
        {
            // LogicWarResourceStorageComponent.
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.WAR_RESOURCE_STORAGE;
        }

        public bool IsNotEmpty()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicResourceData data = (LogicResourceData) table.GetItemAt(i);

                if (data.GetWarResourceReferenceData() != null)
                {
                    if (homeOwnerAvatar.GetResourceCount(data) > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override int GetMax(int idx)
        {
            int multiplierPercent = 100;

            if (this.m_parent.GetLevel().GetHomeOwnerAvatar() != null)
            {
                if (this.m_parent.GetLevel().GetHomeOwnerAvatar().IsClientAvatar())
                {
                    int allianceExpLevel = ((LogicClientAvatar) this.m_parent.GetLevel().GetHomeOwnerAvatar()).GetAllianceLevel();

                    if (allianceExpLevel > 0)
                    {
                        multiplierPercent = LogicDataTables.GetAllianceLevel(allianceExpLevel).GetWarLootCapacityPercent();
                    }
                }
            }

            return multiplierPercent * this.m_maxResourceCount[idx] / 100;
        }

        public override void RecalculateAvailableLoot()
        {
            LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < this.m_resourceCount.Size(); i++)
            {
                LogicResourceData data = (LogicResourceData) resourceTable.GetItemAt(i);

                if (this.m_parent.GetData() == LogicDataTables.GetAllianceCastleData())
                {
                    LogicResourceData refData = data.GetWarResourceReferenceData();

                    int resourceCount = this.m_resourceCount[i];

                    if (refData != null)
                    {
                        int warLootPercentage = LogicDataTables.GetGlobals().GetWarLootPercentage();
                        int lootableResourceCount = 0;

                        if ((this.m_parent.GetLevel().GetMatchType() | 4) != 7 && !this.m_parent.GetLevel().IsArrangedWar())
                        {
                            lootableResourceCount = (int) ((long) resourceCount * warLootPercentage / 100);
                        }

                        int storageLootCap = LogicDataTables.GetTownHallLevel(homeOwnerAvatar.GetTownHallLevel()).GetStorageLootCap(data);
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
                                        clampedValue = storageLootCap < 1000
                                            ? (resourceCount * storageLootCap + (maxResourceCount >> 1)) / maxResourceCount
                                            : 10 * ((resourceCount * (storageLootCap / 10) + (maxResourceCount >> 1)) / maxResourceCount);
                                    }
                                    else
                                    {
                                        clampedValue = 100 * ((resourceCount * (storageLootCap / 100) + (maxResourceCount >> 1)) / maxResourceCount);
                                    }
                                }
                                else
                                {
                                    clampedValue = 1000 * ((resourceCount * (storageLootCap / 1000) + (maxResourceCount >> 1)) / maxResourceCount);
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

                        if (lootableResourceCount > resourceCount)
                        {
                            lootableResourceCount = resourceCount;
                        }

                        this.m_stealableResourceCount[i] = lootableResourceCount;
                    }
                }
            }
        }

        public override void ResourcesStolen(int damage, int hp)
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

                    if (stealableResource > 0 && data.GetWarResourceReferenceData() != null)
                    {
                        this.m_parent.GetLevel().GetBattleLog().IncreaseStolenResourceCount(data.GetWarResourceReferenceData(), stealableResource);
                        this.m_resourceCount[i] -= stealableResource;

                        LogicAvatar homeOwnerAvatar = this.m_parent.GetLevel().GetHomeOwnerAvatar();
                        LogicAvatar visitorAvatar = this.m_parent.GetLevel().GetVisitorAvatar();

                        homeOwnerAvatar.CommodityCountChangeHelper(0, data, -stealableResource);
                        visitorAvatar.CommodityCountChangeHelper(0, data.GetWarResourceReferenceData(), stealableResource);

                        this.m_stealableResourceCount[i] = LogicMath.Max(this.m_stealableResourceCount[i] - stealableResource, 0);
                    }
                }
            }
        }

        public int CollectResources()
        {
            int collected = -1;

            if (this.m_parent.GetLevel().GetHomeOwnerAvatar().IsClientAvatar())
            {
                collected = 0;

                LogicClientAvatar playerAvatar = (LogicClientAvatar) this.m_parent.GetLevel().GetHomeOwnerAvatar();
                LogicDataTable table = LogicDataTables.GetTable(LogicDataType.RESOURCE);

                for (int i = 0; i < table.GetItemCount(); i++)
                {
                    LogicResourceData data = (LogicResourceData) table.GetItemAt(i);

                    if (data.GetWarResourceReferenceData() != null)
                    {
                        int count = playerAvatar.GetResourceCount(data);

                        if (count > 0)
                        {
                            int unusedResourceCap = playerAvatar.GetUnusedResourceCap(data.GetWarResourceReferenceData());

                            if (unusedResourceCap != 0)
                            {
                                if (count > unusedResourceCap)
                                {
                                    count = unusedResourceCap;
                                }

                                if (data.GetName().Equals("WarGold"))
                                {
                                    this.m_parent.GetLevel().GetAchievementManager().IncreaseWarGoldResourceLoot(count);
                                }

                                collected = count;

                                playerAvatar.CommodityCountChangeHelper(0, data.GetWarResourceReferenceData(), count);
                                playerAvatar.CommodityCountChangeHelper(0, data, -count);
                            }
                        }
                    }
                }
            }

            return collected;
        }
    }
}