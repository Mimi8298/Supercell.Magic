namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class SendAllianceBookmarksFullDataToClientMessage : ServerSessionMessage
    {
        public LogicArrayList<LogicLong> AllianceIds { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.AllianceIds.Size());

            for (int i = 0; i < this.AllianceIds.Size(); i++)
            {
                stream.WriteLong(this.AllianceIds[i]);
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.AllianceIds = new LogicArrayList<LogicLong>();

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.AllianceIds.Add(stream.ReadLong());
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.SEND_ALLIANCE_BOOKMARKS_FULL_DATA_TO_CLIENT;
        }
    }
}