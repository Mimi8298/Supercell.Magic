namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class CreateReplayStreamResponseMessage : ServerResponseMessage
    {
        public LogicLong Id { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                stream.WriteLong(this.Id);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.Id = stream.ReadLong();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CREATE_REPLAY_STREAM_RESPONSE;
        }
    }
}