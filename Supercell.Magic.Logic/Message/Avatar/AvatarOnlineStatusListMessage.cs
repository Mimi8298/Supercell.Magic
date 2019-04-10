namespace Supercell.Magic.Logic.Message.Avatar
{
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AvatarOnlineStatusListMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 20208;

        private LogicArrayList<int> m_avatarStatusList;
        private LogicArrayList<LogicLong> m_avatarIdList;

        private int m_listType;

        public AvatarOnlineStatusListMessage() : this(0)
        {
            // AvatarOnlineStatusListMessage.
        }

        public AvatarOnlineStatusListMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarOnlineStatusListMessage.
        }

        public override void Decode()
        {
            base.Decode();

            for (int i = this.m_stream.ReadVInt(); i > 0; i--)
            {
                this.m_avatarStatusList.Add(this.m_stream.ReadVInt());
                this.m_avatarIdList.Add(this.m_stream.ReadLong());
            }

            if (!this.m_stream.IsAtEnd())
            {
                this.m_listType = this.m_stream.ReadVInt();
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteVInt(this.m_avatarIdList.Size());

            for (int i = 0; i < this.m_avatarIdList.Size(); i++)
            {
                this.m_stream.WriteVInt(this.m_avatarStatusList[i]);
                this.m_stream.WriteLong(this.m_avatarIdList[i]);
            }

            this.m_stream.WriteVInt(this.m_listType);
        }

        public override short GetMessageType()
        {
            return AvatarOnlineStatusListMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_avatarIdList = null;
        }

        public LogicArrayList<int> GetAvatarStatus()
        {
            return this.m_avatarStatusList;
        }

        public void SetAvatarStatusList(LogicArrayList<int> value)
        {
            this.m_avatarStatusList = value;
        }

        public LogicArrayList<LogicLong> GetAvatarIdList()
        {
            return this.m_avatarIdList;
        }

        public void SetAvatarIdList(LogicArrayList<LogicLong> value)
        {
            this.m_avatarIdList = value;
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