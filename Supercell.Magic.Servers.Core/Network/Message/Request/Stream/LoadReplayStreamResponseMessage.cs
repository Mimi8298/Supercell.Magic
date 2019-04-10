namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Titan.DataStream;

    public class LoadReplayStreamResponseMessage : ServerResponseMessage
    {
        public byte[] StreamData { get; set; }

        public int MajorVersion { get; set; }
        public int BuildVersion { get; set; }
        public int ContentVersion { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                stream.WriteBytes(this.StreamData, this.StreamData.Length);
                stream.WriteVInt(this.MajorVersion);
                stream.WriteVInt(this.BuildVersion);
                stream.WriteVInt(this.ContentVersion);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.StreamData = stream.ReadBytes(stream.ReadBytesLength(), 900000);
                this.MajorVersion = stream.ReadVInt();
                this.BuildVersion = stream.ReadVInt();
                this.ContentVersion = stream.ReadVInt();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LOAD_REPLAY_STREAM_RESPONSE;
        }
    }
}