namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Titan.Message;

    public class AvatarStreamEntryMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24412;

        private AvatarStreamEntry m_avatarStreamEntry;

        public AvatarStreamEntryMessage() : this(0)
        {
            // AvatarStreamEntryMessage.
        }

        public AvatarStreamEntryMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarStreamEntryMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_avatarStreamEntry = AvatarStreamEntryFactory.CreateStreamEntryByType((AvatarStreamEntryType) this.m_stream.ReadInt());
            this.m_avatarStreamEntry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt((int) this.m_avatarStreamEntry.GetAvatarStreamEntryType());
            this.m_avatarStreamEntry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return AvatarStreamEntryMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_avatarStreamEntry = null;
        }

        public AvatarStreamEntry RemoveAvatarStreamEntry()
        {
            AvatarStreamEntry tmp = this.m_avatarStreamEntry;
            this.m_avatarStreamEntry = null;
            return tmp;
        }

        public void SetAvatarStreamEntry(AvatarStreamEntry entry)
        {
            this.m_avatarStreamEntry = entry;
        }
    }
}