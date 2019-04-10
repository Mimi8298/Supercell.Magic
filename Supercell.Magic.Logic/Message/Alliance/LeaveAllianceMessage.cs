namespace Supercell.Magic.Logic.Message.Alliance
{
    using Supercell.Magic.Titan.Message;

    public class LeaveAllianceMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14308;

        public LeaveAllianceMessage() : this(0)
        {
            // LeaveAllianceMessage.
        }

        public LeaveAllianceMessage(short messageVersion) : base(messageVersion)
        {
            // LeaveAllianceMessage.
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
            return LeaveAllianceMessage.MESSAGE_TYPE;
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