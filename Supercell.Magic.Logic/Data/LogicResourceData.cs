namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;
    using Supercell.Magic.Titan.Debug;

    public class LogicResourceData : LogicData
    {
        private string m_resourceIconExportName;
        private string m_hudInstanceName;
        private string m_capFullTID;
        private string m_bundleIconExportName;

        private int m_stealLimitMid;
        private int m_stealLimitBig;
        private int m_textRed;
        private int m_textGreen;
        private int m_textBlue;
        private int m_villageType;

        private bool m_premiumCurrency;

        private LogicEffectData m_collectEffect;
        private LogicEffectData m_stealEffect;
        private LogicResourceData m_warResourceReferenceData;
        private LogicEffectData m_stealEffectMid;
        private LogicEffectData m_stealEffectBig;

        public LogicResourceData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicResourceData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_stealEffect = LogicDataTables.GetEffectByName(this.GetValue("StealEffect", 0), this);
            this.m_collectEffect = LogicDataTables.GetEffectByName(this.GetValue("CollectEffect", 0), this);

            this.m_resourceIconExportName = this.GetValue("ResourceIconExportName", 0);
            this.m_stealLimitMid = this.GetIntegerValue("StealLimitMid", 0);
            this.m_stealEffectMid = LogicDataTables.GetEffectByName(this.GetValue("StealEffectMid", 0), this);
            this.m_stealLimitBig = this.GetIntegerValue("StealLimitBig", 0);
            this.m_stealEffectBig = LogicDataTables.GetEffectByName(this.GetValue("StealEffectBig", 0), this);
            this.m_premiumCurrency = this.GetBooleanValue("PremiumCurrency", 0);
            this.m_hudInstanceName = this.GetValue("HudInstanceName", 0);
            this.m_capFullTID = this.GetValue("CapFullTID", 0);
            this.m_textRed = this.GetIntegerValue("TextRed", 0);
            this.m_textGreen = this.GetIntegerValue("TextGreen", 0);
            this.m_textBlue = this.GetIntegerValue("TextBlue", 0);
            this.m_bundleIconExportName = this.GetValue("BundleIconExportName", 0);
            this.m_villageType = this.GetIntegerValue("VillageType", 0);

            if ((uint) this.m_villageType >= 2)
            {
                Debugger.Error("invalid VillageType");
            }

            string warRefResource = this.GetValue("WarRefResource", 0);

            if (warRefResource.Length > 0)
            {
                this.m_warResourceReferenceData = LogicDataTables.GetResourceByName(warRefResource, this);
            }
        }

        public LogicResourceData GetWarResourceReferenceData()
        {
            return this.m_warResourceReferenceData;
        }

        public LogicEffectData GetCollectEffect()
        {
            return this.m_collectEffect;
        }

        public string GetResourceIconExportName()
        {
            return this.m_resourceIconExportName;
        }

        public LogicEffectData GetStealEffect()
        {
            return this.m_stealEffect;
        }

        public int GetStealLimitMid()
        {
            return this.m_stealLimitMid;
        }

        public LogicEffectData GetStealEffectMid()
        {
            return this.m_stealEffectMid;
        }

        public int GetStealLimitBig()
        {
            return this.m_stealLimitBig;
        }

        public LogicEffectData GetStealEffectBig()
        {
            return this.m_stealEffectBig;
        }

        public bool IsPremiumCurrency()
        {
            return this.m_premiumCurrency;
        }

        public string GetHudInstanceName()
        {
            return this.m_hudInstanceName;
        }

        public string GetCapFullTID()
        {
            return this.m_capFullTID;
        }

        public int GetTextRed()
        {
            return this.m_textRed;
        }

        public int GetTextGreen()
        {
            return this.m_textGreen;
        }

        public int GetTextBlue()
        {
            return this.m_textBlue;
        }

        public string GetBundleIconExportName()
        {
            return this.m_bundleIconExportName;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }
    }
}
