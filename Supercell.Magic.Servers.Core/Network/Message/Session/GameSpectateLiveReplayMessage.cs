namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameSpectateLiveReplayMessage : ServerSessionMessage
    {
        public LogicLong LiveReplayId { get; set; }
        public bool IsEnemy { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.LiveReplayId);
            stream.WriteBoolean(this.IsEnemy);
        }

        public override void Decode(ByteStream stream)
        {
            this.LiveReplayId = stream.ReadLong();
            this.IsEnemy = stream.ReadBoolean();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_SPECTATE_LIVE_REPLAY;
        }
    }
}