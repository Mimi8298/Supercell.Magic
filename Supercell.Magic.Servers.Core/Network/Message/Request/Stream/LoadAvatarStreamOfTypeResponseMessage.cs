namespace Supercell.Magic.Servers.Core.Network.Message.Request.Stream
{
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public class LoadAvatarStreamOfTypeResponseMessage : ServerResponseMessage
    {
        public LogicArrayList<AvatarStreamEntry> StreamList { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                stream.WriteVInt(this.StreamList.Size());

                for (int i = 0; i < this.StreamList.Size(); i++)
                {
                    stream.WriteVInt((int) this.StreamList[i].GetAvatarStreamEntryType());
                    this.StreamList[i].Encode(stream);
                }
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.StreamList = new LogicArrayList<AvatarStreamEntry>();

                for (int i = stream.ReadVInt() - 1; i >= 0; i--)
                {
                    AvatarStreamEntry streamEntry = AvatarStreamEntryFactory.CreateStreamEntryByType((AvatarStreamEntryType) stream.ReadVInt());
                    streamEntry.Decode(stream);
                    this.StreamList.Add(streamEntry);
                }
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.LOAD_AVATAR_STREAM_OF_TYPE_RESPONSE;
        }

        public enum Reason
        {
            GENERIC,
            ALREADY_SENT
        }
    }
}