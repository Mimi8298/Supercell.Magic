namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message.Account;

    public class DisconnectedMessage : TitanDisconnectedMessage
    {
        public DisconnectedMessage() : this(0)
        {
            // DisconnectedMessage.
        }

        public DisconnectedMessage(short messageVersion) : base(messageVersion)
        {
            // DisconnectedMessage.
        }

        public override void Decode()
        {
            base.Decode();
        }

        public override void Encode()
        {
            base.Encode();
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}