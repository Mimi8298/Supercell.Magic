namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LoadReplayStreamRequestMessage : ServerRequestMessage
    {
        public LogicLong Id { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.Id);
        }

        public override void Decode(ByteStream stream)
        {
            this.Id = stream.ReadLong();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LOAD_REPLAY_STREAM_REQUEST;
        }
    }
}