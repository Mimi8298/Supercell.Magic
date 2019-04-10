namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public class LogicCalendarTargeting
    {
        private int m_minTownHallLevel;
        private int m_maxTownHallLevel;
        private int m_minGemsLevel;
        private int m_maxGemsLevel;

        public LogicCalendarTargeting(LogicJSONObject jsonObject)
        {
            this.Load(jsonObject);
        }

        public void Load(LogicJSONObject jsonObject)
        {
            Debugger.DoAssert(jsonObject != null, "Unable to load targeting");

            this.m_minTownHallLevel = LogicJSONHelper.GetInt(jsonObject, "minTownHallLevel") & 0x7FFFFFFF;
            this.m_maxTownHallLevel = LogicJSONHelper.GetInt(jsonObject, "maxTownHallLevel") & 0x7FFFFFFF;
            this.m_minGemsLevel = LogicJSONHelper.GetInt(jsonObject, "minGemsPurchased") & 0x7FFFFFFF;
            this.m_maxGemsLevel = LogicJSONHelper.GetInt(jsonObject, "maxGemsPurchased") & 0x7FFFFFFF;
        }

        public void Save(LogicJSONObject jsonObject)
        {
            Debugger.DoAssert(jsonObject != null, "Unable to save targeting");

            jsonObject.Put("minTownHallLevel", new LogicJSONNumber(this.m_minTownHallLevel));
            jsonObject.Put("maxTownHallLevel", new LogicJSONNumber(this.m_maxTownHallLevel));
            jsonObject.Put("minGemsPurchased", new LogicJSONNumber(this.m_minGemsLevel));
            jsonObject.Put("maxGemsPurchased", new LogicJSONNumber(this.m_maxGemsLevel));
        }
    }
}