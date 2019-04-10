namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Titan.DataStream;

    public class LoadAvatarStreamResponseMessage : ServerResponseMessage
    {
        public AvatarStreamEntry Entry { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                stream.WriteVInt((int) this.Entry.GetAvatarStreamEntryType());
                this.Entry.Encode(stream);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.Entry = AvatarStreamEntryFactory.CreateStreamEntryByType((AvatarStreamEntryType)stream.ReadVInt());
                this.Entry.Decode(stream);
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LOAD_AVATAR_STREAM_RESPONSE;
        }
    }
}