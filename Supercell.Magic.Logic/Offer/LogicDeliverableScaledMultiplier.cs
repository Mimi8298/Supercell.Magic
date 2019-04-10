namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Json;

    public class LogicDeliverableScaledMultiplier : LogicDeliverable
    {
        private LogicResourceData m_scaledResourceData;
        private int m_scaledResourceMultiplier;

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void WriteToJSON(LogicJSONObject jsonObject)
        {
            base.WriteToJSON(jsonObject);

            LogicJSONHelper.SetLogicData(jsonObject, "scaledResource", this.m_scaledResourceData);
            jsonObject.Put("scaledResourceMultiplier", new LogicJSONNumber(this.m_scaledResourceMultiplier));
        }

        public override void ReadFromJSON(LogicJSONObject jsonObject)
        {
            base.ReadFromJSON(jsonObject);

            this.m_scaledResourceData = (LogicResourceData) LogicJSONHelper.GetLogicData(jsonObject, "scaledResource");
            this.m_scaledResourceMultiplier = LogicJSONHelper.GetInt(jsonObject, "scaledResourceMultiplier");
        }

        public override int GetDeliverableType()
        {
            return 7;
        }

        public override bool Deliver(LogicLevel level)
        {
            LogicAvatar avatar = level.GetHomeOwnerAvatar();
            int count = avatar.GetResourceCount(this.m_scaledResourceData) + this.m_scaledResourceMultiplier;

            avatar.SetResourceCount(this.m_scaledResourceData, count);
            avatar.GetChangeListener().CommodityCountChanged(0, this.m_scaledResourceData, count);

            return true;
        }

        public override bool CanBeDeliver(LogicLevel level)
        {
            return true;
        }

        public LogicResourceData GetScaledResourceData()
        {
            return this.m_scaledResourceData;
        }

        public void SetScaledResourceData(LogicResourceData data)
        {
            this.m_scaledResourceData = data;
        }

        public int GetScaledResourceMultiplier()
        {
            return this.m_scaledResourceMultiplier;
        }

        public void SetScaledResourceMultiplier(int value)
        {
            this.m_scaledResourceMultiplier = value;
        }
    }
}