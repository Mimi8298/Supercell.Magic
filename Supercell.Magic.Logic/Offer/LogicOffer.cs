namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Json;

    public class LogicOffer
    {
        private readonly LogicLevel m_level;
        private readonly LogicOfferData m_data;

        private int m_state;
        private int m_payCount;

        public LogicOffer(LogicOfferData data, LogicLevel level)
        {
            this.m_data = data;
            this.m_level = level;
        }

        public LogicOfferData GetData()
        {
            return this.m_data;
        }

        public int GetId()
        {
            return this.m_data.GetId();
        }

        public int GetState()
        {
            return this.m_state;
        }

        public void SetState(int value)
        {
            this.m_state = value;
        }

        public LogicJSONObject Save()
        {
            if (this.m_payCount <= 0)
            {
                return null;
            }

            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put("data", new LogicJSONNumber(this.m_data.GetId()));
            jsonObject.Put("pc", new LogicJSONNumber(this.m_payCount));

            return jsonObject;
        }

        public void Load(LogicJSONObject jsonObject)
        {
            this.m_payCount = LogicJSONHelper.GetInt(jsonObject, "pc", 0);
        }

        public void AddPayCount(int value)
        {
            this.m_payCount += value;
            this.m_level.GetOfferManager().OfferBuyed(this);
        }
    }
}