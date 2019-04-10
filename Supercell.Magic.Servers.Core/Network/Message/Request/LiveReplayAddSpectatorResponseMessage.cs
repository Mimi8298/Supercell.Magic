namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public class LiveReplayAddSpectatorResponseMessage : ServerResponseMessage
    {
        public Reason ErrorCode { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (!this.Success)
            {
                stream.WriteVInt((int) this.ErrorCode);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (!this.Success)
            {
                this.ErrorCode = (Reason) stream.ReadVInt();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LIVE_REPLAY_ADD_SPECTATOR_RESPONSE;
        }

        public enum Reason
        {
            NOT_EXISTS,
            FULL
        }
    }
}