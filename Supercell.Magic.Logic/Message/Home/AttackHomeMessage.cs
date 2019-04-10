namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class AttackHomeMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14106;

        private LogicLong m_homeId;
        private LogicLong m_avatarStreamId;

        private int m_attackSource;

        public AttackHomeMessage() : this(0)
        {
            // AttackHomeMessage.
        }

        public AttackHomeMessage(short messageVersion) : base(messageVersion)
        {
            // AttackHomeMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_homeId = this.m_stream.ReadLong();
            this.m_attackSource = this.m_stream.ReadInt();
            this.m_avatarStreamId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteLong(this.m_homeId);
            this.m_stream.WriteInt(this.m_attackSource);
            this.m_stream.WriteLong(this.m_avatarStreamId);
        }

        public override short GetMessageType()
        {
            return AttackHomeMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicLong GetHomeId()
        {
            return this.m_homeId;
        }

        public void SetHomeId(LogicLong value)
        {
            this.m_homeId = value;
        }

        public LogicLong GetAvatarStreamId()
        {
            return this.m_avatarStreamId;
        }

        public void SetAvatarStreamId(LogicLong value)
        {
            this.m_avatarStreamId = value;
        }

        public int GetAttackSource()
        {
            return this.m_attackSource;
        }

        public void SetAttackSource(int value)
        {
            this.m_attackSource = value;
        }
    }
}