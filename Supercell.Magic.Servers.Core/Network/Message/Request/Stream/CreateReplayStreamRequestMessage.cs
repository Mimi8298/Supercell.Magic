namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Titan.DataStream;

    public class CreateReplayStreamRequestMessage : ServerRequestMessage
    {
        public string JSON { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteString(this.JSON);
        }

        public override void Decode(ByteStream stream)
        {
            this.JSON = stream.ReadString(900000);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CREATE_REPLAY_STREAM_REQUEST;
        }
    }
}