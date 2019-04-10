namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicSaveUnitPresetCommand : LogicCommand
    {
        private int m_presetId;

        private LogicLevel m_level;
        private readonly LogicArrayList<LogicDataSlot> m_slots;

        public LogicSaveUnitPresetCommand()
        {
            this.m_slots = new LogicArrayList<LogicDataSlot>();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_presetId = stream.ReadInt();

            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                LogicDataSlot slot = new LogicDataSlot(null, 0);
                slot.Decode(stream);
                this.m_slots.Add(slot);
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_presetId);
            encoder.WriteInt(this.m_slots.Size());

            for (int i = 0; i < this.m_slots.Size(); i++)
            {
                this.m_slots[i].Encode(encoder);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SAVE_UNIT_PRESET;
        }

        public override void Destruct()
        {
            base.Destruct();

            while (this.m_slots.Size() > 0)
            {
                this.m_slots[0].Destruct();
                this.m_slots.Remove(0);
            }

            this.m_level = null;
        }

        public override int Execute(LogicLevel level)
        {
            this.m_level = level;

            if (LogicDataTables.GetGlobals().EnablePresets())
            {
                LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                if (homeOwnerAvatar.GetTownHallLevel() >= LogicDataTables.GetGlobals().GetEnablePresetsTownHallLevel())
                {
                    if (this.m_presetId <= 3)
                    {
                        LogicDataTable table = LogicDataTables.GetTable(LogicDataType.CHARACTER);
                        LogicComponentManager componentManager = level.GetComponentManager();

                        int totalMaxHousing = componentManager.GetTotalMaxHousing(0);

                        for (int i = 0, housingSpace = 0; i < table.GetItemCount(); i++)
                        {
                            LogicCharacterData data = (LogicCharacterData) table.GetItemAt(i);

                            if (level.GetGameMode().GetCalendar().IsProductionEnabled(data) && !data.IsSecondaryTroop())
                            {
                                int count = 0;

                                if (this.m_slots.Size() > 0)
                                {
                                    for (int j = 0; j < this.m_slots.Size(); j++)
                                    {
                                        if (this.m_slots[j].GetData() == data)
                                        {
                                            count = this.m_slots[j].GetCount();
                                            break;
                                        }
                                    }
                                }

                                housingSpace += count * data.GetHousingSpace();

                                if (housingSpace > totalMaxHousing || !this.IsUnlocked(data))
                                {
                                    this.SetUnitPresetCount(data, 0);
                                }
                                else
                                {
                                    this.SetUnitPresetCount(data, count);
                                }
                            }
                        }

                        table = LogicDataTables.GetTable(LogicDataType.SPELL);
                        totalMaxHousing = componentManager.GetTotalMaxHousing(0);

                        for (int i = 0, housingSpace = 0; i < table.GetItemCount(); i++)
                        {
                            LogicSpellData data = (LogicSpellData) table.GetItemAt(i);

                            if (level.GetGameMode().GetCalendar().IsProductionEnabled(data))
                            {
                                int count = 0;

                                if (this.m_slots.Size() > 0)
                                {
                                    for (int j = 0; j < this.m_slots.Size(); j++)
                                    {
                                        if (this.m_slots[j].GetData() == data)
                                        {
                                            count = this.m_slots[j].GetCount();
                                            break;
                                        }
                                    }
                                }

                                housingSpace += count * data.GetHousingSpace();

                                if (housingSpace > totalMaxHousing || !this.IsUnlocked(data))
                                {
                                    this.SetUnitPresetCount(data, 0);
                                }
                                else
                                {
                                    this.SetUnitPresetCount(data, count);
                                }
                            }
                        }

                        return 0;
                    }

                    return -2;
                }
            }

            return -1;
        }

        public bool IsUnlocked(LogicCombatItemData data)
        {
            return data.IsUnlockedForProductionHouseLevel(this.m_level.GetGameObjectManager().GetHighestBuildingLevel(data.GetProductionHouseData()));
        }

        public void SetUnitPresetCount(LogicCombatItemData data, int count)
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

            if (homeOwnerAvatar.GetUnitPresetCount(data, this.m_presetId) != count)
            {
                homeOwnerAvatar.SetUnitPresetCount(data, this.m_presetId, count);
                homeOwnerAvatar.GetChangeListener().CommodityCountChanged(3, data, count);
            }
        }
    }
}