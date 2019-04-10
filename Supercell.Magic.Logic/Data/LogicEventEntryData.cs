namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicEventEntryData : LogicData
    {
        private string m_itemSWF;
        private string m_itemExportName;
        private string m_upcomingItemExportName;
        private string m_titleTID;
        private string m_upcomingTitleTID;
        private string m_buttonTID;
        private string m_buttonAction;
        private string m_buttonActionData;
        private string m_button2TID;
        private string m_button2Action;
        private string m_button2ActionData;
        private string m_buttonLanguage;

        private bool m_loadSWF;

        public LogicEventEntryData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicEventEntryData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_itemSWF = this.GetValue("ItemSWF", 0);
            this.m_itemExportName = this.GetValue("ItemExportName", 0);
            this.m_upcomingItemExportName = this.GetValue("UpcomingItemExportName", 0);
            this.m_loadSWF = this.GetBooleanValue("LoadSWF", 0);
            this.m_titleTID = this.GetValue("TitleTID", 0);
            this.m_upcomingTitleTID = this.GetValue("UpcomingTitleTID", 0);
            this.m_buttonTID = this.GetValue("ButtonTID", 0);
            this.m_buttonAction = this.GetValue("ButtonAction", 0);
            this.m_buttonActionData = this.GetValue("ButtonActionData", 0);
            this.m_button2TID = this.GetValue("Button2TID", 0);
            this.m_button2Action = this.GetValue("Button2Action", 0);
            this.m_button2ActionData = this.GetValue("Button2ActionData", 0);
            this.m_buttonLanguage = this.GetValue("ButtonLanguage", 0);
        }

        public string GetItemSWF()
        {
            return this.m_itemSWF;
        }

        public string GetItemExportName()
        {
            return this.m_itemExportName;
        }

        public string GetUpcomingItemExportName()
        {
            return this.m_upcomingItemExportName;
        }

        public bool IsLoadSWF()
        {
            return this.m_loadSWF;
        }

        public string GetTitleTID()
        {
            return this.m_titleTID;
        }

        public string GetUpcomingTitleTID()
        {
            return this.m_upcomingTitleTID;
        }

        public string GetButtonTID()
        {
            return this.m_buttonTID;
        }

        public string GetButtonAction()
        {
            return this.m_buttonAction;
        }

        public string GetButtonActionData()
        {
            return this.m_buttonActionData;
        }

        public string GetButton2TID()
        {
            return this.m_button2TID;
        }

        public string GetButton2Action()
        {
            return this.m_button2Action;
        }

        public string GetButton2ActionData()
        {
            return this.m_button2ActionData;
        }

        public string GetButtonLanguage()
        {
            return this.m_buttonLanguage;
        }
    }
}