namespace Supercell.Magic.Logic.Message.Scoring
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class RankingEntry
    {
        private const string JSON_ATTRIBUTE_ID = "id";
        private const string JSON_ATTRIBUTE_NAME = "name";
        private const string JSON_ATTRIBUTE_ORDER = "o";
        private const string JSON_ATTRIBUTE_PREVIOUS_ORDER = "prevO";
        private const string JSON_ATTRIBUTE_SCORE = "scr";

        private LogicLong m_id;

        private string m_name;

        private int m_order;
        private int m_previousOrder;
        private int m_score;

        public virtual void Encode(ByteStream stream)
        {
            stream.WriteLong(this.m_id);
            stream.WriteString(this.m_name);
            stream.WriteInt(this.m_order);
            stream.WriteInt(this.m_score);
            stream.WriteInt(this.m_previousOrder);
        }

        public virtual void Decode(ByteStream stream)
        {
            this.m_id = stream.ReadLong();
            this.m_name = stream.ReadString(900000);
            this.m_order = stream.ReadInt();
            this.m_score = stream.ReadInt();
            this.m_previousOrder = stream.ReadInt();
        }

        public LogicLong GetId()
        {
            return this.m_id;
        }

        public void SetId(LogicLong value)
        {
            this.m_id = value;
        }

        public string GetName()
        {
            return this.m_name;
        }

        public void SetName(string name)
        {
            this.m_name = name;
        }

        public int GetOrder()
        {
            return this.m_order;
        }

        public void SetOrder(int order)
        {
            this.m_order = order;
        }

        public int GetScore()
        {
            return this.m_score;
        }

        public void SetScore(int value)
        {
            this.m_score = value;
        }

        public int GetPreviousOrder()
        {
            return this.m_previousOrder;
        }

        public void SetPreviousOrder(int order)
        {
            this.m_previousOrder = order;
        }

        public virtual LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = new LogicJSONObject();
            LogicJSONArray idArray = new LogicJSONArray(2);

            idArray.Add(new LogicJSONNumber(this.m_id.GetHigherInt()));
            idArray.Add(new LogicJSONNumber(this.m_id.GetLowerInt()));

            jsonObject.Put(RankingEntry.JSON_ATTRIBUTE_ID, idArray);
            jsonObject.Put(RankingEntry.JSON_ATTRIBUTE_NAME, new LogicJSONString(this.m_name));
            jsonObject.Put(RankingEntry.JSON_ATTRIBUTE_ORDER, new LogicJSONNumber(this.m_order));
            jsonObject.Put(RankingEntry.JSON_ATTRIBUTE_PREVIOUS_ORDER, new LogicJSONNumber(this.m_previousOrder));
            jsonObject.Put(RankingEntry.JSON_ATTRIBUTE_SCORE, new LogicJSONNumber(this.m_score));

            return jsonObject;
        }

        public virtual void Load(LogicJSONObject jsonObject)
        {
            LogicJSONArray idArray = jsonObject.GetJSONArray(RankingEntry.JSON_ATTRIBUTE_ID);

            this.m_id = new LogicLong(idArray.GetJSONNumber(0).GetIntValue(), idArray.GetJSONNumber(1).GetIntValue());
            this.m_name = jsonObject.GetJSONString(RankingEntry.JSON_ATTRIBUTE_NAME).GetStringValue();
            this.m_order = jsonObject.GetJSONNumber(RankingEntry.JSON_ATTRIBUTE_ORDER).GetIntValue();
            this.m_previousOrder = jsonObject.GetJSONNumber(RankingEntry.JSON_ATTRIBUTE_PREVIOUS_ORDER).GetIntValue();
            this.m_score = jsonObject.GetJSONNumber(RankingEntry.JSON_ATTRIBUTE_SCORE).GetIntValue();
        }
    }
}