namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;

    public class GameStateDataMessage : ServerSessionMessage
    {
        public GameState State { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt((int) this.State.GetGameStateType());
            this.State.Encode(stream);
        }

        public override void Decode(ByteStream stream)
        {
            this.State = GameStateFactory.CreateByType((GameStateType) stream.ReadVInt());
            this.State.Decode(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_STATE_DATA;
        }
    }
}