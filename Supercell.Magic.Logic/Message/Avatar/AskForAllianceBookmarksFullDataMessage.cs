namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Message;

    public class AskForAllianceBookmarksFullDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14341;

        public AskForAllianceBookmarksFullDataMessage() : this(0)
        {
            // AskForAllianceBookmarksFullDataMessage.
        }

        public AskForAllianceBookmarksFullDataMessage(short messageVersion) : base(messageVersion)
        {
            // AskForAllianceBookmarksFullDataMessage.
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
            return AskForAllianceBookmarksFullDataMessage.MESSAGE_TYPE;
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