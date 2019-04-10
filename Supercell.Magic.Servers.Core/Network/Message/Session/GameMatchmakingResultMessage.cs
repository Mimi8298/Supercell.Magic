namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameMatchmakingResultMessage : ServerSessionMessage
    {
        public LogicLong EnemyId { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.EnemyId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.EnemyId);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (stream.ReadBoolean())
            {
                this.EnemyId = stream.ReadLong();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_MATCHMAKING_RESULT;
        }
    }
}