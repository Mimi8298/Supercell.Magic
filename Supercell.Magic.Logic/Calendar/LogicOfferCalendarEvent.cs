namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Titan.Json;

    public class LogicOfferCalendarEvent : LogicCalendarEvent
    {
        private string m_tid;
        private string m_infoTid;
        private string m_infoSwf;
        private string m_infoExportName;
        private string m_shopItemExportName;
        private string m_shopInfoItemExportName;
        private string m_shopItemBGFrame;
        private string m_valueTid;

        private string m_billingPackage;
        private string m_replaceBillingPackage;

        private int m_amountCanBePurchased;
        private int m_priority;
        private int m_value;

        private bool m_availableTreasurePage;
        private bool m_availableFrontPage;

        public override void Load(LogicJSONObject jsonObject)
        {
            base.Load(jsonObject);
        }

        public override LogicJSONObject Save()
        {
            return base.Save();
        }

        public override int GetCalendarEventType()
        {
            return LogicCalendarEvent.EVENT_TYPE_OFFER;
        }
    }
}