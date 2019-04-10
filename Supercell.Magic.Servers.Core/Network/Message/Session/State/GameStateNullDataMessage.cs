namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public class GameStateNullDataMessage : ServerSessionMessage
    {
        public override void Encode(ByteStream stream)
        {
        }

        public override void Decode(ByteStream stream)
        {
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_STATE_NULL_DATA;
        }
    }
}