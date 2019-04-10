namespace Supercell.Magic.Servers.Core.Network.Message.Core
{
    using Supercell.Magic.Titan.DataStream;

    public class ServerPerformanceDataMessage : ServerCoreMessage
    {
        public int SessionCount { get; set; }
        public int ClusterCount { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.SessionCount);
            stream.WriteVInt(this.ClusterCount);
        }

        public override void Decode(ByteStream stream)
        {
            this.SessionCount = stream.ReadVInt();
            this.ClusterCount = stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.SERVER_PERFORMANCE_DATA;
        }
    }
}