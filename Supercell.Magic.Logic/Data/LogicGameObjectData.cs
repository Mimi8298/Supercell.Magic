namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;

    public abstract class LogicGameObjectData : LogicData
    {
        protected int m_villageType;

        protected LogicGameObjectData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            this.m_villageType = -1;
        }

        public override void CreateReferences()
        {
            base.CreateReferences();
            int columnIndex = this.m_row.GetColumnIndexByName("VillageType");

            if (columnIndex > 0)
            {
                this.m_villageType = this.m_row.GetIntegerValueAt(columnIndex, 0);

                if (this.m_villageType >= 2)
                {
                    Debugger.Error("invalid VillageType");
                }
            }
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public bool IsEnabledInVillageType(int villageType)
        {
            return this.m_villageType == -1 || this.m_villageType == villageType;
        }
    }
}