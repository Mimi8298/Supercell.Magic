namespace Supercell.Magic.Logic.Message.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Home;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Message;

    public class EnemyHomeDataMessage : PiranhaMessage
    {
        public const int MESSAGE_TYPE = 24107;

        private int m_secondsSinceLastMaintenance;
        private int m_currentTimestamp;
        private int m_secondsSinceLastSave;
        private int m_attackSource;
        private int m_mapId;

        private LogicClientAvatar m_attackerLogicClientAvatar;
        private LogicClientAvatar m_logicClientAvatar;
        private LogicClientHome m_logicClientHome;
        private LogicLong m_AvatarStreamEntryId;

        public EnemyHomeDataMessage() : this(0)
        {
            // EnemyHomeDataMessage.
        }

        public EnemyHomeDataMessage(short messageVersion) : base(messageVersion)
        {
            // EnemyHomeDataMessage.
        }

        public override void Decode()
        {
            base.Decode();

            this.m_secondsSinceLastSave = this.m_stream.ReadInt();
            this.m_secondsSinceLastMaintenance = this.m_stream.ReadInt();
            this.m_currentTimestamp = this.m_stream.ReadInt();

            this.m_logicClientHome = new LogicClientHome();
            this.m_logicClientHome.Decode(this.m_stream);
            this.m_logicClientAvatar = new LogicClientAvatar();
            this.m_logicClientAvatar.Decode(this.m_stream);
            this.m_attackerLogicClientAvatar = new LogicClientAvatar();
            this.m_attackerLogicClientAvatar.Decode(this.m_stream);

            this.m_attackSource = this.m_stream.ReadInt();
            this.m_mapId = this.m_stream.ReadInt();

            if (this.m_stream.ReadBoolean())
            {
                this.m_AvatarStreamEntryId = this.m_stream.ReadLong();
            }
        }

        public override void Encode()
        {
            base.Encode();

            this.m_stream.WriteInt(this.m_secondsSinceLastSave);
            this.m_stream.WriteInt(this.m_secondsSinceLastMaintenance);
            this.m_stream.WriteInt(this.m_currentTimestamp);

            this.m_logicClientHome.Encode(this.m_stream);
            this.m_logicClientAvatar.Encode(this.m_stream);
            this.m_attackerLogicClientAvatar.Encode(this.m_stream);
            this.m_stream.WriteInt(this.m_attackSource);
            this.m_stream.WriteInt(this.m_mapId);

            if (this.m_AvatarStreamEntryId != null)
            {
                this.m_stream.WriteBoolean(true);
                this.m_stream.WriteLong(this.m_AvatarStreamEntryId);
            }
            else
            {
                this.m_stream.WriteBoolean(false);
            }
        }

        public override short GetMessageType()
        {
            return EnemyHomeDataMessage.MESSAGE_TYPE;
        }

        public override int GetServiceNodeType()
        {
            return 10;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_logicClientHome = null;
            this.m_logicClientAvatar = null;
            this.m_attackerLogicClientAvatar = null;
            this.m_AvatarStreamEntryId = null;
        }

        public int GetCurrentTimestamp()
        {
            return this.m_currentTimestamp;
        }

        public void SetCurrentTimestamp(int value)
        {
            this.m_currentTimestamp = value;
        }

        public int GetSecondsSinceLastSave()
        {
            return this.m_secondsSinceLastSave;
        }

        public void SetSecondsSinceLastSave(int value)
        {
            this.m_secondsSinceLastSave = value;
        }

        public int GetSecondsSinceLastMaintenance()
        {
            return this.m_secondsSinceLastMaintenance;
        }

        public void SetAttackSource(int value)
        {
            this.m_attackSource = value;
        }

        public int GetAttackSource()
        {
            return this.m_attackSource;
        }

        public void SetMapId(int value)
        {
            this.m_mapId = value;
        }

        public int GetMapId()
        {
            return this.m_mapId;
        }

        public void SetSecondsSinceLastMaintenance(int value)
        {
            this.m_secondsSinceLastMaintenance = value;
        }

        public LogicClientHome RemoveLogicClientHome()
        {
            LogicClientHome tmp = this.m_logicClientHome;
            this.m_logicClientHome = null;
            return tmp;
        }

        public void SetLogicClientHome(LogicClientHome logicClientHome)
        {
            this.m_logicClientHome = logicClientHome;
        }

        public LogicClientAvatar RemoveLogicClientAvatar()
        {
            LogicClientAvatar tmp = this.m_logicClientAvatar;
            this.m_logicClientAvatar = null;
            return tmp;
        }

        public void SetLogicClientAvatar(LogicClientAvatar logicClientAvatar)
        {
            this.m_logicClientAvatar = logicClientAvatar;
        }

        public LogicClientAvatar RemoveAttackerLogicClientAvatar()
        {
            LogicClientAvatar tmp = this.m_attackerLogicClientAvatar;
            this.m_attackerLogicClientAvatar = null;
            return tmp;
        }

        public void SetAttackerLogicClientAvatar(LogicClientAvatar logicClientAvatar)
        {
            this.m_attackerLogicClientAvatar = logicClientAvatar;
        }

        public LogicLong GetAvatarStreamEntryId()
        {
            return this.m_AvatarStreamEntryId;
        }

        public void SetAvatarStreamEntryId(LogicLong id)
        {
            this.m_AvatarStreamEntryId = id;
        }
    }
}