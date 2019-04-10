namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class AllianceInvitationAvatarStreamEntry : AvatarStreamEntry
    {
        private LogicLong m_allianceId;
        private LogicLong m_senderHomeId;

        private string m_allianceName;

        private int m_allianceBadgeId;
        private int m_allianceLevel;

        public AllianceInvitationAvatarStreamEntry()
        {
            this.m_allianceBadgeId = -1;
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

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

            stream.WriteInt(this.m_allianceLevel);
            stream.WriteBoolean(true);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_allianceId = stream.ReadLong();
            this.m_allianceName = stream.ReadString(900000);
            this.m_allianceBadgeId = stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_senderHomeId = stream.ReadLong();
            }

            this.m_allianceLevel = stream.ReadInt();
            stream.ReadBoolean();
        }

        public LogicLong GetAllianceId()
        {
            return this.m_allianceId;
        }

        public void SetAllianceId(LogicLong value)
        {
            this.m_allianceId = value;
        }

        public LogicLong GetSenderHomeId()
        {
            return this.m_senderHomeId;
        }

        public void SetSenderHomeId(LogicLong value)
        {
            this.m_senderHomeId = value;
        }

        public string GetAllianceName()
        {
            return this.m_allianceName;
        }

        public void SetAllianceName(string value)
        {
            this.m_allianceName = value;
        }

        public int GetAllianceBadgeId()
        {
            return this.m_allianceBadgeId;
        }

        public void SetAllianceBadgeId(int value)
        {
            this.m_allianceBadgeId = value;
        }

        public int GetAllianceLevel()
        {
            return this.m_allianceLevel;
        }

        public void SetAllianceLevel(int value)
        {
            this.m_allianceLevel = value;
        }

        public override AvatarStreamEntryType GetAvatarStreamEntryType()
        {
            return AvatarStreamEntryType.ALLIANCE_INVITATION;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("AllianceInvitationAvatarStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            LogicJSONNumber allianceIdHighNumber = jsonObject.GetJSONNumber("alli_id_high");
            LogicJSONNumber allianceIdLowNumber = jsonObject.GetJSONNumber("alli_id_low");

            if (allianceIdHighNumber != null && allianceIdLowNumber != null)
            {
                this.m_allianceId = new LogicLong(allianceIdHighNumber.GetIntValue(), allianceIdLowNumber.GetIntValue());
            }

            this.m_allianceName = jsonObject.GetJSONString("alli_name").GetStringValue();
            this.m_allianceBadgeId = jsonObject.GetJSONNumber("alli_badge_id").GetIntValue();
            this.m_allianceLevel = jsonObject.GetJSONNumber("alli_level").GetIntValue();

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
            jsonObject.Put("alli_level", new LogicJSONNumber(this.m_allianceLevel));

            if (this.m_senderHomeId != null)
            {
                jsonObject.Put("sender_id_high", new LogicJSONNumber(this.m_senderHomeId.GetHigherInt()));
                jsonObject.Put("sender_id_low", new LogicJSONNumber(this.m_senderHomeId.GetLowerInt()));
            }
        }
    }
}