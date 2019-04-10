namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;

    public class ServerUpdateLiveReplayMessage : ServerAccountMessage
    {
        public int Milliseconds { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Milliseconds);
        }

        public override void Decode(ByteStream stream)
        {
            this.Milliseconds = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.SERVER_UPDATE_LIVE_REPLAY;
        }
    }
}