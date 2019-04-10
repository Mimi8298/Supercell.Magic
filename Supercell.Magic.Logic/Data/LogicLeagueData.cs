namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicLeagueData : LogicData
    {
        private string m_leagueBannerIcon;
        private string m_leagueBannerIconNum;
        private string m_leagueBannerIconHUD;

        private int m_goldReward;
        private int m_elixirReward;
        private int m_darkElixirReward;
        private int m_goldRewardStarBonus;
        private int m_elixirRewardStarBonus;
        private int m_darkElixirRewardStarBonus;
        private int m_placementLimitLow;
        private int m_placementLimitHigh;
        private int m_demoteLimit;
        private int m_promoteLimit;
        private int m_allocateAmount;
        private int m_saverCount;
        private int m_villageGuardInMins;

        private int[] m_bucketPlacementRangeLow;
        private int[] m_bucketPlacementRangeHigh;
        private int[] m_bucketPlacementSoftLimit;
        private int[] m_bucketPlacementHardLimit;

        private bool m_useStarBonus;
        private bool m_ignoredByServer;
        private bool m_demoteEnabled;
        private bool m_promoteEnabled;

        public LogicLeagueData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicLeagueData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_leagueBannerIcon = this.GetValue("LeagueBannerIcon", 0);
            this.m_leagueBannerIconNum = this.GetValue("LeagueBannerIconNum", 0);
            this.m_leagueBannerIconHUD = this.GetValue("LeagueBannerIconHUD", 0);
            this.m_goldReward = this.GetIntegerValue("GoldReward", 0);
            this.m_elixirReward = this.GetIntegerValue("ElixirReward", 0);
            this.m_darkElixirReward = this.GetIntegerValue("DarkElixirReward", 0);
            this.m_useStarBonus = this.GetBooleanValue("UseStarBonus", 0);
            this.m_goldRewardStarBonus = this.GetIntegerValue("GoldRewardStarBonus", 0);
            this.m_elixirRewardStarBonus = this.GetIntegerValue("ElixirRewardStarBonus", 0);
            this.m_darkElixirRewardStarBonus = this.GetIntegerValue("DarkElixirRewardStarBonus", 0);
            this.m_placementLimitLow = this.GetIntegerValue("PlacementLimitLow", 0);
            this.m_placementLimitHigh = this.GetIntegerValue("PlacementLimitHigh", 0);
            this.m_demoteLimit = this.GetIntegerValue("DemoteLimit", 0);
            this.m_promoteLimit = this.GetIntegerValue("PromoteLimit", 0);
            this.m_ignoredByServer = this.GetBooleanValue("IgnoredByServer", 0);
            this.m_demoteEnabled = this.GetBooleanValue("DemoteEnabled", 0);
            this.m_promoteEnabled = this.GetBooleanValue("PromoteEnabled", 0);
            this.m_allocateAmount = this.GetIntegerValue("AllocateAmount", 0);
            this.m_saverCount = this.GetIntegerValue("SaverCount", 0);
            this.m_villageGuardInMins = this.GetIntegerValue("VillageGuardInMins", 0);

            int size = this.m_row.GetBiggestArraySize();

            this.m_bucketPlacementRangeLow = new int[size];
            this.m_bucketPlacementRangeHigh = new int[size];
            this.m_bucketPlacementSoftLimit = new int[size];
            this.m_bucketPlacementHardLimit = new int[size];

            for (int i = 0; i < size; i++)
            {
                this.m_bucketPlacementRangeLow[i] = this.GetIntegerValue("BucketPlacementRangeLow", i);
                this.m_bucketPlacementRangeHigh[i] = this.GetIntegerValue("BucketPlacementRangeHigh", i);
                this.m_bucketPlacementSoftLimit[i] = this.GetIntegerValue("BucketPlacementSoftLimit", i);
                this.m_bucketPlacementHardLimit[i] = this.GetIntegerValue("BucketPlacementHardLimit", i);
            }
        }

        public int GetBucketPlacementRangeLow(int index)
        {
            return this.m_bucketPlacementRangeLow[index];
        }

        public int GetBucketPlacementRangeHigh(int index)
        {
            return this.m_bucketPlacementRangeHigh[index];
        }

        public int GetBucketPlacementSoftLimit(int index)
        {
            return this.m_bucketPlacementSoftLimit[index];
        }

        public int GetBucketPlacementHardLimit(int index)
        {
            return this.m_bucketPlacementHardLimit[index];
        }

        public string GetLeagueBannerIcon()
        {
            return this.m_leagueBannerIcon;
        }

        public string GetLeagueBannerIconNum()
        {
            return this.m_leagueBannerIconNum;
        }

        public string GetLeagueBannerIconHUD()
        {
            return this.m_leagueBannerIconHUD;
        }

        public int GetGoldReward()
        {
            return this.m_goldReward;
        }

        public int GetElixirReward()
        {
            return this.m_elixirReward;
        }

        public int GetDarkElixirReward()
        {
            return this.m_darkElixirReward;
        }

        public bool IsUseStarBonus()
        {
            return this.m_useStarBonus;
        }

        public int GetGoldRewardStarBonus()
        {
            return this.m_goldRewardStarBonus;
        }

        public int GetElixirRewardStarBonus()
        {
            return this.m_elixirRewardStarBonus;
        }

        public int GetDarkElixirRewardStarBonus()
        {
            return this.m_darkElixirRewardStarBonus;
        }

        public int GetPlacementLimitLow()
        {
            return this.m_placementLimitLow;
        }

        public int GetPlacementLimitHigh()
        {
            return this.m_placementLimitHigh;
        }

        public int GetDemoteLimit()
        {
            return this.m_demoteLimit;
        }

        public int GetPromoteLimit()
        {
            return this.m_promoteLimit;
        }

        public bool IsIgnoredByServer()
        {
            return this.m_ignoredByServer;
        }

        public bool IsDemoteEnabled()
        {
            return this.m_demoteEnabled;
        }

        public bool IsPromoteEnabled()
        {
            return this.m_promoteEnabled;
        }

        public int GetAllocateAmount()
        {
            return this.m_allocateAmount;
        }

        public int GetSaverCount()
        {
            return this.m_saverCount;
        }

        public int GetVillageGuardInMins()
        {
            return this.m_villageGuardInMins;
        }
    }
}