namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AskForAvatarProfileMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14325;

        private LogicLong m_avatarId;
        private LogicLong m_homeId;

        public AskForAvatarProfileMessage() : this(0)
        {
            // AskForAvatarProfileMessage.
        }

        public AskForAvatarProfileMessage(short messageVersion) : base(messageVersion)
        {
            // AskForAvatarProfileMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_avatarId = this.m_stream.ReadLong();

            if (this.m_stream.ReadBoolean())
            {
                this.m_homeId = this.m_stream.ReadLong();
            }

            this.m_stream.ReadBoolean();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_avatarId);

            if (this.m_homeId != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_homeId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }

            this.m_stream.WriteBoolean(false);
        }

        public override short GetMessageType()
        {
            return AskForAvatarProfileMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_avatarId = null;
            this.m_homeId = null;
        }

        public LogicLong RemoveAvatarId()
        {
            LogicLong tmp = this.m_avatarId;
            this.m_avatarId = null;
            return tmp;
        }

        public void SetAvatarId(LogicLong id)
        {
            this.m_avatarId = id;
        }

        public LogicLong RemoveHomeId()
        {
            LogicLong tmp = this.m_homeId;
            this.m_homeId = null;
            return tmp;
        }

        public void SetHomeId(LogicLong id)
        {
            this.m_homeId = id;
        }
    }
}