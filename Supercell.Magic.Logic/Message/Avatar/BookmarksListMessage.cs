namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class BookmarksListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24340;
        private LogicArrayList<LogicLong> m_allianceIds;

        public BookmarksListMessage() : this(0)
        {
            // BookmarksListMessage.
        }

        public BookmarksListMessage(short messageVersion) : base(messageVersion)
        {
            // BookmarksListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int count = this.m_stream.ReadInt();

            if (count >= 0)
            {
                Debugger.DoAssert(count < 1000, "Too many alliance ids in BookmarksListMessage");
                this.m_allianceIds = new LogicArrayList<LogicLong>(count);

                for (int i = 0; i < count; i++)
                {
                    this.m_allianceIds.Add(this.m_stream.ReadLong());
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_allianceIds != null)
            {
                this.m_stream.WriteInt(this.m_allianceIds.Size());

                for (int i = 0; i < this.m_allianceIds.Size(); i++)
                {
                    this.m_stream.WriteLong(this.m_allianceIds[i]);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return BookmarksListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_allianceIds = null;
        }

        public LogicArrayList<LogicLong> GetAllianceIds()
        {
            return this.m_allianceIds;
        }

        public void SetAllianceIds(LogicArrayList<LogicLong> list)
        {
            this.m_allianceIds = list;
        }
    }
}