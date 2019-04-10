namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Logic.Data;

    public class LogicCalendarBuildingDestroyedSpawnUnit
    {
        private readonly LogicBuildingData m_buildingData;
        private readonly LogicCharacterData m_characterData;

        private readonly int m_count;

        public LogicCalendarBuildingDestroyedSpawnUnit(LogicBuildingData buildingData, LogicCharacterData unitData, int count)
        {
            this.m_buildingData = buildingData;
            this.m_characterData = unitData;
            this.m_count = count;
        }

        public LogicBuildingData GetBuildingData()
        {
            return this.m_buildingData;
        }

        public LogicCharacterData GetCharacterData()
        {
            return this.m_characterData;
        }

        public int GetCount()
        {
            return this.m_count;
        }
    }
}