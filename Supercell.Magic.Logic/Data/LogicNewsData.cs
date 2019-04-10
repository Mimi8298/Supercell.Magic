namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicNewsData : LogicData
    {
        private string m_type;
        private string m_buttonTID;
        private string m_actionType;
        private string m_actionParameter1;
        private string m_actionParameter2;
        private string m_nativeAndroidURL;
        private string m_includedCountries;
        private string m_excludedCountries;
        private string m_excludedLoginCountries;
        private string m_itemSWF;
        private string m_itemExportName;
        private string m_iconSWF;
        private string m_iconExportName;
        private string m_minOS;
        private string m_maxOS;
        private string m_buttonTID2;
        private string m_action2Type;
        private string m_action2Parameter1;
        private string m_action2Parameter2;

        private int m_id;
        private int m_iconFrame;
        private int m_minTownHall;
        private int m_maxTownHall;
        private int m_minLevel;
        private int m_maxLevel;
        private int m_maxDiamonds;
        private int m_notifyMinLevel;
        private int m_avatarIdModulo;
        private int m_moduloMin;
        private int m_moduloMax;

        private bool m_enabled;
        private bool m_enabledIOS;
        private bool m_enabledAndroid;
        private bool m_enabledKunlun;
        private bool m_enabledTencent;
        private bool m_enabledLowEnd;
        private bool m_enabledHighEnd;
        private bool m_showAsNew;
        private bool m_centerText;
        private bool m_loadResources;
        private bool m_loadInLowEnd;
        private bool m_animateIcon;
        private bool m_centerIcon;
        private bool m_clickToDismiss;
        private bool m_notifyAlways;
        private bool m_collapsed;

        public LogicNewsData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicNewsData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_id = this.GetIntegerValue("ID", 0);
            this.m_enabled = this.GetBooleanValue("Enabled", 0);
            this.m_enabledIOS = this.GetBooleanValue("EnabledIOS", 0);
            this.m_enabledAndroid = this.GetBooleanValue("EnabledAndroid", 0);
            this.m_enabledKunlun = this.GetBooleanValue("EnabledKunlun", 0);
            this.m_enabledTencent = this.GetBooleanValue("EnabledTencent", 0);
            this.m_enabledLowEnd = this.GetBooleanValue("EnabledLowEnd", 0);
            this.m_enabledHighEnd = this.GetBooleanValue("EnabledHighEnd", 0);
            this.m_type = this.GetValue("Type", 0);
            this.m_showAsNew = this.GetBooleanValue("ShowAsNew", 0);
            this.m_buttonTID = this.GetValue("ButtonTID", 0);
            this.m_actionType = this.GetValue("ActionType", 0);
            this.m_actionParameter1 = this.GetValue("ActionParameter1", 0);
            this.m_actionParameter2 = this.GetValue("ActionParameter2", 0);
            this.m_nativeAndroidURL = this.GetValue("NativeAndroidURL", 0);
            this.m_includedCountries = this.GetValue("IncludedCountries", 0);
            this.m_excludedCountries = this.GetValue("ExcludedCountries", 0);
            this.m_excludedLoginCountries = this.GetValue("ExcludedLoginCountries", 0);
            this.m_centerText = this.GetBooleanValue("CenterText", 0);
            this.m_loadResources = this.GetBooleanValue("LoadResources", 0);
            this.m_loadInLowEnd = this.GetBooleanValue("LoadInLowEnd", 0);
            this.m_itemSWF = this.GetValue("ItemSWF", 0);
            this.m_itemExportName = this.GetValue("ItemExportName", 0);
            this.m_iconSWF = this.GetValue("IconSWF", 0);
            this.m_iconExportName = this.GetValue("IconExportName", 0);
            this.m_iconFrame = this.GetIntegerValue("IconFrame", 0);
            this.m_animateIcon = this.GetBooleanValue("AnimateIcon", 0);
            this.m_centerIcon = this.GetBooleanValue("CenterIcon", 0);
            this.m_minTownHall = this.GetIntegerValue("MinTownHall", 0);
            this.m_maxTownHall = this.GetIntegerValue("MaxTownHall", 0);
            this.m_minLevel = this.GetIntegerValue("MinLevel", 0);
            this.m_maxLevel = this.GetIntegerValue("MaxLevel", 0);
            this.m_maxDiamonds = this.GetIntegerValue("MaxDiamonds", 0);
            this.m_clickToDismiss = this.GetBooleanValue("ClickToDismiss", 0);
            this.m_notifyAlways = this.GetBooleanValue("NotifyAlways", 0);
            this.m_notifyMinLevel = this.GetIntegerValue("NotifyMinLevel", 0);
            this.m_avatarIdModulo = this.GetIntegerValue("AvatarIdModulo", 0);
            this.m_moduloMin = this.GetIntegerValue("ModuloMin", 0);
            this.m_moduloMax = this.GetIntegerValue("ModuloMax", 0);
            this.m_collapsed = this.GetBooleanValue("Collapsed", 0);
            this.m_minOS = this.GetValue("MinOS", 0);
            this.m_maxOS = this.GetValue("MaxOS", 0);
            this.m_buttonTID2 = this.GetValue("ButtonTID2", 0);
            this.m_action2Type = this.GetValue("Action2Type", 0);
            this.m_action2Parameter1 = this.GetValue("Action2Parameter1", 0);
            this.m_action2Parameter2 = this.GetValue("Action2Parameter2", 0);
        }

        public int GetID()
        {
            return this.m_id;
        }

        public bool IsEnabled()
        {
            return this.m_enabled;
        }

        public bool IsEnabledIOS()
        {
            return this.m_enabledIOS;
        }

        public bool IsEnabledAndroid()
        {
            return this.m_enabledAndroid;
        }

        public bool IsEnabledKunlun()
        {
            return this.m_enabledKunlun;
        }

        public bool IsEnabledTencent()
        {
            return this.m_enabledTencent;
        }

        public bool IsEnabledLowEnd()
        {
            return this.m_enabledLowEnd;
        }

        public bool IsEnabledHighEnd()
        {
            return this.m_enabledHighEnd;
        }

        public string GetType()
        {
            return this.m_type;
        }

        public bool IsShowAsNew()
        {
            return this.m_showAsNew;
        }

        public string GetButtonTID()
        {
            return this.m_buttonTID;
        }

        public string GetActionType()
        {
            return this.m_actionType;
        }

        public string GetActionParameter1()
        {
            return this.m_actionParameter1;
        }

        public string GetActionParameter2()
        {
            return this.m_actionParameter2;
        }

        public string GetNativeAndroidURL()
        {
            return this.m_nativeAndroidURL;
        }

        public string GetIncludedCountries()
        {
            return this.m_includedCountries;
        }

        public string GetExcludedCountries()
        {
            return this.m_excludedCountries;
        }

        public string GetExcludedLoginCountries()
        {
            return this.m_excludedLoginCountries;
        }

        public bool IsCenterText()
        {
            return this.m_centerText;
        }

        public bool IsLoadResources()
        {
            return this.m_loadResources;
        }

        public bool IsLoadInLowEnd()
        {
            return this.m_loadInLowEnd;
        }

        public string GetItemSWF()
        {
            return this.m_itemSWF;
        }

        public string GetItemExportName()
        {
            return this.m_itemExportName;
        }

        public string GetIconSWF()
        {
            return this.m_iconSWF;
        }

        public int GetIconFrame()
        {
            return this.m_iconFrame;
        }

        public bool IsAnimateIcon()
        {
            return this.m_animateIcon;
        }

        public bool IsCenterIcon()
        {
            return this.m_centerIcon;
        }

        public int GetMinTownHall()
        {
            return this.m_minTownHall;
        }

        public int GetMaxTownHall()
        {
            return this.m_maxTownHall;
        }

        public int GetMinLevel()
        {
            return this.m_minLevel;
        }

        public int GetMaxLevel()
        {
            return this.m_maxLevel;
        }

        public int GetMaxDiamonds()
        {
            return this.m_maxDiamonds;
        }

        public bool IsClickToDismiss()
        {
            return this.m_clickToDismiss;
        }

        public bool IsNotifyAlways()
        {
            return this.m_notifyAlways;
        }

        public int GetNotifyMinLevel()
        {
            return this.m_notifyMinLevel;
        }

        public int GetAvatarIdModulo()
        {
            return this.m_avatarIdModulo;
        }

        public int GetModuloMin()
        {
            return this.m_moduloMin;
        }

        public int GetModuloMax()
        {
            return this.m_moduloMax;
        }

        public bool IsCollapsed()
        {
            return this.m_collapsed;
        }

        public string GetMinOS()
        {
            return this.m_minOS;
        }

        public string GetMaxOS()
        {
            return this.m_maxOS;
        }

        public string GetButtonTID2()
        {
            return this.m_buttonTID2;
        }

        public string GetAction2Type()
        {
            return this.m_action2Type;
        }

        public string GetAction2Parameter1()
        {
            return this.m_action2Parameter1;
        }

        public string GetAction2Parameter2()
        {
            return this.m_action2Parameter2;
        }
    }
}