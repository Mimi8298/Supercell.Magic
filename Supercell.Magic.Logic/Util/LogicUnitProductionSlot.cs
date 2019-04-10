namespace Supercell.Magic.Logic.Util
{
    using Supercell.Magic.Logic.Data;

    public class LogicUnitProductionSlot
    {
        private LogicData m_data;

        private int m_count;
        private bool m_terminate;

        public LogicUnitProductionSlot(LogicData data, int count, bool terminate)
        {
            this.m_data = data;
            this.m_count = count;
            this.m_terminate = terminate;
        }

        public void Destruct()
        {
            this.m_data = null;
            this.m_count = 0;
        }

        public LogicData GetData()
        {
            return this.m_data;
        }

        public int GetCount()
        {
            return this.m_count;
        }

        public void SetCount(int count)
        {
            this.m_count = count;
        }

        public bool IsTerminate()
        {
            return this.m_terminate;
        }

        public void SetTerminate(bool terminate)
        {
            this.m_terminate = terminate;
        }
    }
}