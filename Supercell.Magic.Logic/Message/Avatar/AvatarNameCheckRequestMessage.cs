namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Message;

    public class AvatarNameCheckRequestMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14600;

        private string m_name;

        public AvatarNameCheckRequestMessage() : this(0)
        {
            // AvatarNameCheckRequestMessage.
        }

        public AvatarNameCheckRequestMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarNameCheckRequestMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_name = this.m_stream.ReadString(900000);
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteString(this.m_name);
        }

        public override short GetMessageType()
        {
            return AvatarNameCheckRequestMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_name = null;
        }

        public string GetName()
        {
            return this.m_name;
        }

        public void SetName(string name)
        {
            this.m_name = name;
        }
    }
}