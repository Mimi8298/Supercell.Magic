namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicAllianceLevelData : LogicData
    {
        private int m_expPoints;

        private bool m_visible;

        private int m_troopRequestCooldown;
        private int m_troopDonationLimit;
        private int m_troopDonationRefund;
        private int m_troopDonationUpgrade;
        private int m_warLootCapacityPercent;
        private int m_warLootMultiplierPercent;
        private int m_badgeLevel;

        public LogicAllianceLevelData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicAllianceLevelData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_visible = this.GetBooleanValue("IsVisible", 0);
            this.m_expPoints = this.GetIntegerValue("ExpPoints", 0);

            LogicAllianceLevelData previousLevel = null;

            if (this.GetInstanceID() > 0)
            {
                previousLevel = (LogicAllianceLevelData) this.m_table.GetItemAt(this.GetInstanceID() - 1);
            }

            this.m_troopRequestCooldown = this.GetIntegerValue("TroopRequestCooldown", 0);

            if (previousLevel != null)
            {
                if (this.m_troopRequestCooldown == 0)
                {
                    this.m_troopRequestCooldown = previousLevel.m_troopRequestCooldown;
                }
            }

            this.m_troopDonationLimit = this.GetIntegerValue("TroopDonationLimit", 0);

            if (previousLevel != null)
            {
                if (this.m_troopDonationLimit == 0)
                {
                    this.m_troopDonationLimit = previousLevel.m_troopDonationLimit;
                }
            }

            this.m_troopDonationRefund = this.GetIntegerValue("TroopDonationRefund", 0);

            if (previousLevel != null)
            {
                if (this.m_troopDonationRefund == 0)
                {
                    this.m_troopDonationRefund = previousLevel.m_troopDonationRefund;
                }
            }

            this.m_troopDonationUpgrade = this.GetIntegerValue("TroopDonationUpgrade", 0);

            if (previousLevel != null)
            {
                if (this.m_troopDonationUpgrade == 0)
                {
                    this.m_troopDonationUpgrade = previousLevel.m_troopDonationUpgrade;
                }
            }

            this.m_warLootCapacityPercent = this.GetIntegerValue("WarLootCapacityPercent", 0);

            if (previousLevel != null)
            {
                if (this.m_warLootCapacityPercent == 0)
                {
                    this.m_warLootCapacityPercent = previousLevel.m_warLootCapacityPercent;
                }
            }

            this.m_warLootMultiplierPercent = this.GetIntegerValue("WarLootMultiplierPercent", 0);

            if (previousLevel != null)
            {
                if (this.m_warLootMultiplierPercent == 0)
                {
                    this.m_warLootMultiplierPercent = previousLevel.m_warLootMultiplierPercent;
                }
            }

            this.m_badgeLevel = this.GetIntegerValue("BadgeLevel", 0);

            if (previousLevel != null)
            {
                if (this.m_badgeLevel == 0)
                {
                    this.m_badgeLevel = previousLevel.m_badgeLevel;
                }
            }
        }

        public bool IsVisible()
        {
            return this.m_visible;
        }

        public int GetExpPoints()
        {
            return this.m_expPoints;
        }

        public int GetTroopRequestCooldown()
        {
            return this.m_troopRequestCooldown;
        }

        public int GetTroopDonationLimit()
        {
            return this.m_troopDonationLimit;
        }

        public int GetTroopDonationRefund()
        {
            return this.m_troopDonationRefund;
        }

        public int GetTroopDonationUpgrade()
        {
            return this.m_troopDonationUpgrade;
        }

        public int GetWarLootCapacityPercent()
        {
            return this.m_warLootCapacityPercent;
        }

        public int GetWarLootMultiplierPercent()
        {
            return this.m_warLootMultiplierPercent;
        }

        public int GetBadgeLevel()
        {
            return this.m_badgeLevel;
        }
    }
}