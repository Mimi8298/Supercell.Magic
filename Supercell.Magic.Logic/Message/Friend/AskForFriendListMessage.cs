namespace Supercell.Magic.Logic.Message.Friend
{
    using Supercell.Magic.Titan.Message;

    public class AskForFriendListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10504;

        public AskForFriendListMessage() : this(0)
        {
            // AskForFriendListMessage.
        }

        public AskForFriendListMessage(short messageVersion) : base(messageVersion)
        {
            // AskForFriendListMessage.
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
            return AskForFriendListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 3;
        }

        public override void Destruct()
        {
            base.Destruct();
        }
    }
}