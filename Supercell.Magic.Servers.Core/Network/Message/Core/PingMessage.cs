namespace Supercell.Magic.Servers.Core.Network.Message.Core
{
    using Supercell.Magic.Titan.DataStream;

    public class PingMessage : ServerCoreMessage
    {
        public override void Encode(ByteStream stream)
        {
        }

        public override void Decode(ByteStream stream)
        {
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.PING;
        }
    }
}