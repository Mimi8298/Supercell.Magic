namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Message;
    using Supercell.Magic.Titan.Util;

    public class AvatarStreamMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24411;

        private LogicArrayList<AvatarStreamEntry> m_entries;

        public AvatarStreamMessage() : this(0)
        {
            // AvatarStreamMessage.
        }

        public AvatarStreamMessage(short messageVersion) : base(messageVersion)
        {
            // AvatarStreamMessage.
        }

        public override void Decode()
        {
            base.Decode();

            int cnt = this.m_stream.ReadInt();

            if (cnt != -1)
            {
                this.m_entries = new LogicArrayList<AvatarStreamEntry>(cnt);

                for (int i = 0; i < cnt; i++)
                {
                    AvatarStreamEntry entry = AvatarStreamEntryFactory.CreateStreamEntryByType((AvatarStreamEntryType) this.m_stream.ReadInt());

                    if (entry == null)
                    {
                        Debugger.Warning("Corrupted AvatarStreamMessage");
                        break;
                    }

                    entry.Decode(this.m_stream);
                }
            }
            else
            {
                this.m_entries = null;
            }
        }

        public override void Encode()
        {
            base.Encode();

            if (this.m_entries != null)
            {
                this.m_stream.WriteInt(this.m_entries.Size());

                for (int i = 0; i < this.m_entries.Size(); i++)
                {
                    this.m_stream.WriteInt((int) this.m_entries[i].GetAvatarStreamEntryType());
                    this.m_entries[i].Encode(this.m_stream);
                }
            }
            else
            {
                this.m_stream.WriteInt(-1);
            }
        }

        public override short GetMessageType()
        {
            return AvatarStreamMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_entries != null)
            {
                if (this.m_entries.Size() != 0)
                {
                    do
                    {
                        this.m_entries[0].Destruct();
                        this.m_entries.Remove(0);
                    } while (this.m_entries.Size() != 0);
                }

                this.m_entries = null;
            }
        }

        public LogicArrayList<AvatarStreamEntry> RemoveStreamEntries()
        {
            LogicArrayList<AvatarStreamEntry> tmp = this.m_entries;
            this.m_entries = null;
            return tmp;
        }

        public void SetStreamEntries(LogicArrayList<AvatarStreamEntry> entry)
        {
            this.m_entries = entry;
        }
    }
}