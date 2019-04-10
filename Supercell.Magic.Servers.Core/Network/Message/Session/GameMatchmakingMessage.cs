namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public class GameMatchmakingMessage : ServerSessionMessage
    {
        public GameMatchmakingType MatchmakingType { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt((int) this.MatchmakingType);
        }

        public override void Decode(ByteStream stream)
        {
            this.MatchmakingType = (GameMatchmakingType) stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_MATCHMAKING;
        }

        public enum GameMatchmakingType
        {
            DEFAULT,
            DUEL
        }
    }
}