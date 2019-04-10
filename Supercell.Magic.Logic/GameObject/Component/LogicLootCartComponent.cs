namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicLootCartComponent : LogicComponent
    {
        private LogicArrayList<int> m_lootCount;
        private LogicArrayList<int> m_capCount;

        public LogicLootCartComponent(LogicGameObject gameObject) : base(gameObject)
        {
            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            this.m_lootCount = new LogicArrayList<int>(resourceTable.GetItemCount());
            this.m_capCount = new LogicArrayList<int>(resourceTable.GetItemCount());

            for (int i = 0; i < resourceTable.GetItemCount(); i++)
            {
                this.m_lootCount.Add(0);
                this.m_capCount.Add(0);
            }
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_lootCount = null;
            this.m_capCount = null;
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.LOOT_CART;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < resourceTable.GetItemCount(); i++)
            {
                LogicResourceData resourceData = (LogicResourceData) resourceTable.GetItemAt(i);

                if (!resourceData.IsPremiumCurrency() && resourceData.GetWarResourceReferenceData() == null)
                {
                    if (LogicDataTables.GetGoldData() == resourceData)
                    {
                        LogicJSONNumber count = jsonObject.GetJSONNumber("defg");

                        if (count != null)
                        {
                            this.SetResourceCount(i, count.GetIntValue());
                        }
                    }
                    else if (LogicDataTables.GetElixirData() == resourceData)
                    {
                        LogicJSONNumber count = jsonObject.GetJSONNumber("defe");

                        if (count != null)
                        {
                            this.SetResourceCount(i, count.GetIntValue());
                        }
                    }
                    else if (LogicDataTables.GetDarkElixirData() == resourceData)
                    {
                        LogicJSONNumber count = jsonObject.GetJSONNumber("defde");

                        if (count != null)
                        {
                            this.SetResourceCount(i, count.GetIntValue());
                        }
                    }
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < resourceTable.GetItemCount(); i++)
            {
                LogicResourceData resourceData = (LogicResourceData) resourceTable.GetItemAt(i);

                if (!resourceData.IsPremiumCurrency() && resourceData.GetWarResourceReferenceData() == null)
                {
                    if (LogicDataTables.GetGoldData() == resourceData)
                    {
                        int count = this.GetResourceCount(i);

                        if (count > 0)
                        {
                            jsonObject.Put("defg", new LogicJSONNumber(count));
                        }
                    }
                    else if (LogicDataTables.GetElixirData() == resourceData)
                    {
                        int count = this.GetResourceCount(i);

                        if (count > 0)
                        {
                            jsonObject.Put("defe", new LogicJSONNumber(count));
                        }
                    }
                    else if (LogicDataTables.GetDarkElixirData() == resourceData)
                    {
                        int count = this.GetResourceCount(i);

                        if (count > 0)
                        {
                            jsonObject.Put("defde", new LogicJSONNumber(count));
                        }
                    }
                }
            }
        }

        public int GetResourceCount(int idx)
        {
            return this.m_lootCount[idx];
        }

        public void SetResourceCount(int idx, int count)
        {
            this.m_lootCount[idx] = LogicMath.Clamp(count, 0, this.m_capCount[idx]);
        }

        public int GetCapacityCount(int idx)
        {
            return this.m_capCount[idx];
        }

        public void SetCapacityCount(LogicArrayList<int> count)
        {
            for (int i = 0; i < count.Size(); i++)
            {
                this.m_capCount[i] = count[i];
            }

            this.m_parent.GetLevel().RefreshResourceCaps();
        }
    }
}