namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Titan.Message;

    public class AttackHomeFailedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24103;
        
        private Reason m_reason;
        private int m_protectionEndSeconds;

        public AttackHomeFailedMessage() : this(0)
        {
            // AttackHomeFailedMessage.
        }

        public AttackHomeFailedMessage(short messageVersion) : base(messageVersion)
        {
            // AttackHomeFailedMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_reason = (Reason) this.m_stream.ReadInt();
            this.m_protectionEndSeconds = this.m_stream.ReadInt();
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt((int) this.m_reason);
            this.m_stream.WriteInt(this.m_protectionEndSeconds);
        }

        public override short GetMessageType()
        {
            return AttackHomeFailedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public Reason GetReason()
        {
            return this.m_reason;
        }

        public void SetReason(Reason value)
        {
            this.m_reason = value;
        }

        public int GetProtectionEndSeconds()
        {
            return this.m_protectionEndSeconds;
        }

        public void SetProtectionEndSeconds(int value)
        {
            this.m_protectionEndSeconds = value;
        }

        public enum Reason
        {
            GENERIC = 0,
            TARGET_ONLINE = 1,
            ALREADY_UNDER_ATTACK = 2,
            SAME_ALLIANCE = 3,
            SHIELD = 4,
            LEVEL_DIFFERENCE = 5,
            NEWBIE_PROTECTED = 6,
            NO_MATCHES = 7,
            NO_ENOUGH_RESOURCE = 8,
            COOLDOWN_AFTER_MAINTENANCE = 10,
            SHUTDOWN_ATTACK_DISABLED = 11,
            PERSONAL_BREAK_ATTACK_DISABLED = 12,
            TARGET_HAS_GUARD = 16
        }
    }
}