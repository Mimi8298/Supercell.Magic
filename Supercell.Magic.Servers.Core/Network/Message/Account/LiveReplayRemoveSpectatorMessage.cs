namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;

    public class LiveReplayRemoveSpectatorMessage : ServerAccountMessage
    {
        public long SessionId { get; set; }
        public int SlotId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLongLong(this.SessionId);
            stream.WriteVInt(this.SlotId);
        }

        public override void Decode(ByteStream stream)
        {
            this.SessionId = stream.ReadLongLong();
            this.SlotId = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LIVE_REPLAY_REMOVE_SPECTATOR;
        }
    }
}