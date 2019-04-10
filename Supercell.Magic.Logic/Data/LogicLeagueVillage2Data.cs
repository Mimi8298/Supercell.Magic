namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicLeagueVillage2Data : LogicData
    {
        private int m_trophyLimitLow;
        private int m_trophyLimitHigh;
        private int m_goldReward;
        private int m_elixirReward;
        private int m_bonusGold;
        private int m_bonusElixir;
        private int m_seasonTrophyReset;
        private int m_maxDiamondCost;

        public LogicLeagueVillage2Data(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicLeagueVillage2Data.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_trophyLimitLow = this.GetIntegerValue("TrophyLimitLow", 0);
            this.m_trophyLimitHigh = this.GetIntegerValue("TrophyLimitHigh", 0);
            this.m_goldReward = this.GetIntegerValue("GoldReward", 0);
            this.m_elixirReward = this.GetIntegerValue("ElixirReward", 0);
            this.m_bonusGold = this.GetIntegerValue("BonusGold", 0);
            this.m_bonusElixir = this.GetIntegerValue("BonusElixir", 0);
            this.m_seasonTrophyReset = this.GetIntegerValue("SeasonTrophyReset", 0);
            this.m_maxDiamondCost = this.GetIntegerValue("MaxDiamondCost", 0);
        }

        public int GetTrophyLimitLow()
        {
            return this.m_trophyLimitLow;
        }

        public int GetTrophyLimitHigh()
        {
            return this.m_trophyLimitHigh;
        }

        public int GetGoldReward()
        {
            return this.m_goldReward;
        }

        public int GetElixirReward()
        {
            return this.m_elixirReward;
        }

        public int GetBonusGold()
        {
            return this.m_bonusGold;
        }

        public int GetBonusElixir()
        {
            return this.m_bonusElixir;
        }

        public int GetSeasonTrophyReset()
        {
            return this.m_seasonTrophyReset;
        }

        public int GetMaxDiamondCost()
        {
            return this.m_maxDiamondCost;
        }
    }
}