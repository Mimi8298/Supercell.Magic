namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicDataTable
    {
        private LogicDataType m_tableIndex;
        private string m_tableName;
        private bool m_loaded;
        private bool m_loaded2;

        protected CSVTable m_table;
        protected LogicArrayList<LogicData> m_items;

        public LogicDataTable(CSVTable table, LogicDataType index)
        {
            this.m_tableIndex = index;
            this.m_table = table;
            this.m_items = new LogicArrayList<LogicData>();

            this.LoadTable();
        }

        public void Destruct()
        {
            this.m_items.Destruct();
            this.m_tableName = null;
            this.m_tableIndex = 0;
        }

        public void LoadTable()
        {
            for (int i = 0, j = this.m_table.GetRowCount(); i < j; i++)
            {
                this.AddItem(this.m_table.GetRowAt(i));
            }
        }

        public void SetTable(CSVTable table)
        {
            this.m_table = table;

            for (int i = 0; i < this.m_items.Size(); i++)
            {
                this.m_items[i].SetCSVRow(table.GetRowAt(i));
            }
        }

        public void AddItem(CSVRow row)
        {
            this.m_items.Add(this.CreateItem(row));
        }

        public LogicData CreateItem(CSVRow row)
        {
            LogicData data = null;

            switch (this.m_tableIndex)
            {
                case LogicDataType.BUILDING:
                {
                    data = new LogicBuildingData(row, this);
                    break;
                }

                case LogicDataType.LOCALE:
                {
                    data = new LogicLocaleData(row, this);
                    break;
                }

                case LogicDataType.RESOURCE:
                {
                    data = new LogicResourceData(row, this);
                    break;
                }

                case LogicDataType.CHARACTER:
                {
                    data = new LogicCharacterData(row, this);
                    break;
                }

                case LogicDataType.ANIMATION:
                {
                    data = new LogicAnimationData(row, this);
                    break;
                }

                case LogicDataType.PROJECTILE:
                {
                    data = new LogicProjectileData(row, this);
                    break;
                }

                case LogicDataType.BUILDING_CLASS:
                {
                    data = new LogicBuildingClassData(row, this);
                    break;
                }

                case LogicDataType.OBSTACLE:
                {
                    data = new LogicObstacleData(row, this);
                    break;
                }

                case LogicDataType.EFFECT:
                {
                    data = new LogicEffectData(row, this);
                    break;
                }

                case LogicDataType.PARTICLE_EMITTER:
                {
                    data = new LogicParticleEmitterData(row, this);
                    break;
                }

                case LogicDataType.EXPERIENCE_LEVEL:
                {
                    data = new LogicExperienceLevelData(row, this);
                    break;
                }

                case LogicDataType.TRAP:
                {
                    data = new LogicTrapData(row, this);
                    break;
                }

                case LogicDataType.ALLIANCE_BADGE:
                {
                    data = new LogicAllianceBadgeData(row, this);
                    break;
                }

                case LogicDataType.GLOBAL:
                case LogicDataType.CLIENT_GLOBAL:
                {
                    data = new LogicGlobalData(row, this);
                    break;
                }

                case LogicDataType.TOWNHALL_LEVEL:
                {
                    data = new LogicTownhallLevelData(row, this);
                    break;
                }

                case LogicDataType.ALLIANCE_PORTAL:
                {
                    data = new LogicAlliancePortalData(row, this);
                    break;
                }

                case LogicDataType.NPC:
                {
                    data = new LogicNpcData(row, this);
                    break;
                }

                case LogicDataType.DECO:
                {
                    data = new LogicDecoData(row, this);
                    break;
                }

                case LogicDataType.RESOURCE_PACK:
                {
                    data = new LogicResourcePackData(row, this);
                    break;
                }

                case LogicDataType.SHIELD:
                {
                    data = new LogicShieldData(row, this);
                    break;
                }

                case LogicDataType.MISSION:
                {
                    data = new LogicMissionData(row, this);
                    break;
                }

                case LogicDataType.BILLING_PACKAGE:
                {
                    data = new LogicBillingPackageData(row, this);
                    break;
                }

                case LogicDataType.ACHIEVEMENT:
                {
                    data = new LogicAchievementData(row, this);
                    break;
                }

                case LogicDataType.SPELL:
                {
                    data = new LogicSpellData(row, this);
                    break;
                }

                case LogicDataType.HINT:
                {
                    data = new LogicHintData(row, this);
                    break;
                }

                case LogicDataType.HERO:
                {
                    data = new LogicHeroData(row, this);
                    break;
                }

                case LogicDataType.LEAGUE:
                {
                    data = new LogicLeagueData(row, this);
                    break;
                }

                case LogicDataType.NEWS:
                {
                    data = new LogicNewsData(row, this);
                    break;
                }

                case LogicDataType.WAR:
                {
                    data = new LogicWarData(row, this);
                    break;
                }

                case LogicDataType.REGION:
                {
                    data = new LogicRegionData(row, this);
                    break;
                }

                case LogicDataType.ALLIANCE_BADGE_LAYER:
                {
                    data = new LogicAllianceBadgeLayerData(row, this);
                    break;
                }

                case LogicDataType.ALLIANCE_LEVEL:
                {
                    data = new LogicAllianceLevelData(row, this);
                    break;
                }

                case LogicDataType.HELPSHIFT:
                {
                    data = new LogicHelpshiftData(row, this);
                    break;
                }

                case LogicDataType.CREDIT:
                case LogicDataType.FAQ:
                case LogicDataType.VARIABLE:
                {
                    data = new LogicData(row, this);
                    break;
                }

                case LogicDataType.GEM_BUNDLE:
                {
                    data = new LogicGemBundleData(row, this);
                    break;
                }

                case LogicDataType.VILLAGE_OBJECT:
                {
                    data = new LogicVillageObjectData(row, this);
                    break;
                }

                case LogicDataType.CALENDAR_EVENT_FUNCTION:
                {
                    data = new LogicCalendarEventFunctionData(row, this);
                    break;
                }

                case LogicDataType.BOOMBOX:
                {
                    data = new LogicBoomboxData(row, this);
                    break;
                }

                case LogicDataType.EVENT_ENTRY:
                {
                    data = new LogicEventEntryData(row, this);
                    break;
                }

                case LogicDataType.DEEPLINK:
                {
                    data = new LogicDeeplinkData(row, this);
                    break;
                }

                case LogicDataType.LEAGUE_VILLAGE2:
                {
                    data = new LogicLeagueVillage2Data(row, this);
                    break;
                }

                default:
                {
                    Debugger.Error("Invalid data table id: " + this.m_tableIndex);
                    break;
                }
            }

            return data;
        }

        public virtual void CreateReferences()
        {
            if (LogicDataTables.CanReloadTable(this) || !this.m_loaded)
            {
                for (int i = 0; i < this.m_items.Size(); i++)
                {
                    this.m_items[i].CreateReferences();
                }

                this.m_loaded = true;
            }
        }

        public virtual void CreateReferences2()
        {
            if (LogicDataTables.CanReloadTable(this) || !this.m_loaded2)
            {
                for (int i = 0; i < this.m_items.Size(); i++)
                {
                    this.m_items[i].CreateReferences2();
                }

                this.m_loaded2 = true;
            }
        }

        public LogicData GetItemAt(int index)
        {
            return this.m_items[index];
        }

        public LogicData GetDataByName(string name, LogicData caller)
        {
            if (!string.IsNullOrEmpty(name))
            {
                for (int i = 0; i < this.m_items.Size(); i++)
                {
                    LogicData data = this.m_items[i];

                    if (data.GetName().Equals(name))
                    {
                        return data;
                    }
                }

                if (caller != null)
                {
                    Debugger.Warning(string.Format("CSV row ({0}) has an invalid reference ({1})", caller.GetName(), name));
                }
            }

            return null;
        }

        public LogicData GetItemById(int globalId)
        {
            int instanceId = GlobalID.GetInstanceID(globalId);

            if (instanceId < 0 || instanceId >= this.m_items.Size())
            {
                Debugger.Warning("LogicDataTable::getItemById() - Instance id out of bounds! " + (instanceId + 1) + "/" + this.m_items.Size());
                return null;
            }

            return this.m_items[instanceId];
        }

        public int GetItemCount()
        {
            return this.m_items.Size();
        }

        public LogicDataType GetTableIndex()
        {
            return this.m_tableIndex;
        }

        public string GetTableName()
        {
            return this.m_tableName;
        }

        public void SetName(string name)
        {
            this.m_tableName = name;
        }
    }
}