namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicAnimationTable : LogicDataTable
    {
        public LogicAnimationTable(CSVNode node, LogicDataType index) : base(node.GetTable(), index)
        {
        }

        public override void CreateReferences()
        {
            for (int i = 0; i < this.m_items.Size(); i++)
            {
                this.m_items[i].CreateReferences();
            }
        }

        public void SetTable(CSVNode node)
        {
            // TODO: Implement this.
        }
    }
}