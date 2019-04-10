namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public class AdminMessageAvatarStreamEntry : AvatarStreamEntry
    {
        private string m_titleTID;
        private string m_descriptionTID;
        private string m_buttonTID;
        private string m_helpshiftLink;
        private string m_urlLink;

        private int m_diamondCount;

        private bool m_supportMessage;
        private bool m_claimed;

        public AdminMessageAvatarStreamEntry()
        {
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteBoolean(false);
            stream.WriteString(this.m_titleTID);
            stream.WriteString(this.m_descriptionTID);
            stream.WriteString(this.m_helpshiftLink);
            stream.WriteString(this.m_urlLink);
            stream.WriteString(this.m_buttonTID);
            stream.WriteBoolean(this.m_supportMessage);
            stream.WriteInt(this.m_diamondCount);
            stream.WriteBoolean(this.m_claimed);
            stream.WriteInt(0);
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            stream.ReadBoolean();

            this.m_titleTID = stream.ReadString(900000);
            this.m_descriptionTID = stream.ReadString(900000);
            this.m_helpshiftLink = stream.ReadString(900000);
            this.m_urlLink = stream.ReadString(900000);
            this.m_buttonTID = stream.ReadString(900000);
            this.m_supportMessage = stream.ReadBoolean();
            this.m_diamondCount = stream.ReadInt();
            this.m_claimed = stream.ReadBoolean();

            stream.ReadInt();
        }

        public override AvatarStreamEntryType GetAvatarStreamEntryType()
        {
            return AvatarStreamEntryType.ADMIN_MESSAGE;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("AllianceInvitationAvatarStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_titleTID = jsonObject.GetJSONString("title").GetStringValue();
            this.m_descriptionTID = jsonObject.GetJSONString("description").GetStringValue();

            LogicJSONString buttonString = jsonObject.GetJSONString("button");

            if (buttonString != null)
            {
                this.m_buttonTID = buttonString.GetStringValue();
            }

            LogicJSONString helpshiftUrlString = jsonObject.GetJSONString("helpshift_url");

            if (helpshiftUrlString != null)
            {
                this.m_helpshiftLink = helpshiftUrlString.GetStringValue();
            }

            LogicJSONString urlString = jsonObject.GetJSONString("url");

            if (urlString != null)
            {
                this.m_urlLink = urlString.GetStringValue();
            }

            LogicJSONNumber diamondsNumber = jsonObject.GetJSONNumber("diamonds");

            if (diamondsNumber != null)
            {
                this.m_diamondCount = diamondsNumber.GetIntValue();
            }

            LogicJSONBoolean supportMessageBoolean = jsonObject.GetJSONBoolean("support_msg");

            if (supportMessageBoolean != null)
            {
                this.m_supportMessage = supportMessageBoolean.IsTrue();
            }

            LogicJSONBoolean claimedBoolean = jsonObject.GetJSONBoolean("claimed");

            if (claimedBoolean != null)
            {
                this.m_claimed = claimedBoolean.IsTrue();
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("title", new LogicJSONString(this.m_titleTID));
            jsonObject.Put("description", new LogicJSONString(this.m_descriptionTID));

            if (this.m_buttonTID != null)
            {
                jsonObject.Put("button", new LogicJSONString(this.m_buttonTID));
            }

            if (this.m_helpshiftLink != null)
            {
                jsonObject.Put("helpshift_url", new LogicJSONString(this.m_helpshiftLink));
            }

            if (this.m_urlLink != null)
            {
                jsonObject.Put("url", new LogicJSONString(this.m_urlLink));
            }

            if (this.m_diamondCount != 0)
            {
                jsonObject.Put("diamonds", new LogicJSONNumber(this.m_diamondCount));
            }

            if (this.m_supportMessage)
            {
                jsonObject.Put("support_msg", new LogicJSONBoolean(this.m_supportMessage));
            }

            if (this.m_claimed)
            {
                jsonObject.Put("claimed", new LogicJSONBoolean(this.m_claimed));
            }
        }

        public string GetTitleTID()
        {
            return this.m_titleTID;
        }

        public void SetTitleTID(string value)
        {
            this.m_titleTID = value;
        }

        public string GetDescriptionTID()
        {
            return this.m_descriptionTID;
        }

        public void SetDescriptionTID(string value)
        {
            this.m_descriptionTID = value;
        }

        public string GetButtonTID()
        {
            return this.m_buttonTID;
        }

        public void SetButtonTID(string value)
        {
            this.m_buttonTID = value;
        }

        public string GetHelpshiftLink()
        {
            return this.m_helpshiftLink;
        }

        public void SetHelpshiftLink(string value)
        {
            this.m_helpshiftLink = value;
        }

        public string GetUrlLink()
        {
            return this.m_urlLink;
        }

        public void SetUrlLink(string value)
        {
            this.m_urlLink = value;
        }

        public int GetDiamondCount()
        {
            return this.m_diamondCount;
        }

        public void SetDiamondCount(int value)
        {
            this.m_diamondCount = value;
        }

        public bool IsSupportMessage()
        {
            return this.m_supportMessage;
        }

        public void SetSupportMessage(bool value)
        {
            this.m_supportMessage = value;
        }

        public bool IsClaimed()
        {
            return this.m_claimed;
        }

        public void SetClaimed(bool value)
        {
            this.m_claimed = value;
        }
    }
}