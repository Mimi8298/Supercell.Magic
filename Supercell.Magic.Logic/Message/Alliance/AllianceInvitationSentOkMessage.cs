namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AllianceInvitationSentOkMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24322;

        public AllianceInvitationSentOkMessage() : this(0)
        {
            // AllianceInvitationSentOkMessage.
        }

        public AllianceInvitationSentOkMessage(short messageVersion) : base(messageVersion)
        {
            // AllianceInvitationSentOkMessage.
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
            return AllianceInvitationSentOkMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}