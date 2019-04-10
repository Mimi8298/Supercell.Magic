namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Message;

    public class KeepAliveServerMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20108;

        public KeepAliveServerMessage() : this(0)
        {
            // KeepAliveServerMessage.
        }

        public KeepAliveServerMessage(short messageVersion) : base(messageVersion)
        {
            // KeepAliveServerMessage.
        }

        public override void Decode()
        {
            base.Decode();
        }

        public override void Encode()
        {
            base.Encode();
        }

        public override short GetMessageType()
        {
            return KeepAliveServerMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}