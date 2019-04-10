namespace Supercell.Magic.Logic.Data
{
    using Supercell.Magic.Titan.CSV;

    public class LogicBillingPackageData : LogicData
    {
        private string m_shopItemExportName;
        private string m_offerItemExportName;
        private string m_tencentID;

        private int m_diamonds;
        private int m_usd;
        private int m_order;
        private int m_rmb;
        private int m_lenovoID;

        private bool m_disabled;
        private bool m_existsApple;
        private bool m_existsAndroid;
        private bool m_red;
        private bool m_kunlunOnly;
        private bool m_isOfferPackage;

        public LogicBillingPackageData(CSVRow row, LogicDataTable table) : base(row, table)
        {
            // LogicBillingPackageData.
        }

        public override void CreateReferences()
        {
            base.CreateReferences();

            this.m_disabled = this.GetBooleanValue("Disabled", 0);
            this.m_existsApple = this.GetBooleanValue("ExistsApple", 0);
            this.m_existsAndroid = this.GetBooleanValue("ExistsAndroid", 0);
            this.m_diamonds = this.GetIntegerValue("Diamonds", 0);
            this.m_usd = this.GetIntegerValue("USD", 0);
            this.m_shopItemExportName = this.GetValue("ShopItemExportName", 0);
            this.m_offerItemExportName = this.GetValue("OfferItemExportName", 0);
            this.m_order = this.GetIntegerValue("Order", 0);
            this.m_red = this.GetBooleanValue("RED", 0);
            this.m_rmb = this.GetIntegerValue("RMB", 0);
            this.m_kunlunOnly = this.GetBooleanValue("KunlunOnly", 0);
            this.m_lenovoID = this.GetIntegerValue("LenovoID", 0);
            this.m_tencentID = this.GetValue("TencentID", 0);
            this.m_isOfferPackage = this.GetBooleanValue("isOfferPackage", 0);
        }

        public bool Disabled()
        {
            return this.m_disabled;
        }

        public bool ExistsApple()
        {
            return this.m_existsApple;
        }

        public bool ExistsAndroid()
        {
            return this.m_existsAndroid;
        }

        public int GetDiamonds()
        {
            return this.m_diamonds;
        }

        public int GetUSD()
        {
            return this.m_usd;
        }

        public string GetShopItemExportName()
        {
            return this.m_shopItemExportName;
        }

        public string GetOfferItemExportName()
        {
            return this.m_offerItemExportName;
        }

        public int GetOrder()
        {
            return this.m_order;
        }

        public bool IsRED()
        {
            return this.m_red;
        }

        public int GetRMB()
        {
            return this.m_rmb;
        }

        public bool IsKunlunOnly()
        {
            return this.m_kunlunOnly;
        }

        public int GetLenovoID()
        {
            return this.m_lenovoID;
        }

        public string GetTencentID()
        {
            return this.m_tencentID;
        }

        public bool IsOfferPackage()
        {
            return this.m_isOfferPackage;
        }
    }
}