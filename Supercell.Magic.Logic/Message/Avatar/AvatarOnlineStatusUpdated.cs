namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AvatarOnlineStatusUpdated : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20206;

        private LogicLong m_avatarId;
        private int m_state;

        public AvatarOnlineStatusUpdated() : this(0)
        {
            // AvatarOnlineStatusUpdated.
        }

        public AvatarOnlineStatusUpdated(short messageVersion) : base(messageVersion)
        {
            // AvatarOnlineStatusUpdated.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_avatarId = this.m_stream.ReadLong();
            this.m_state = this.m_stream.ReadVInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_avatarId);
            this.m_stream.WriteVInt(this.m_state);
        }

        public override short GetMessageType()
        {
            return AvatarOnlineStatusUpdated.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_avatarId = null;
        }

        public LogicLong GetAvatarId()
        {
            return this.m_avatarId;
        }

        public void SetAvatarId(LogicLong value)
        {
            this.m_avatarId = value;
        }

        public int GetState()
        {
            return this.m_state;
        }

        public void SetState(int value)
        {
            this.m_state = value;
        }
    }
}