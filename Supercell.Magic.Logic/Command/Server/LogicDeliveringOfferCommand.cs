namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Offer;
    using Supercell.Magic.Titan.DataStream;

    public class LogicDeliveringOfferCommand : LogicServerCommand
    {
        private int m_offerId;
        private string m_transactionId;

        private LogicDeliverableBundle m_deliverableBundle;
        private LogicBillingPackageData m_billingPackageData;

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_offerId = stream.ReadVInt();
            this.m_transactionId = stream.ReadString(900000);

            if (this.m_deliverableBundle != null)
            {
                this.m_deliverableBundle.Destruct();
                this.m_deliverableBundle = null;
            }

            this.m_deliverableBundle = new LogicDeliverableBundle();
            this.m_deliverableBundle.Decode(stream);
            this.m_billingPackageData = (LogicBillingPackageData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.BILLING_PACKAGE);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);

            encoder.WriteVInt(this.m_offerId);
            encoder.WriteString(this.m_transactionId);

            this.m_deliverableBundle.Encode(encoder);

            ByteStreamHelper.WriteDataReference(encoder, this.m_billingPackageData);
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetGameMode().GetState() == 1)
            {
                LogicAvatar avatar = level.GetHomeOwnerAvatar();

                if (avatar != null)
                {
                    if (this.m_deliverableBundle != null)
                    {
                        if (this.m_billingPackageData != null)
                        {
                            LogicDeliveryHelper.Deliver(level, this.m_deliverableBundle);
                            LogicOffer offer = level.GetOfferManager().GetOfferById(this.m_offerId);

                            if (offer != null)
                            {
                                offer.SetState(4);
                                offer.AddPayCount(1);
                            }
                            /*else
                            {
                                Debugger.Warning(string.Format("Delivering offerUid:{0}. Offer was no longer found.", this.m_offerId));
                            }*/

                            return 0;
                        }

                        return -4;
                    }

                    return -3;
                }

                return -2;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.DELIVERING_OFFER;
        }

        public void SetDatas(int offerId, string transactionId, LogicDeliverableBundle deliverableBundle, LogicBillingPackageData billingPackageData)
        {
            this.m_offerId = offerId;
            this.m_transactionId = transactionId;
            this.m_deliverableBundle = deliverableBundle;
            this.m_billingPackageData = billingPackageData;
        }
    }
}