namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Titan.Util;

    public class LogicCalendarUseTroop
    {
        private readonly LogicCombatItemData m_data;
        private readonly LogicArrayList<int> m_parameters;

        public LogicCalendarUseTroop(LogicCombatItemData data)
        {
            this.m_data = data;
            this.m_parameters = new LogicArrayList<int>();
        }

        public LogicCombatItemData GetData()
        {
            return this.m_data;
        }

        public void AddParameter(int parameter)
        {
            this.m_parameters.Add(parameter);
        }

        public int GetParameter(int idx)
        {
            return this.m_parameters[idx];
        }
    }
}