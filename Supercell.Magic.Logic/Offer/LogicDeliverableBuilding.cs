namespace Supercell.Magic.Logic.Offer
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.Json;

    public class LogicDeliverableBuilding : LogicDeliverable
    {
        private LogicBuildingData m_buildingData;

        private int m_buildingLevel;
        private int m_buildingCount;

        public override void Destruct()
        {
            base.Destruct();

            this.m_buildingData = null;
            this.m_buildingCount = 0;
            this.m_buildingLevel = 0;
        }

        public override void WriteToJSON(LogicJSONObject jsonObject)
        {
            base.WriteToJSON(jsonObject);

            LogicJSONHelper.SetLogicData(jsonObject, "building", this.m_buildingData);

            jsonObject.Put("buildingNumber", new LogicJSONNumber(this.m_buildingCount));
            jsonObject.Put("buildingLevel", new LogicJSONNumber(this.m_buildingLevel));
        }

        public override void ReadFromJSON(LogicJSONObject jsonObject)
        {
            base.ReadFromJSON(jsonObject);

            this.m_buildingData = (LogicBuildingData) LogicJSONHelper.GetLogicData(jsonObject, "building");
            this.m_buildingCount = LogicJSONHelper.GetInt(jsonObject, "buildingNumber");
            this.m_buildingLevel = LogicJSONHelper.GetInt(jsonObject, "buildingLevel");
        }

        public override int GetDeliverableType()
        {
            return 2;
        }

        public override bool Deliver(LogicLevel level)
        {
            if (this.CanBeDeliver(level))
            {
                level.AddUnplacedObject(new LogicDataSlot(this.m_buildingData, this.m_buildingLevel));
                return true;
            }

            return false;
        }

        public override bool CanBeDeliver(LogicLevel level)
        {
            int placedBuildingCount = level.GetObjectCount(this.m_buildingData, this.m_buildingData.GetVillageType());
            int townHallLevel = this.m_buildingData.GetVillageType() == 1
                ? level.GetHomeOwnerAvatar().GetVillage2TownHallLevel()
                : level.GetHomeOwnerAvatar().GetTownHallLevel();
            int unlockedBuildingCount = LogicDataTables.GetTownHallLevel(townHallLevel).GetUnlockedBuildingCount(this.m_buildingData);

            if (placedBuildingCount >= unlockedBuildingCount || this.m_buildingCount != 0)
            {
                return this.m_buildingCount == placedBuildingCount + 1;
            }

            return true;
        }

        public override LogicDeliverableBundle Compensate(LogicLevel level)
        {
            LogicDeliverableBundle logicDeliverableBundle = new LogicDeliverableBundle();

            if (this.m_buildingData.IsWorkerBuilding())
            {
                logicDeliverableBundle.AddResources(this.m_buildingData.GetBuildResource(0), LogicDataTables.GetGlobals().GetWorkerCost(level));
            }
            else
            {
                for (int i = 0; i <= this.m_buildingLevel; i++)
                {
                    logicDeliverableBundle.AddResources(this.m_buildingData.GetBuildResource(i), this.m_buildingData.GetBuildCost(i, level));
                }
            }

            return logicDeliverableBundle;
        }

        public LogicBuildingData GetBuildingData()
        {
            return this.m_buildingData;
        }

        public void SetBuildingData(LogicBuildingData data)
        {
            this.m_buildingData = data;
        }

        public int GetBuildingLevel()
        {
            return this.m_buildingLevel;
        }

        public void SetBuildingLevel(int value)
        {
            this.m_buildingLevel = value;
        }

        public int GetBuildingCount()
        {
            return this.m_buildingCount;
        }

        public void SetBuildingCount(int value)
        {
            this.m_buildingCount = value;
        }
    }
}