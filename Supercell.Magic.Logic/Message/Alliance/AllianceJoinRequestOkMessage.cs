namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceJoinRequestOkMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24319;

        public AllianceJoinRequestOkMessage() : this(0)
        {
            // AllianceJoinRequestOkMessage.
        }

        public AllianceJoinRequestOkMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceJoinRequestOkMessage.
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
            return AllianceJoinRequestOkMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}