namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Message;

    public class AvatarProfileMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24334;

        private AvatarProfileFullEntry m_entry;

        public AvatarProfileMessage() : this(0)
        {
            // AvatarProfileMessage.
        }

        public AvatarProfileMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarProfileMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_entry = new AvatarProfileFullEntry();
            this.m_entry.Decode(this.m_stream);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_entry.Encode(this.m_stream);
        }

        public override short GetMessageType()
        {
            return AvatarProfileMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_entry != null)
            {
                this.m_entry.Destruct();
                this.m_entry = null;
            }
        }

        public AvatarProfileFullEntry RemoveAvatarProfileFullEntry()
        {
            AvatarProfileFullEntry tmp = this.m_entry;
            this.m_entry = null;
            return tmp;
        }

        public void SetAvatarProfileFullEntry(AvatarProfileFullEntry entry)
        {
            this.m_entry = entry;
        }
    }
}