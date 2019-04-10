namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class RemoveAvatarStreamEntryMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 14418;
        private LogicLong m_streamId;

        public RemoveAvatarStreamEntryMessage() : this(0)
        {
            // RemoveAvatarStreamEntryMessage.
        }

        public RemoveAvatarStreamEntryMessage(short messageVersion) : base(messageVersion)
        {
            // RemoveAvatarStreamEntryMessage.
        }

        public override void Decode()
        {
            base.Decode();
            this.m_streamId = this.m_stream.ReadLong();
        }

        public override void Encode()
        {
            base.Encode();
            this.m_stream.WriteLong(this.m_streamId);
        }

        public override short GetMessageType()
        {
            return RemoveAvatarStreamEntryMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_streamId = null;
        }

        public LogicLong GetStreamEntryId()
        {
            return this.m_streamId;
        }

        public void SetStreamEntryId(LogicLong value)
        {
            this.m_streamId = value;
        }
    }
}