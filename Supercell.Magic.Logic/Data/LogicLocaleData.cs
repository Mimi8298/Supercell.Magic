namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicLocaleData : LogicData
    {
        private string m_fileName;
        private string m_localizedName;
        private string m_usedSystemFont;
        private string m_helpshiftSDKLanguage;
        private string m_helpshiftSDKLanguageAndroid;
        private string m_boomboxUrl;
        private string m_boomboxStagingUrl;
        private string m_helpshiftLanguageTagOverride;

        private int m_sortOrder;

        private bool m_hasEvenSpaceCharacters;
        private bool m_isRTL;
        private bool m_testLanguage;
        private bool m_boomboxEnabled;

        public LogicLocaleData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicLocaleData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_fileName = this.GetValue("FileName", 0);
            this.m_localizedName = this.GetValue("LocalizedName", 0);
            this.m_hasEvenSpaceCharacters = this.GetBooleanValue("HasEvenSpaceCharacters", 0);
            this.m_isRTL = this.GetBooleanValue("isRTL", 0);
            this.m_usedSystemFont = this.GetValue("UsedSystemFont", 0);
            this.m_helpshiftSDKLanguage = this.GetValue("HelpshiftSDKLanguage", 0);
            this.m_helpshiftSDKLanguageAndroid = this.GetValue("HelpshiftSDKLanguageAndroid", 0);
            this.m_sortOrder = this.GetIntegerValue("SortOrder", 0);
            this.m_testLanguage = this.GetBooleanValue("TestLanguage", 0);
            this.m_boomboxEnabled = this.GetBooleanValue("BoomboxEnabled", 0);
            this.m_boomboxUrl = this.GetValue("BoomboxUrl", 0);
            this.m_boomboxStagingUrl = this.GetValue("BoomboxStagingUrl", 0);
            this.m_helpshiftLanguageTagOverride = this.GetValue("HelpshiftLanguageTagOverride", 0);
        }

        public string GetFileName()
        {
            return this.m_fileName;
        }

        public string GetLocalizedName()
        {
            return this.m_localizedName;
        }

        public bool IsHasEvenSpaceCharacters()
        {
            return this.m_hasEvenSpaceCharacters;
        }

        public bool IsRTL()
        {
            return this.m_isRTL;
        }

        public string GetUsedSystemFont()
        {
            return this.m_usedSystemFont;
        }

        public string GetHelpshiftSDKLanguage()
        {
            return this.m_helpshiftSDKLanguage;
        }

        public string GetHelpshiftSDKLanguageAndroid()
        {
            return this.m_helpshiftSDKLanguageAndroid;
        }

        public int GetSortOrder()
        {
            return this.m_sortOrder;
        }

        public bool IsTestLanguage()
        {
            return this.m_testLanguage;
        }

        public bool IsBoomboxEnabled()
        {
            return this.m_boomboxEnabled;
        }

        public string GetBoomboxUrl()
        {
            return this.m_boomboxUrl;
        }

        public string GetBoomboxStagingUrl()
        {
            return this.m_boomboxStagingUrl;
        }

        public string GetHelpshiftLanguageTagOverride()
        {
            return this.m_helpshiftLanguageTagOverride;
        }
    }
}