namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicBoomboxData : LogicData
    {
        private bool m_enabled;
        private bool m_enabledLowMemory;
        private bool m_preLoading;
        private bool m_preLoadingLowMemory;

        private string[] m_disabledDevices;
        private string[] m_supportedPlatforms;
        private string[] m_supportedPlatformsVersion;
        private string[] m_allowedDomains;
        private string[] m_allowedUrls;

        public LogicBoomboxData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicBoomboxData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_enabled = this.GetBooleanValue("Enabled", 0);
            this.m_enabledLowMemory = this.GetBooleanValue("EnabledLowMem", 0);
            this.m_preLoading = this.GetBooleanValue("PreLoading", 0);
            this.m_preLoadingLowMemory = this.GetBooleanValue("PreLoadingLowMem", 0);

            this.m_disabledDevices = new string[this.GetArraySize("DisabledDevices")];

            for (int i = 0; i < this.m_disabledDevices.Length; i++)
            {
                this.m_disabledDevices[i] = this.GetValue("DisabledDevices", i);
            }

            this.m_supportedPlatforms = new string[this.GetArraySize("SupportedPlatforms")];

            for (int i = 0; i < this.m_supportedPlatforms.Length; i++)
            {
                this.m_supportedPlatforms[i] = this.GetValue("SupportedPlatforms", i);
            }

            this.m_supportedPlatformsVersion = new string[this.GetArraySize("SupportedPlatformsVersion")];

            for (int i = 0; i < this.m_supportedPlatformsVersion.Length; i++)
            {
                this.m_supportedPlatformsVersion[i] = this.GetValue("SupportedPlatformsVersion", i);
            }

            this.m_allowedDomains = new string[this.GetArraySize("AllowedDomains")];

            for (int i = 0; i < this.m_allowedDomains.Length; i++)
            {
                this.m_allowedDomains[i] = this.GetValue("AllowedDomains", i);
            }

            this.m_allowedUrls = new string[this.GetArraySize("AllowedUrls")];

            for (int i = 0; i < this.m_allowedUrls.Length; i++)
            {
                this.m_allowedUrls[i] = this.GetValue("AllowedUrls", i);
            }
        }
    }
}