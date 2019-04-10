namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LoadAvatarStreamOfTypeRequestMessage : ServerRequestMessage
    {
        public LogicArrayList<LogicLong> StreamIds { get; set; }
        public LogicLong SenderAvatarId { get; set; }
        public AvatarStreamEntryType Type { get; set; }


        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.StreamIds.Size());

            for (int i = 0; i < this.StreamIds.Size(); i++)
            {
                stream.WriteLong(this.StreamIds[i]);
            }

            if (this.SenderAvatarId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.SenderAvatarId);
            }

            stream.WriteVInt((int) this.Type);
        }

        public override void Decode(ByteStream stream)
        {
            this.StreamIds = new LogicArrayList<LogicLong>();

            for (int i = stream.ReadVInt(); i > 0; i--)
            {
                this.StreamIds.Add(stream.ReadLong());
            }

            if (stream.ReadBoolean())
            {
                this.SenderAvatarId = stream.ReadLong();
            }

            this.Type = (AvatarStreamEntryType) stream.ReadVInt();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LOAD_AVATAR_STREAM_OF_TYPE_REQUEST;
        }
    }
}