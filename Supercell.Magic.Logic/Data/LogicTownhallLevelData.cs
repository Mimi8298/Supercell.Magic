namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicTownhallLevelData : LogicData
    {
        private LogicArrayList<int> m_buildingCaps;
        private LogicArrayList<int> m_buildingGearupCaps;
        private LogicArrayList<int> m_trapCaps;
        private LogicArrayList<int> m_treasuryCaps;

        private int m_attackCost;
        private int m_attackCostVillage2;
        private int m_maxHousingSpace;
        private int m_friendlyCost;
        private int m_resourceStorageLootPercentage;
        private int m_darkElixirStorageLootPercentage;
        private int m_resourceStorageLootCap;
        private int m_darkElixirStorageLootCap;
        private int m_warPrizeResourceCap;
        private int m_warPrizeDarkElixirCap;
        private int m_warPrizeAllianceExpCap;
        private int m_cartLootCapResource;
        private int m_cartLootReengagementResource;
        private int m_cartLootCapDarkElixir;
        private int m_cartLootReengagementDarkElixir;
        private int m_strengthMaxTroopTypes;
        private int m_strengthMaxSpellTypes;
        private int m_packElixir;
        private int m_packGold;
        private int m_packDarkElixir;
        private int m_packGold2;
        private int m_packElixir2;
        private int m_duelPrizeResourceCap;
        private int m_changeTroopCost;

        public LogicTownhallLevelData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            this.m_maxHousingSpace = -1;
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_buildingCaps = new LogicArrayList<int>();
            this.m_buildingGearupCaps = new LogicArrayList<int>();
            this.m_trapCaps = new LogicArrayList<int>();
            this.m_treasuryCaps = new LogicArrayList<int>();

            LogicTownhallLevelData previousItem = null;

            if (this.GetInstanceID() > 0)
            {
                previousItem = (LogicTownhallLevelData) this.m_table.GetItemAt(this.GetInstanceID() - 1);
            }

            LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);

            for (int i = 0; i < buildingTable.GetItemCount(); i++)
            {
                LogicData item = buildingTable.GetItemAt(i);

                int cap = this.GetIntegerValue(item.GetName(), 0);
                int gearup = this.GetIntegerValue(item.GetName() + "_gearup", 0);

                if (previousItem != null)
                {
                    if (cap == 0)
                    {
                        cap = previousItem.m_buildingCaps[i];
                    }

                    if (gearup == 0)
                    {
                        gearup = previousItem.m_buildingGearupCaps[i];
                    }
                }

                this.m_buildingCaps.Add(cap);
                this.m_buildingGearupCaps.Add(gearup);
            }

            LogicDataTable trapTable = LogicDataTables.GetTable(LogicDataType.TRAP);

            for (int i = 0; i < trapTable.GetItemCount(); i++)
            {
                int cap = this.GetIntegerValue(trapTable.GetItemAt(i).GetName(), 0);

                if (previousItem != null)
                {
                    if (cap == 0)
                    {
                        cap = previousItem.m_trapCaps[i];
                    }
                }

                this.m_trapCaps.Add(cap);
            }

            LogicDataTable resourceTable = LogicDataTables.GetTable(LogicDataType.RESOURCE);

            for (int i = 0; i < resourceTable.GetItemCount(); i++)
            {
                this.m_treasuryCaps.Add(this.GetIntegerValue("Treasury" + resourceTable.GetItemAt(i).GetName(), 0));
            }

            this.m_attackCost = this.GetIntegerValue("AttackCost", 0);
            this.m_attackCostVillage2 = this.GetIntegerValue("AttackCostVillage2", 0);
            this.m_resourceStorageLootPercentage = this.GetIntegerValue("ResourceStorageLootPercentage", 0);
            this.m_darkElixirStorageLootPercentage = this.GetIntegerValue("DarkElixirStorageLootPercentage", 0);
            this.m_resourceStorageLootCap = this.GetIntegerValue("ResourceStorageLootCap", 0);
            this.m_darkElixirStorageLootCap = this.GetIntegerValue("DarkElixirStorageLootCap", 0);
            this.m_warPrizeResourceCap = this.GetIntegerValue("WarPrizeResourceCap", 0);
            this.m_warPrizeDarkElixirCap = this.GetIntegerValue("WarPrizeDarkElixirCap", 0);
            this.m_warPrizeAllianceExpCap = this.GetIntegerValue("WarPrizeAllianceExpCap", 0);
            this.m_cartLootCapResource = this.GetIntegerValue("CartLootCapResource", 0);
            this.m_cartLootReengagementResource = this.GetIntegerValue("CartLootReengagementResource", 0);
            this.m_cartLootCapDarkElixir = this.GetIntegerValue("CartLootCapDarkElixir", 0);
            this.m_cartLootReengagementDarkElixir = this.GetIntegerValue("CartLootReengagementDarkElixir", 0);
            this.m_strengthMaxTroopTypes = this.GetIntegerValue("StrengthMaxTroopTypes", 0);
            this.m_strengthMaxSpellTypes = this.GetIntegerValue("StrengthMaxSpellTypes", 0);
            this.m_friendlyCost = this.GetIntegerValue("FriendlyCost", 0);
            this.m_packElixir = this.GetIntegerValue("PackElixir", 0);
            this.m_packGold = this.GetIntegerValue("PackGold", 0);
            this.m_packDarkElixir = this.GetIntegerValue("PackDarkElixir", 0);
            this.m_packGold2 = this.GetIntegerValue("PackGold2", 0);
            this.m_packElixir2 = this.GetIntegerValue("PackElixir2", 0);
            this.m_duelPrizeResourceCap = this.GetIntegerValue("DuelPrizeResourceCap", 0);
            this.m_changeTroopCost = this.GetIntegerValue("ChangeTroopCost", 0);

            if ((uint) this.m_darkElixirStorageLootPercentage > 100 || (uint) this.m_darkElixirStorageLootPercentage > 100)
            {
                Debugger.Error("townhall_levels.csv: Invalid loot percentage!");
            }
        }

        public int GetStorageLootPercentage(LogicResourceData data)
        {
            if (LogicDataTables.GetDarkElixirData() == data)
            {
                return this.m_darkElixirStorageLootPercentage;
            }

            return this.m_resourceStorageLootPercentage;
        }

        public int GetAttackCost()
        {
            return this.m_attackCost;
        }

        public int GetAttackCostVillage2()
        {
            return this.m_attackCostVillage2;
        }

        public int GetFriendlyCost()
        {
            return this.m_friendlyCost;
        }

        public int GetStorageLootCap(LogicResourceData data)
        {
            if (data != null && !data.IsPremiumCurrency())
            {
                if (LogicDataTables.GetDarkElixirData() == data)
                {
                    return this.m_darkElixirStorageLootCap;
                }

                return this.m_resourceStorageLootCap;
            }

            return 0;
        }

        public int GetCartLootCap(LogicResourceData data)
        {
            if (data != null && !data.IsPremiumCurrency())
            {
                if (LogicDataTables.GetDarkElixirData() == data)
                {
                    return this.m_cartLootCapDarkElixir;
                }

                return this.m_cartLootCapResource;
            }

            return 0;
        }

        public int GetCartLootReengagement(LogicResourceData data)
        {
            if (data != null && !data.IsPremiumCurrency() && data.GetWarResourceReferenceData() == null && data.GetVillageType() != 1)
            {
                if (LogicDataTables.GetDarkElixirData() == data)
                {
                    return this.m_cartLootReengagementDarkElixir;
                }

                return this.m_cartLootReengagementResource;
            }

            return 0;
        }

        public int GetMaxHousingSpace()
        {
            if (this.m_maxHousingSpace == -1)
            {
                this.CalculateHousingSpaceCap();
            }

            return this.m_maxHousingSpace;
        }

        public void CalculateHousingSpaceCap()
        {
            this.m_maxHousingSpace = 0;

            if (this.GetInstanceID() > 0)
            {
                this.m_table.GetItemAt(this.GetInstanceID() - 1); // Thx supercell for the crappy code.
            }

            LogicDataTable buildingTable = LogicDataTables.GetTable(LogicDataType.BUILDING);
            int dataTableCount = this.m_table.GetItemCount();

            if (dataTableCount > 0)
            {
                int unitHousingCostMultiplierForTotal = LogicDataTables.GetGlobals().GetUnitHousingCostMultiplierForTotal();
                int spellHousingCostMultiplierForTotal = LogicDataTables.GetGlobals().GetSpellHousingCostMultiplierForTotal();
                int heroHousingCostMultiplierForTotal = LogicDataTables.GetGlobals().GetHeroHousingCostMultiplierForTotal();
                int allianceUnitHousingCostMultiplierForTotal = LogicDataTables.GetGlobals().GetAllianceUnitHousingCostMultiplierForTotal();

                int idx = 0;

                do
                {
                    LogicBuildingData buildingData = (LogicBuildingData) buildingTable.GetItemAt(idx);
                    int count = this.m_buildingCaps[idx];

                    if (count > 0)
                    {
                        int multiplier = unitHousingCostMultiplierForTotal;
                        int maxUpgLevel = buildingData.GetMaxUpgradeLevelForTownHallLevel(this.GetInstanceID());

                        if (maxUpgLevel >= 0)
                        {
                            int housingSpace = buildingData.GetUnitStorageCapacity(maxUpgLevel);

                            if (!buildingData.IsAllianceCastle())
                            {
                                if (buildingData.IsForgesMiniSpells() || buildingData.IsForgesSpells())
                                {
                                    multiplier = spellHousingCostMultiplierForTotal;
                                }
                                else if (buildingData.IsHeroBarrack())
                                {
                                    housingSpace = buildingData.GetHeroData().GetHousingSpace();
                                    multiplier = heroHousingCostMultiplierForTotal;
                                }
                            }
                            else
                            {
                                multiplier = allianceUnitHousingCostMultiplierForTotal;
                            }

                            if (housingSpace > 0)
                            {
                                this.m_maxHousingSpace += multiplier * count * housingSpace / 100;
                            }
                        }
                    }
                } while (++idx != dataTableCount);
            }
        }

        public int GetUnlockedBuildingCount(LogicBuildingData data)
        {
            return this.m_buildingCaps[data.GetInstanceID()];
        }

        public int GetUnlockedBuildingGearupCount(LogicBuildingData data)
        {
            return this.m_buildingGearupCaps[data.GetInstanceID()];
        }

        public int GetUnlockedTrapCount(LogicTrapData data)
        {
            return this.m_trapCaps[data.GetInstanceID()];
        }

        public LogicArrayList<int> GetTreasuryCaps()
        {
            return this.m_treasuryCaps;
        }

        public int GetStrengthMaxTroopTypes()
        {
            return this.m_strengthMaxTroopTypes;
        }

        public int GetStrengthMaxSpellTypes()
        {
            return this.m_strengthMaxSpellTypes;
        }

        public int GetPackElixir()
        {
            return this.m_packElixir;
        }

        public int GetPackGold()
        {
            return this.m_packGold;
        }

        public int GetPackDarkElixir()
        {
            return this.m_packDarkElixir;
        }

        public int GetPackGold2()
        {
            return this.m_packGold2;
        }

        public int GetPackElixir2()
        {
            return this.m_packElixir2;
        }

        public int GetDuelPrizeResourceCap()
        {
            return this.m_duelPrizeResourceCap;
        }

        public int GetChangeTroopCost()
        {
            return this.m_changeTroopCost;
        }
    }
}