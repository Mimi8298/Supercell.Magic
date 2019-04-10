namespace Supercell.Magic.Logic.Message.Friend
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class FriendAvatarBaseMessage : PiranhaMessage
    {
        private int m_avatarIdHigh;
        private int m_avatarIdLow;

        public FriendAvatarBaseMessage() : this(0)
        {
            // FriendAvatarBaseMessage.
        }

        public FriendAvatarBaseMessage(short messageVersion) : base(messageVersion)
        {
            // FriendAvatarBaseMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_avatarIdHigh = this.m_stream.ReadInt();
            this.m_avatarIdLow = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_avatarIdHigh);
            this.m_stream.WriteInt(this.m_avatarIdLow);
        }

        public override int GetServiceNodeType()
        {
            return 3;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong GetAvatarId()
        {
            return new LogicLong(this.m_avatarIdHigh, this.m_avatarIdLow);
        }

        public void SetAvatarId(LogicLong avatarId)
        {
            this.m_avatarIdHigh = avatarId.GetHigherInt();
            this.m_avatarIdLow = avatarId.GetLowerInt();
        }

        public void SetAvatarId(int high, int low)
        {
            this.m_avatarIdHigh = high;
            this.m_avatarIdLow = low;
        }
    }
}