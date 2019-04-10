namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicShieldData : LogicData
    {
        private int m_diamondsCost;
        private int m_cooldownSecs;
        private int m_scoreLimit;

        private int m_timeHours;
        private int m_guardTimeHours;

        public LogicShieldData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicShieldData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_diamondsCost = this.GetIntegerValue("Diamonds", 0);
            this.m_cooldownSecs = this.GetIntegerValue("CooldownS", 0);
            this.m_scoreLimit = this.GetIntegerValue("LockedAboveScore", 0);
            this.m_timeHours = this.GetIntegerValue("TimeH", 0);
            this.m_guardTimeHours = this.GetIntegerValue("GuardTimeH", 0);
        }

        public int GetDiamondsCost()
        {
            return this.m_diamondsCost;
        }

        public int GetCooldownSecs()
        {
            return this.m_cooldownSecs;
        }

        public int GetScoreLimit()
        {
            return this.m_scoreLimit;
        }

        public int GetTimeHours()
        {
            return this.m_timeHours;
        }

        public int GetGuardTimeHours()
        {
            return this.m_guardTimeHours;
        }
    }
}