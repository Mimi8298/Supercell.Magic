namespace Supercell.Magic.Logic.Offer
{
    public class LogicOfferData
    {
        protected int m_offerId;
        protected int m_linkedPackageId;
        protected int m_shopFrontPageCooldownAfterPurchaseSecs;

        public int GetId()
        {
            return this.m_offerId;
        }

        public int GetLinkedPackageId()
        {
            return this.m_linkedPackageId;
        }

        public int GetShopFrontPageCooldownAfterPurchaseSeconds()
        {
            return this.m_shopFrontPageCooldownAfterPurchaseSecs;
        }
    }
}