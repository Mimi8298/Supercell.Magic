namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Titan.DataStream;

    public class CreateAvatarStreamMessage : ServerAccountMessage
    {
        public AvatarStreamEntry Entry { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt((int) this.Entry.GetAvatarStreamEntryType());
            this.Entry.Encode(stream);
        }

        public override void Decode(ByteStream stream)
        {
            this.Entry = AvatarStreamEntryFactory.CreateStreamEntryByType((AvatarStreamEntryType) stream.ReadVInt());
            this.Entry.Decode(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CREATE_AVATAR_STREAM;
        }
    }
}