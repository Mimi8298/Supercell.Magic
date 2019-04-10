namespace Supercell.Magic.Servers.Core.Network.Message.Session
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class SendAvatarStreamsToClientMessage : ServerSessionMessage
    {
        public LogicArrayList<LogicLong> StreamIds { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.StreamIds.Size());

            for (int i = 0; i < this.StreamIds.Size(); i++)
            {
                stream.WriteLong(this.StreamIds[i]);
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.StreamIds = new LogicArrayList<LogicLong>();

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.StreamIds.Add(stream.ReadLong());
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.SEND_AVATAR_STREAMS_TO_CLIENT;
        }
    }
}