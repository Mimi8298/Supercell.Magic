namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Offer;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;

    public class LogicUpdateOfferStateCommand : LogicServerCommand
    {
        private int m_offerState;
        private int m_offerId;

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_offerId = stream.ReadInt();
            this.m_offerState = stream.ReadInt();
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);

            encoder.WriteInt(this.m_offerId);
            encoder.WriteInt(this.m_offerState);
        }

        public override int Execute(LogicLevel level)
        {
            LogicOffer offer = level.GetOfferManager().GetOfferById(this.m_offerId);

            if (offer != null)
            {
                offer.SetState(this.m_offerState);
                return 0;
            }

            Debugger.Warning(string.Format("Offer not found when updating offer state for id: {0} to state: {1}", this.m_offerId, this.m_offerState));
            return -2;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.UPDATE_OFFER_STATE;
        }
    }
}