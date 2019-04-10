namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicExperienceLevelData : LogicData
    {
        private int m_expPoints;

        public LogicExperienceLevelData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicExperienceLevelData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();
            this.m_expPoints = this.GetIntegerValue("ExpPoints", 0);
        }

        public int GetMaxExpPoints()
        {
            return this.m_expPoints;
        }

        public static int GetLevelCap()
        {
            return LogicDataTables.GetTable(LogicDataType.EXPERIENCE_LEVEL).GetItemCount();
        }
    }
}