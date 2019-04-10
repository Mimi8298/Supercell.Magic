namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;

    public class InitializeLiveReplayMessage : ServerAccountMessage
    {
        public byte[] StreamData { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteBytes(this.StreamData, this.StreamData.Length);
        }

        public override void Decode(ByteStream stream)
        {
            this.StreamData = stream.ReadBytes(stream.ReadBytesLength(), 900000);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.INITIALIZE_LIVE_REPLAY;
        }
    }
}