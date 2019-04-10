namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicHintData : LogicData
    {
        public LogicHintData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicHintData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();
        }
    }
}