namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LiveReplayAddSpectatorRequestMessage : ServerRequestMessage
    {
        public LogicLong LiveReplayId { get; set; }
        public long SessionId { get; set; }
        public int SlotId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.LiveReplayId);
            stream.WriteLongLong(this.SessionId);
            stream.WriteVInt(this.SlotId);
        }

        public override void Decode(ByteStream stream)
        {
            this.LiveReplayId = stream.ReadLong();
            this.SessionId = stream.ReadLongLong();
            this.SlotId = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LIVE_REPLAY_ADD_SPECTATOR_REQUEST;
        }
    }
}