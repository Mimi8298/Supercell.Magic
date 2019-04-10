namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public class GameJoinAllianceResponseMessage : ServerResponseMessage
    {
        public Reason ErrorReason { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (!this.Success)
            {
                stream.WriteVInt((int) this.ErrorReason);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (!this.Success)
            {
                this.ErrorReason = (Reason)stream.ReadVInt();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_JOIN_ALLIANCE_RESPONSE;
        }

        public enum Reason
        {
            NO_CASTLE,
            ALREADY_IN_ALLIANCE,
            GENERIC,
            FULL,
            CLOSED,
            SCORE,
            BANNED,
        }
    }
}