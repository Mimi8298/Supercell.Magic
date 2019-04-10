namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Calendar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicLoadUnitPresetCommand : LogicCommand
    {
        private int m_presetId;
        private LogicLevel m_level;

        public LogicLoadUnitPresetCommand()
        {
            // LogicLoadUnitPresetCommand.
        }

        public LogicLoadUnitPresetCommand(int id)
        {
            this.m_presetId = id;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_presetId = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_presetId);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.LOAD_UNIT_PRESET;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_level = null;
        }

        public override int Execute(LogicLevel level)
        {
            this.m_level = level;

            if (level.GetVillageType() == 0)
            {
                if (LogicDataTables.GetGlobals().EnablePresets())
                {
                    LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                    if (homeOwnerAvatar.GetTownHallLevel() >= LogicDataTables.GetGlobals().GetEnablePresetsTownHallLevel())
                    {
                        if (this.m_presetId <= 3)
                        {
                            if (this.HasEnoughFreeHousingSpace())
                            {
                                int elixirCost = this.GetResourceCost(LogicDataTables.GetElixirData());
                                int darkElixirCost = this.GetResourceCost(LogicDataTables.GetDarkElixirData());

                                if (level.GetPlayerAvatar().HasEnoughResources(LogicDataTables.GetElixirData(), elixirCost, LogicDataTables.GetDarkElixirData(), darkElixirCost,
                                                                               true,
                                                                               this, false))
                                {
                                    this.TrainUnits();
                                    return 0;
                                }
                            }

                            return -1;
                        }

                        return -2;
                    }
                }

                return -3;
            }

            return -32;
        }

        public int GetResourceCost(LogicResourceData resourceData)
        {
            int cost = 0;

            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
            LogicCalendar calendar = this.m_level.GetCalendar();
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.CHARACTER);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicCharacterData data = (LogicCharacterData) table.GetItemAt(i);

                if (calendar.IsProductionEnabled(data) && !data.IsSecondaryTroop())
                {
                    int count = homeOwnerAvatar.GetUnitPresetCount(data, this.m_presetId);

                    if (count > 0)
                    {
                        if (data.GetTrainingResource() == resourceData)
                        {
                            cost += count * calendar.GetTrainingCost(data, homeOwnerAvatar.GetUnitUpgradeLevel(data));
                        }
                    }
                }
            }

            table = LogicDataTables.GetTable(LogicDataType.SPELL);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicSpellData data = (LogicSpellData) table.GetItemAt(i);

                if (calendar.IsProductionEnabled(data))
                {
                    int count = homeOwnerAvatar.GetUnitPresetCount(data, this.m_presetId);

                    if (count > 0)
                    {
                        if (data.GetTrainingResource() == resourceData)
                        {
                            cost += count * calendar.GetTrainingCost(data, homeOwnerAvatar.GetUnitUpgradeLevel(data));
                        }
                    }
                }
            }

            return cost;
        }

        public void TrainUnits()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
            LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);
            LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);
            LogicArrayList<LogicCombatItemData> productionUnits = new LogicArrayList<LogicCombatItemData>(characterTable.GetItemCount());
            LogicArrayList<LogicCombatItemData> productionSpells = new LogicArrayList<LogicCombatItemData>(spellTable.GetItemCount());

            for (int i = 0; i < characterTable.GetItemCount(); i++)
            {
                LogicCharacterData data = (LogicCharacterData) characterTable.GetItemAt(i);

                if (this.m_level.GetCalendar().IsProductionEnabled(data) && !data.IsSecondaryTroop())
                {
                    productionUnits.Add(data);
                }
            }

            this.SortProduction(productionUnits);

            for (int i = 0; i < productionUnits.Size(); i++)
            {
                int unitCount = homeOwnerAvatar.GetUnitPresetCount(productionUnits[i], this.m_presetId);

                if (unitCount > 0)
                {
                    this.AddUnitsToQueue(productionUnits[i], unitCount);
                }
            }

            for (int i = 0; i < spellTable.GetItemCount(); i++)
            {
                LogicSpellData data = (LogicSpellData) spellTable.GetItemAt(i);

                if (this.m_level.GetCalendar().IsProductionEnabled(data))
                {
                    productionSpells.Add(data);
                }
            }

            this.SortProduction(productionSpells);

            for (int i = 0; i < productionSpells.Size(); i++)
            {
                int spellCount = homeOwnerAvatar.GetUnitPresetCount(productionSpells[i], this.m_presetId);

                if (spellCount > 0)
                {
                    this.AddUnitsToQueue(productionSpells[i], spellCount);
                }
            }
        }

        public void AddUnitsToQueue(LogicCombatItemData data, int count)
        {
            LogicCalendar calendar = this.m_level.GetCalendar();
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
            LogicGameObjectManager gameObjectManager = this.m_level.GetGameObjectManagerAt(0);
            LogicUnitProduction production = gameObjectManager.GetUnitProduction();

            if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
            {
                if (data.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_SPELL)
                {
                    return;
                }

                production = gameObjectManager.GetSpellProduction();
            }

            if (production != null)
            {
                int trainCost = calendar.GetTrainingCost(data, homeOwnerAvatar.GetUnitUpgradeLevel(data));

                for (int i = 0; i < count; i++)
                {
                    if (production.CanAddUnitToQueue(data, true) &&
                        playerAvatar.HasEnoughResources(data.GetTrainingResource(), trainCost, false, null, false))
                    {
                        playerAvatar.CommodityCountChangeHelper(0, data.GetTrainingResource(), -trainCost);
                        production.AddUnitToQueue(data, production.GetSlotCount(), true);
                    }
                }
            }
        }

        public bool HasEnoughFreeHousingSpace()
        {
            LogicCalendar calendar = this.m_level.GetCalendar();
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
            LogicUnitProduction unitProduction = this.m_level.GetGameObjectManagerAt(0).GetUnitProduction();
            LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);

            int freeHousingSpace = unitProduction.GetMaxTrainCount() - (homeOwnerAvatar.GetUnitsTotalCapacity() - unitProduction.GetTotalCount());
            int requiredHousingSpace = 0;

            for (int i = 0; i < characterTable.GetItemCount(); i++)
            {
                LogicCharacterData data = (LogicCharacterData) characterTable.GetItemAt(i);

                if (calendar.IsProductionEnabled(data) && !data.IsSecondaryTroop())
                {
                    int count = homeOwnerAvatar.GetUnitPresetCount(data, this.m_presetId);

                    if (count > 0)
                    {
                        requiredHousingSpace += data.GetHousingSpace() * count;
                    }
                }
            }

            if (requiredHousingSpace <= freeHousingSpace)
            {
                LogicUnitProduction spellProduction = this.m_level.GetGameObjectManagerAt(0).GetSpellProduction();
                LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);

                int freeSpellHousingSpace = spellProduction.GetMaxTrainCount() - (homeOwnerAvatar.GetSpellsTotalCapacity() - spellProduction.GetTotalCount());
                int requiredSpellHousingSpace = 0;

                for (int i = 0; i < spellTable.GetItemCount(); i++)
                {
                    LogicSpellData data = (LogicSpellData) spellTable.GetItemAt(i);

                    if (calendar.IsProductionEnabled(data))
                    {
                        int count = homeOwnerAvatar.GetUnitPresetCount(data, this.m_presetId);

                        if (count > 0)
                        {
                            requiredSpellHousingSpace += data.GetHousingSpace() * count;
                        }
                    }
                }

                return requiredSpellHousingSpace <= freeSpellHousingSpace;
            }

            return false;
        }

        private void SortProduction(LogicArrayList<LogicCombatItemData> arrayList)
        {
            for (int i = 0; i < arrayList.Size(); i++)
            {
                bool change = false;

                for (int j = 0; j < arrayList.Size() - 1; j++)
                {
                    LogicCombatItemData data = arrayList[j];
                    LogicCombatItemData nextData = arrayList[j + 1];

                    int sort1 = data.GetRequiredProductionHouseLevel() + 100 * data.GetUnitOfType() + 500 * data.GetCombatItemType();
                    int sort2 = nextData.GetRequiredProductionHouseLevel() + 100 * nextData.GetUnitOfType() + 500 * nextData.GetCombatItemType();

                    if (sort1 > sort2)
                    {
                        arrayList[j] = nextData;
                        arrayList[j + 1] = data;
                        change = true;
                    }
                }

                if (!change)
                {
                    break;
                }
            }
        }
    }
}