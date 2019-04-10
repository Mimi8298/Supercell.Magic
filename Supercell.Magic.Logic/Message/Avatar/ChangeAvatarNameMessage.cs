namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Message;

    public class ChangeAvatarNameMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10212;

        private string m_avatarName;
        private bool m_nameSetByUser;

        public ChangeAvatarNameMessage() : this(0)
        {
            // ChangeAvatarNameMessage.
        }

        public ChangeAvatarNameMessage(short messageVersion) : base(messageVersion)
        {
            // ChangeAvatarNameMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_avatarName = this.m_stream.ReadString(900000);
            this.m_nameSetByUser = this.m_stream.ReadBoolean();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteString(this.m_avatarName);
            this.m_stream.WriteBoolean(this.m_nameSetByUser);
        }

        public override short GetMessageType()
        {
            return ChangeAvatarNameMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_avatarName = null;
        }

        public string RemoveAvatarName()
        {
            string tmp = this.m_avatarName;
            this.m_avatarName = null;
            return tmp;
        }

        public void SetAvatarName(string name)
        {
            this.m_avatarName = name;
        }

        public bool GetNameSetByUser()
        {
            return this.m_nameSetByUser;
        }

        public void SetNameSetByUser(bool set)
        {
            this.m_nameSetByUser = set;
        }
    }
}