namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameFriendlyScoutMessage : ServerSessionMessage
    {
        public LogicLong AccountId { get; set; }
        public LogicLong StreamId { get; set; }

        public byte[] HomeJSON { get; set; }
        public int MapId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AccountId);
            stream.WriteLong(this.StreamId);
            stream.WriteBytes(this.HomeJSON, this.HomeJSON.Length);
            stream.WriteVInt(this.MapId);
        }

        public override void Decode(ByteStream stream)
        {
            this.AccountId = stream.ReadLong();
            this.StreamId = stream.ReadLong();
            this.HomeJSON = stream.ReadBytes(stream.ReadBytesLength(), 900000);
            this.MapId = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_FRIENDLY_SCOUT;
        }
    }
}