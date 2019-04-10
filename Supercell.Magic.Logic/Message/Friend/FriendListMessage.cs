namespace Supercell.Magic.Logic.Message.Friend
{
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class FriendListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20105;

        private int m_listType;
        private LogicArrayList<FriendEntry> m_friendEntryList;

        public FriendListMessage() : this(0)
        {
            // FriendListMessage.
        }

        public FriendListMessage(short messageVersion) : base(messageVersion)
        {
            // FriendListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_listType = this.m_stream.ReadInt();

            int count = this.m_stream.ReadInt();

            if (count != -1)
            {
                this.m_friendEntryList = new LogicArrayList<FriendEntry>(count);

                for (int i = 0; i < count; i++)
                {
                    FriendEntry friendEntry = new FriendEntry();
                    friendEntry.Decode(this.m_stream);
                    this.m_friendEntryList.Add(friendEntry);
                }
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_listType);

            if (this.m_friendEntryList != null)
            {
                this.m_stream.WriteInt(this.m_friendEntryList.Size());

                for (int i = 0; i < this.m_friendEntryList.Size(); i++)
                {
                    this.m_friendEntryList[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return FriendListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 3;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_friendEntryList = null;
        }

        public LogicArrayList<FriendEntry> RemoveFriendEntries()
        {
            LogicArrayList<FriendEntry> tmp = this.m_friendEntryList;
            this.m_friendEntryList = null;
            return tmp;
        }

        public void SetFriendEntries(LogicArrayList<FriendEntry> list)
        {
            this.m_friendEntryList = list;
        }

        public int GetListType()
        {
            return this.m_listType;
        }

        public void SetListType(int value)
        {
            this.m_listType = value;
        }
    }
}