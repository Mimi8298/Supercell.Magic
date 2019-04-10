namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Data;

    public class LogicBundleOfferData : LogicOfferData
    {
        public LogicBundleOfferData(LogicGemBundleData data)
        {
            this.m_offerId = data.GetGlobalID();
            this.m_linkedPackageId = data.GetLinkedPackageId();
            this.m_shopFrontPageCooldownAfterPurchaseSecs = data.GetShopFrontPageCooldownAfterPurchaseSeconds();
        }
    }
}