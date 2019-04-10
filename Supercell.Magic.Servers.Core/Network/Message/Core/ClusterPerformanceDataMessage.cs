namespace Supercell.Magic.Servers.Core.Network.Message.Core
{
    using Supercell.Magic.Titan.DataStream;

    public class ClusterPerformanceDataMessage : ServerCoreMessage
    {
        public int Id { get; set; }
        public int SessionCount { get; set; }
        public int Ping { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Id);
            stream.WriteVInt(this.SessionCount);
            stream.WriteVInt(this.Ping);
        }

        public override void Decode(ByteStream stream)
        {
            this.Id = stream.ReadVInt();
            this.SessionCount = stream.ReadVInt();
            this.Ping = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CLUSTER_PERFORMANCE_DATA;
        }
    }
}