namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public class RequestAllianceJoinResponseMessage : ServerResponseMessage
    {
        public Reason ErrorReason { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (!this.Success)
            {
                stream.WriteVInt((int)this.ErrorReason);
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
            return ServerMessageType.REQUEST_ALLIANCE_JOIN_RESPONSE;
        }

        public enum Reason
        {
            GENERIC,
            CLOSED,
            ALREADY_SENT,
            NO_SCORE,
            BANNED,
            TOO_MANY_PENDING_REQUESTS,
            NO_DUEL_SCORE,
        }
    }
}