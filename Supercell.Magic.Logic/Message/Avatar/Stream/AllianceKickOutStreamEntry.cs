namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AllianceKickOutStreamEntry : AvatarStreamEntry
    {
        private LogicLong m_allianceId;
        private LogicLong m_senderHomeId;

        private string m_message;
        private string m_allianceName;

        private int m_allianceBadgeId;

        public AllianceKickOutStreamEntry()
        {
            this.m_allianceBadgeId = -1;
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteString(this.m_message);
            stream.WriteLong(this.m_allianceId);
            stream.WriteString(this.m_allianceName);
            stream.WriteInt(this.m_allianceBadgeId);

            if (this.m_senderHomeId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.m_senderHomeId);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_message = stream.ReadString(900000);
            this.m_allianceId = stream.ReadLong();
            this.m_allianceName = stream.ReadString(900000);
            this.m_allianceBadgeId = stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_senderHomeId = stream.ReadLong();
            }
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong allianceId)
        {
            this.m_allianceId = allianceId;
        }

        public LogicLong GetSenderHomeId()
        {
            return this.m_senderHomeId;
        }

        public void SetSenderHomeId(LogicLong allianceId)
        {
            this.m_senderHomeId = allianceId;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string name)
        {
            this.m_allianceName = name;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public string GetMessage()
        {
            return this.m_message;
        }

        public void SetMessage(string message)
        {
            this.m_message = message;
        }

        public override AvatarStreamEntryType GetAvatarStreamEntryType()
        {
            return AvatarStreamEntryType.ALLIANCE_KICKOUT;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("AllianceKickOutStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            LogicJSONNumber allianceIdHighNumber = jsonObject.GetJSONNumber("alli_id_high");
            LogicJSONNumber allianceIdLowNumber = jsonObject.GetJSONNumber("alli_id_low");

            if (allianceIdHighNumber != null && allianceIdLowNumber != null)
            {
                this.m_allianceId = new LogicLong(allianceIdHighNumber.GetIntValue(), allianceIdLowNumber.GetIntValue());
            }

            this.m_allianceName = LogicJSONHelper.GetString(jsonObject, "alli_name");
            this.m_allianceBadgeId = LogicJSONHelper.GetInt(jsonObject, "alli_badge_id");
            this.m_message = LogicJSONHelper.GetString(jsonObject, "message");

            LogicJSONNumber senderIdHighNumber = jsonObject.GetJSONNumber("sender_id_high");
            LogicJSONNumber senderIdLowNumber = jsonObject.GetJSONNumber("sender_id_low");

            if (senderIdHighNumber != null && senderIdLowNumber != null)
            {
                this.m_senderHomeId = new LogicLong(senderIdHighNumber.GetIntValue(), senderIdLowNumber.GetIntValue());
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("alli_id_high", new LogicJSONNumber(this.m_allianceId.GetHigherInt()));
            jsonObject.Put("alli_id_low", new LogicJSONNumber(this.m_allianceId.GetLowerInt()));
            jsonObject.Put("alli_name", new LogicJSONString(this.m_allianceName));
            jsonObject.Put("alli_badge_id", new LogicJSONNumber(this.m_allianceBadgeId));
            jsonObject.Put("message", new LogicJSONString(this.m_message));

            if (this.m_senderHomeId != null)
            {
                jsonObject.Put("sender_id_high", new LogicJSONNumber(this.m_senderHomeId.GetHigherInt()));
                jsonObject.Put("sender_id_low", new LogicJSONNumber(this.m_senderHomeId.GetLowerInt()));
            }
        }
    }
}