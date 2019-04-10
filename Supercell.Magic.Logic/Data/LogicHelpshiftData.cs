namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicHelpshiftData : LogicData
    {
        private string m_helpshiftId;

        public LogicHelpshiftData(CSVRow row, LogicDataTable table) : base(row, table)
        {
        }

        public override void CreateReferences()
        {
            base.CreateReferences();
            this.m_helpshiftId = this.GetValue("HelpshiftId", 0);
        }
    }
}