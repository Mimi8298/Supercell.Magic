namespace Supercell.Magic.Logic
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Util;

    public class LogicConfiguration
    {
        private LogicJSONObject m_json;
        private LogicObstacleData m_specialObstacle;
        private LogicArrayList<int> m_milestoneScoreChangeForLosing;
        private LogicArrayList<int> m_percentageScoreChangeForLosing;
        private LogicArrayList<int> m_milestoneStrengthRangeForScore;
        private LogicArrayList<int> m_percentageStrengthRangeForScore;

        private bool m_battleWaitForDieDamage;
        private bool m_battleWaitForProjectileDestruction;

        private int m_maxTownHallLevel;
        private int m_duelLootLimitCooldownInMinutes;
        private int m_duelBonusLimitWinsPerDay;
        private int m_duelBonusPercentWin;
        private int m_duelBonusPercentLose;
        private int m_duelBonusPercentDraw;
        private int m_duelBonusMaxDiamondCostPercent;

        private string m_giftPackExtension;

        public LogicConfiguration()
        {
            this.m_maxTownHallLevel = 8;
        }

        public LogicJSONObject GetJson()
        {
            return this.m_json;
        }

        public bool GetBattleWaitForProjectileDestruction()
        {
            return this.m_battleWaitForProjectileDestruction;
        }

        public bool GetBattleWaitForDieDamage()
        {
            return this.m_battleWaitForDieDamage;
        }

        public int GetMaxTownHallLevel()
        {
            return this.m_maxTownHallLevel;
        }

        public int GetDuelBonusLimitWinsPerDay()
        {
            return this.m_duelBonusLimitWinsPerDay;
        }

        public int GetDuelLootLimitCooldownInMinutes()
        {
            return this.m_duelLootLimitCooldownInMinutes;
        }

        public int GetDuelBonusMaxDiamondCostPercent()
        {
            return this.m_duelBonusMaxDiamondCostPercent;
        }

        public LogicObstacleData GetSpecialObstacleData()
        {
            return this.m_specialObstacle;
        }

        public void Load(LogicJSONObject jsonObject)
        {
            this.m_json = jsonObject;

            if (jsonObject != null)
            {
                LogicJSONObject village1Object = jsonObject.GetJSONObject("Village1");
                Debugger.DoAssert(village1Object != null, "pVillage1 = NULL!");

                LogicJSONString specialObstacleObject = village1Object.GetJSONString("SpecialObstacle");

                if (specialObstacleObject != null)
                {
                    this.m_specialObstacle = LogicDataTables.GetObstacleByName(specialObstacleObject.GetStringValue(), null);
                }

                LogicJSONObject village2Object = jsonObject.GetJSONObject("Village2");
                Debugger.DoAssert(village2Object != null, "pVillage2 = NULL!");

                this.m_maxTownHallLevel = LogicJSONHelper.GetInt(village2Object, "TownHallMaxLevel");

                LogicJSONArray scoreChangeForLosingArray = village2Object.GetJSONArray("ScoreChangeForLosing");
                Debugger.DoAssert(scoreChangeForLosingArray != null, "ScoreChangeForLosing array is null");

                this.m_milestoneScoreChangeForLosing = new LogicArrayList<int>(scoreChangeForLosingArray.Size());
                this.m_percentageScoreChangeForLosing = new LogicArrayList<int>(scoreChangeForLosingArray.Size());

                for (int i = 0; i < scoreChangeForLosingArray.Size(); i++)
                {
                    LogicJSONObject obj = scoreChangeForLosingArray.GetJSONObject(i);

                    if (obj != null)
                    {
                        LogicJSONNumber milestoneObject = obj.GetJSONNumber("Milestone");
                        LogicJSONNumber percentageObject = obj.GetJSONNumber("Percentage");

                        if (milestoneObject != null && percentageObject != null)
                        {
                            this.m_milestoneScoreChangeForLosing.Add(milestoneObject.GetIntValue());
                            this.m_percentageScoreChangeForLosing.Add(percentageObject.GetIntValue());
                        }
                    }
                }

                LogicJSONArray strengthRangeForScoreArray = village2Object.GetJSONArray("StrengthRangeForScore");
                Debugger.DoAssert(strengthRangeForScoreArray != null, "StrengthRangeForScore array is null");

                this.m_milestoneStrengthRangeForScore = new LogicArrayList<int>(strengthRangeForScoreArray.Size());
                this.m_percentageStrengthRangeForScore = new LogicArrayList<int>(strengthRangeForScoreArray.Size());

                for (int i = 0; i < strengthRangeForScoreArray.Size(); i++)
                {
                    LogicJSONObject obj = strengthRangeForScoreArray.GetJSONObject(i);

                    if (obj != null)
                    {
                        LogicJSONNumber milestoneObject = obj.GetJSONNumber("Milestone");
                        LogicJSONNumber percentageObject = obj.GetJSONNumber("Percentage");

                        if (milestoneObject != null && percentageObject != null)
                        {
                            this.m_milestoneStrengthRangeForScore.Add(milestoneObject.GetIntValue());
                            this.m_percentageStrengthRangeForScore.Add(percentageObject.GetIntValue());
                        }
                    }
                }

                LogicJSONObject killSwitchesObject = jsonObject.GetJSONObject("KillSwitches");
                Debugger.DoAssert(killSwitchesObject != null, "pKillSwitches = NULL!");

                this.m_battleWaitForProjectileDestruction = LogicJSONHelper.GetBool(killSwitchesObject, "BattleWaitForProjectileDestruction");
                this.m_battleWaitForDieDamage = LogicJSONHelper.GetBool(killSwitchesObject, "BattleWaitForDieDamage");

                LogicJSONObject globalsObject = jsonObject.GetJSONObject("Globals");
                Debugger.DoAssert(globalsObject != null, "pGlobals = NULL!");

                this.m_giftPackExtension = LogicJSONHelper.GetString(globalsObject, "GiftPackExtension");

                this.m_duelLootLimitCooldownInMinutes = LogicJSONHelper.GetInt(globalsObject, "DuelLootLimitCooldownInMinutes");
                this.m_duelBonusLimitWinsPerDay = LogicJSONHelper.GetInt(globalsObject, "DuelBonusLimitWinsPerDay");
                this.m_duelBonusPercentWin = LogicJSONHelper.GetInt(globalsObject, "DuelBonusPercentWin");
                this.m_duelBonusPercentLose = LogicJSONHelper.GetInt(globalsObject, "DuelBonusPercentLose");
                this.m_duelBonusPercentDraw = LogicJSONHelper.GetInt(globalsObject, "DuelBonusPercentDraw");
                this.m_duelBonusMaxDiamondCostPercent = LogicJSONHelper.GetInt(globalsObject, "DuelBonusMaxDiamondCostPercent");
            }
            else
            {
                Debugger.Error("pConfiguration = NULL!");
            }
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
    }
}