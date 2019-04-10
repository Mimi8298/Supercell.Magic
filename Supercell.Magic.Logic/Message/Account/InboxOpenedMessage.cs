namespace Supercell.Magic.Logic.Message.Account
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class InboxOpenedMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 10905;
        private LogicArrayList<int> m_eventInboxIds;

        public InboxOpenedMessage() : this(0)
        {
            // UnlockAccountMessage.
        }

        public InboxOpenedMessage(short messageVersion) : base(messageVersion)
        {
            // InboxOpenedMessage.
        }

        public override void Decode()
        {
            int count = this.m_stream.ReadVInt();

            this.m_eventInboxIds = new LogicArrayList<int>(count);
            Debugger.DoAssert(count < 1000, "Too many event inbox ids");

            for (int i = count; i > 0; i--)
            {
                this.m_eventInboxIds.Add(this.m_stream.ReadVInt());
            }
        }

        public override void Encode()
        {
            this.m_stream.WriteVInt(this.m_eventInboxIds.Size());

            for (int i = 0; i < this.m_eventInboxIds.Size(); i++)
            {
                this.m_stream.WriteVInt(this.m_eventInboxIds[i]);
            }
        }

        public override short GetMessageType()
        {
            return InboxOpenedMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 1;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public LogicArrayList<int> GetEventInboxIds()
        {
            return this.m_eventInboxIds;
        }

        public void SetEventInboxIds(LogicArrayList<int> ids)
        {
            this.m_eventInboxIds = ids;
        }
    }
}