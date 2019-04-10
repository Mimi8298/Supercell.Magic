namespace Supercell.Magic.Logic.Message.Alliance.Stream
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public class ChatStreamEntry : StreamEntry
    {
        private string m_message;

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.m_message = stream.ReadString(900000);
        }

        public override void Encode(ByteStream stream)
        {
            base.Encode(stream);
            stream.WriteString(this.m_message);
        }

        public string GetMessage()
        {
            return this.m_message;
        }

        public void SetMessage(string message)
        {
            this.m_message = message;
        }

        public override StreamEntryType GetStreamEntryType()
        {
            return StreamEntryType.CHAT;
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = jsonObject.GetJSONObject("base");

            if (baseObject == null)
            {
                Debugger.Error("ChatStreamEntry::load base is NULL");
            }

            base.Load(baseObject);

            LogicJSONString messageObject = jsonObject.GetJSONString("message");

            if (messageObject != null)
            {
                this.m_message = messageObject.GetStringValue();
            }
        }

        public override void Save(LogicJSONObject jsonObject)
        {
            LogicJSONObject baseObject = new LogicJSONObject();

            base.Save(baseObject);

            jsonObject.Put("base", baseObject);
            jsonObject.Put("message", new LogicJSONString(this.m_message));
        }
    }
}