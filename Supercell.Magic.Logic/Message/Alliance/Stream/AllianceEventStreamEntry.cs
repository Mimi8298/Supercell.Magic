namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AllianceEventStreamEntry : StreamEntry
    {
        private AllianceEventStreamEntryType m_eventType;
        private string m_eventAvatarName;

        private LogicLong m_eventAvatarId;

        public AllianceEventStreamEntry()
        {
            this.m_eventAvatarId = new LogicLong();
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_eventType = (AllianceEventStreamEntryType) stream.ReadInt();
            this.m_eventAvatarId = stream.ReadLong();
            this.m_eventAvatarName = stream.ReadString(900000);
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteInt((int) this.m_eventType);
            stream.WriteLong(this.m_eventAvatarId);
            stream.WriteString(this.m_eventAvatarName);
        }

        public AllianceEventStreamEntryType GetEventType()
        {
            return this.m_eventType;
        }

        public void SetEventType(AllianceEventStreamEntryType value)
        {
            this.m_eventType = value;
        }

        public LogicLong GetEventAvatarId()
        {
            return this.m_eventAvatarId;
        }

        public void SetEventAvatarId(LogicLong value)
        {
            this.m_eventAvatarId = value;
        }

        public string GetEventAvatarName()
        {
            return this.m_eventAvatarName;
        }

        public void SetEventAvatarName(string message)
        {
            this.m_eventAvatarName = message;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.ALLIANCE_EVENT;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("ChatStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_eventType = (AllianceEventStreamEntryType) jsonObject.GetJSONNumber("event_type").GetIntValue();

            LogicJSONNumber eventAvatarIdHighObject = jsonObject.GetJSONNumber("event_avatar_id_high");
            LogicJSONNumber eventAvatarIdLowObject = jsonObject.GetJSONNumber("event_avatar_id_low");

            if (eventAvatarIdHighObject != null && eventAvatarIdLowObject != null)
            {
                this.m_eventAvatarId = new LogicLong(eventAvatarIdHighObject.GetIntValue(), eventAvatarIdLowObject.GetIntValue());

                LogicJSONString eventAvatarNameObject = jsonObject.GetJSONString("event_avatar_name");

                if (eventAvatarNameObject != null)
                {
                    this.m_eventAvatarName = eventAvatarNameObject.GetStringValue();
                }
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("event_type", new LogicJSONNumber((int) this.m_eventType));

            if (!this.m_eventAvatarId.IsZero())
            {
                jsonObject.Put("event_avatar_id_high", new LogicJSONNumber(this.m_eventAvatarId.GetHigherInt()));
                jsonObject.Put("event_avatar_id_low", new LogicJSONNumber(this.m_eventAvatarId.GetLowerInt()));
                jsonObject.Put("event_avatar_name", new LogicJSONString(this.m_eventAvatarName));
            }
        }
    }

    public enum AllianceEventStreamEntryType
    {
        KICKED = 1,
        ACCEPTED,
        JOINED,
        LEFT,
        PROMOTED,
        DEMOTED,
        STARTED_WAR,
        CANCELLED_WAR,
        WAR_NO_MATCH_FOUND,
        CHANGED_SETTINGS,
        CANCELLED_ARRANGED_WAR
    }
}