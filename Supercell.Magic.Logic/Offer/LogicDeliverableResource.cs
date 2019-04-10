namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Json;

    public class LogicDeliverableResource : LogicDeliverable
    {
        private LogicResourceData m_resourceData;
        private int m_resourceAmount;

        public override void WriteToJSON(LogicJSONObject jsonObject)
        {
            base.WriteToJSON(jsonObject);

            LogicJSONHelper.SetLogicData(jsonObject, "resource", this.m_resourceData);
            jsonObject.Put("resourceAmount", new LogicJSONNumber(this.m_resourceAmount));
        }

        public override void ReadFromJSON(LogicJSONObject jsonObject)
        {
            base.ReadFromJSON(jsonObject);

            this.m_resourceData = (LogicResourceData) LogicJSONHelper.GetLogicData(jsonObject, "resource");
            this.m_resourceAmount = LogicJSONHelper.GetInt(jsonObject, "resourceAmount");
        }

        public override int GetDeliverableType()
        {
            return 1;
        }

        public override bool Deliver(LogicLevel level)
        {
            LogicAvatar avatar = level.GetHomeOwnerAvatar();
            int count = avatar.GetResourceCount(this.m_resourceData) + this.m_resourceAmount;

            avatar.SetResourceCount(this.m_resourceData, count);
            avatar.GetChangeListener().CommodityCountChanged(0, this.m_resourceData, count);

            return true;
        }

        public LogicResourceData GetResourceData()
        {
            return this.m_resourceData;
        }

        public void SetResourceData(LogicResourceData data)
        {
            this.m_resourceData = data;
        }

        public int GetResourceAmount()
        {
            return this.m_resourceAmount;
        }

        public void SetResourceAmount(int value)
        {
            this.m_resourceAmount = value;
        }
    }
}