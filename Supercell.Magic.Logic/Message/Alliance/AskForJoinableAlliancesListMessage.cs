namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class AskForJoinableAlliancesListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14303;

        public AskForJoinableAlliancesListMessage() : this(0)
        {
            // AskForJoinableAlliancesListMessage.
        }

        public AskForJoinableAlliancesListMessage(short messageVersion) : base(messageVersion)
        {
            // AskForJoinableAlliancesListMessage.
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
            return AskForJoinableAlliancesListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 29;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}