namespace Supercell.Magic.Logic.Calendar
{
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Json;

    public class LogicDuelLootLimitCalendarEvent : LogicCalendarEvent
    {
        private int m_duelLootLimitCooldownInMinutes;
        private int m_duelBonusPercentWin;
        private int m_duelBonusPercentLose;
        private int m_duelBonusPercentDraw;
        private int m_duelBonusMaxDiamondCostPercent;

        public override void Load(LogicJSONObject jsonObject)
        {
            base.Load(jsonObject);

            this.m_duelLootLimitCooldownInMinutes = LogicJSONHelper.GetInt(jsonObject, "lootLimitCooldownInMinutes", 1320);
            this.m_duelBonusPercentWin = LogicJSONHelper.GetInt(jsonObject, "duelBonusPercentWin", 100);
            this.m_duelBonusPercentLose = LogicJSONHelper.GetInt(jsonObject, "duelBonusPercentLose", 0);
            this.m_duelBonusPercentDraw = LogicJSONHelper.GetInt(jsonObject, "duelBonusPercentDraw", 0);
            this.m_duelBonusMaxDiamondCostPercent = LogicJSONHelper.GetInt(jsonObject, "duelBonusMaxDiamondCostPercent", 100);
        }

        public override LogicJSONObject Save()
        {
            LogicJSONObject jsonObject = base.Save();

            jsonObject.Put("lootLimitCooldownInMinutes", new LogicJSONNumber(this.m_duelLootLimitCooldownInMinutes));
            jsonObject.Put("duelBonusPercentWin", new LogicJSONNumber(this.m_duelBonusPercentWin));
            jsonObject.Put("duelBonusPercentLose", new LogicJSONNumber(this.m_duelBonusPercentLose));
            jsonObject.Put("duelBonusPercentDraw", new LogicJSONNumber(this.m_duelBonusPercentDraw));
            jsonObject.Put("duelBonusMaxDiamondCostPercent", new LogicJSONNumber(this.m_duelBonusMaxDiamondCostPercent));

            return jsonObject;
        }

        public override int GetCalendarEventType()
        {
            return LogicCalendarEvent.EVENT_TYPE_DUEL_LOOT_LIMIT;
        }

        public int GetDuelLootLimitCooldownInMinutes()
        {
            return this.m_duelLootLimitCooldownInMinutes;
        }

        public void SetDuelLootLimitCooldownInMinutes(int value)
        {
            this.m_duelLootLimitCooldownInMinutes = value;
        }

        public int GetDuelBonusPercentWin()
        {
            return this.m_duelBonusPercentWin;
        }

        public void SetDuelBonusPercentWin(int value)
        {
            this.m_duelBonusPercentWin = value;
        }

        public int GetDuelBonusPercentLose()
        {
            return this.m_duelBonusPercentLose;
        }

        public void SetDuelBonusPercentLose(int value)
        {
            this.m_duelBonusPercentLose = value;
        }

        public int GetDuelBonusPercentDraw()
        {
            return this.m_duelBonusPercentDraw;
        }

        public void SetDuelBonusPercentDraw(int value)
        {
            this.m_duelBonusPercentDraw = value;
        }

        public int GetDuelBonusMaxDiamondCostPercent()
        {
            return this.m_duelBonusMaxDiamondCostPercent;
        }

        public void SetDuelBonusMaxDiamondCostPercent(int value)
        {
            this.m_duelBonusMaxDiamondCostPercent = value;
        }
    }
}