namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Json;

    public class LogicDeliverableDecoration : LogicDeliverable
    {
        private LogicDecoData m_decoData;

        public override void Destruct()
        {
            base.Destruct();
            this.m_decoData = null;
        }

        public override void WriteToJSON(LogicJSONObject jsonObject)
        {
            base.WriteToJSON(jsonObject);
            LogicJSONHelper.SetLogicData(jsonObject, "decoration", this.m_decoData);
        }

        public override void ReadFromJSON(LogicJSONObject jsonObject)
        {
            base.ReadFromJSON(jsonObject);
            this.m_decoData = (LogicDecoData) LogicJSONHelper.GetLogicData(jsonObject, "decoration");
        }

        public override int GetDeliverableType()
        {
            return 3;
        }

        public override bool Deliver(LogicLevel level)
        {
            if (this.CanBeDeliver(level))
            {
                level.AddUnplacedObject(new LogicDataSlot(this.m_decoData, 0));
                return true;
            }

            return false;
        }

        public override bool CanBeDeliver(LogicLevel level)
        {
            return level.GetObjectCount(this.m_decoData, this.m_decoData.GetVillageType()) < this.m_decoData.GetMaxCount();
        }

        public override LogicDeliverableBundle Compensate(LogicLevel level)
        {
            LogicDeliverableBundle logicDeliverableBundle = new LogicDeliverableBundle();
            logicDeliverableBundle.AddResources(this.m_decoData.GetBuildResource(), this.m_decoData.GetBuildCost());
            return logicDeliverableBundle;
        }

        public LogicDecoData GetDecorationData()
        {
            return this.m_decoData;
        }

        public void SetDecorationData(LogicDecoData data)
        {
            this.m_decoData = data;
        }
    }
}