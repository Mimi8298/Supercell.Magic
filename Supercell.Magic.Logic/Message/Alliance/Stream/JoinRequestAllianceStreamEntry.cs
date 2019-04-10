namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public class JoinRequestAllianceStreamEntry : StreamEntry
    {
        private string m_message;
        private string m_responderName;

        private int m_state;

        public JoinRequestAllianceStreamEntry()
        {
            this.m_state = 1;
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_message = stream.ReadString(900000);
            this.m_responderName = stream.ReadString(900000);
            this.m_state = stream.ReadInt();
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);

            stream.WriteString(this.m_message);
            stream.WriteString(this.m_responderName);
            stream.WriteInt(this.m_state);
        }

        public string GetMessage()
        {
            return this.m_message;
        }

        public void SetMessage(string value)
        {
            this.m_message = value;
        }

        public string GetResponderName()
        {
            return this.m_responderName;
        }

        public void SetResponderName(string value)
        {
            this.m_responderName = value;
        }

        public int GetState()
        {
            return this.m_state;
        }

        public void SetState(int value)
        {
            this.m_state = value;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.JOIN_REQUEST;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("JoinRequestAllianceStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            this.m_message = jsonObject.GetJSONString("message").GetStringValue();
            this.m_state = jsonObject.GetJSONNumber("state").GetIntValue();

            if (this.m_state != 1)
            {
                this.m_responderName = jsonObject.GetJSONString("responder_name").GetStringValue();
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("message", new LogicJSONString(this.m_message));
            jsonObject.Put("state", new LogicJSONNumber(this.m_state));

            if (this.m_state != 1)
            {
                jsonObject.Put("responder_name", new LogicJSONString(this.m_responderName));
            }
        }
    }
}