namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class CreateAvatarStreamRequestMessage : ServerRequestMessage
    {
        public LogicLong OwnerId { get; set; }
        public AvatarStreamEntry Entry { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.OwnerId);
            stream.WriteVInt((int) this.Entry.GetAvatarStreamEntryType());
            this.Entry.Encode(stream);
        }

        public override void Decode(ByteStream stream)
        {
            this.OwnerId = stream.ReadLong();
            this.Entry = AvatarStreamEntryFactory.CreateStreamEntryByType((AvatarStreamEntryType) stream.ReadVInt());
            this.Entry.Decode(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CREATE_AVATAR_STREAM_REQUEST;
        }
    }
}