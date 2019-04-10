namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;

    public class LogicData
    {
        protected readonly int m_globalId;
        
        protected short m_tidIndex;
        protected short m_infoTidIndex;
        protected short m_iconExportNameIndex;
        protected short m_iconSWFIndex;

        protected CSVRow m_row;
        protected readonly LogicDataTable m_table;

        public LogicData(CSVRow row, LogicDataTable table)
        {
            this.m_row = row;
            this.m_table = table;
            this.m_globalId = GlobalID.CreateGlobalID((int) table.GetTableIndex() + 1, table.GetItemCount());
            
            this.m_tidIndex = -1;
            this.m_infoTidIndex = -1;
            this.m_iconSWFIndex = -1;
            this.m_iconExportNameIndex = -1;
        }
        
        public virtual void CreateReferences()
        {
            this.m_iconSWFIndex = (short) this.m_row.GetColumnIndexByName("IconSWF");
            this.m_iconExportNameIndex = (short) this.m_row.GetColumnIndexByName("IconExportName");
            this.m_tidIndex = (short) this.m_row.GetColumnIndexByName("TID");
            this.m_infoTidIndex = (short) this.m_row.GetColumnIndexByName("InfoTID");
        }

        public virtual void CreateReferences2()
        {
        }

        public void SetCSVRow(CSVRow row)
        {
            this.m_row = row;
        }

        public int GetArraySize(string column)
        {
            return this.m_row.GetArraySize(column);
        }

        public LogicDataType GetDataType()
        {
            return this.m_table.GetTableIndex();
        }

        public int GetGlobalID()
        {
            return this.m_globalId;
        }

        public int GetInstanceID()
        {
            return GlobalID.GetInstanceID(this.m_globalId);
        }

        public int GetColumnIndex(string name)
        {
            int columnIndex = this.m_row.GetColumnIndexByName(name);

            if (columnIndex == -1)
            {
                Debugger.Warning(string.Format("Unable to find column {0} from {1} ({2})", name, this.m_row.GetName(), this.m_table.GetTableName()));
            }

            return this.m_row.GetColumnIndexByName(name);
        }

        public string GetDebuggerName()
        {
            return this.m_row.GetName() + " (" + this.m_table.GetTableName() + ")";
        }

        public bool GetBooleanValue(string columnName, int index)
        {
            return this.m_row.GetBooleanValue(columnName, index);
        }

        public bool GetClampedBooleanValue(string columnName, int index)
        {
            return this.m_row.GetClampedBooleanValue(columnName, index);
        }

        public int GetIntegerValue(string columnName, int index)
        {
            return this.m_row.GetIntegerValue(columnName, index);
        }

        public int GetClampedIntegerValue(string columnName, int index)
        {
            return this.m_row.GetClampedIntegerValue(columnName, index);
        }

        public string GetValue(string columnName, int index)
        {
            return this.m_row.GetValue(columnName, index);
        }

        public string GetClampedValue(string columnName, int index)
        {
            return this.m_row.GetClampedValue(columnName, index);
        }

        public string GetName()
        {
            return this.m_row.GetName();
        }

        public string GetTID()
        {
            if (this.m_tidIndex == -1)
                return null;
            return this.m_row.GetValueAt(this.m_tidIndex, 0);
        }

        public string GetInfoTID()
        {
            if (this.m_infoTidIndex == -1)
                return null;
            return this.m_row.GetValueAt(this.m_infoTidIndex, 0);
        }

        public string GetIconExportName()
        {
            if (this.m_iconExportNameIndex == -1)
                return null;
            return this.m_row.GetValueAt(this.m_iconExportNameIndex, 0);
        }
        
        public virtual bool IsEnableByCalendar()
        {
            return false;
        }
    }

    public enum LogicDataType
    {
        BUILDING = 0,
        LOCALE = 1,
        RESOURCE = 2,
        CHARACTER = 3,
        ANIMATION = 4,
        PROJECTILE = 5,
        BUILDING_CLASS = 6,
        OBSTACLE = 7,
        EFFECT = 8,
        PARTICLE_EMITTER = 9,
        EXPERIENCE_LEVEL = 10,
        TRAP = 11,
        ALLIANCE_BADGE = 12,
        GLOBAL = 13,
        TOWNHALL_LEVEL = 14,
        ALLIANCE_PORTAL = 15,
        NPC = 16,
        DECO = 17,
        RESOURCE_PACK = 18,
        SHIELD = 19,
        MISSION = 20,
        BILLING_PACKAGE = 21,
        ACHIEVEMENT = 22,
        CREDIT = 23,
        FAQ = 24,
        SPELL = 25,
        HINT = 26,
        HERO = 27,
        LEAGUE = 28,
        NEWS = 29,
        WAR = 30,
        REGION = 31,
        CLIENT_GLOBAL = 32,
        ALLIANCE_BADGE_LAYER = 33,
        ALLIANCE_LEVEL = 34,
        HELPSHIFT = 35,
        VARIABLE = 36,
        GEM_BUNDLE = 37,
        VILLAGE_OBJECT = 38,
        CALENDAR_EVENT_FUNCTION = 39,
        BOOMBOX = 40,
        EVENT_ENTRY = 41,
        DEEPLINK = 42,
        LEAGUE_VILLAGE2 = 43
    }
}