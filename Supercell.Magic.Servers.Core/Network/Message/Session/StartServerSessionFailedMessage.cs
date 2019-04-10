namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public class StartServerSessionFailedMessage : ServerSessionMessage
    {
        public new long SessionId
        {
            get { return base.SessionId; }
            set { base.SessionId = value; }
        }

        public override void Encode(ByteStream stream)
        {
        }

        public override void Decode(ByteStream stream)
        {
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.START_SERVER_SESSION_FAILED;
        }
    }
}