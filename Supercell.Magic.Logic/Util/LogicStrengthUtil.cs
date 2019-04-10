namespace Supercell.Magic.Logic.Util
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class LogicStrengthUtil
    {
        public static float GetAttackStrength(int townHallLevel, int castleUpgLevel, LogicArrayList<LogicHeroData> unlockedHeroes, int[] heroUpgLevel,
                                              LogicArrayList<LogicCharacterData> unlockedCharacters, int[] characterUpgLevel, int totalHousingSpace,
                                              LogicArrayList<LogicSpellData> unlockedSpells, int[] spellUpgLevel, int totalSpellHousingSpace)
        {
            float castleStrength = 0f;
            float heroStrength = 0f;
            float housingSpellStrength = 0f;
            float[] chStrengths = new float[unlockedCharacters.Size()];

            if (castleUpgLevel > -1)
            {
                castleStrength = (castleUpgLevel + 1) * 10f * (castleUpgLevel >= 4 ? 4f : 2f);
                Debugger.HudPrint("(Attack Strength) Clan Castle: " + castleStrength);
            }

            for (int i = 0; i < unlockedHeroes.Size(); i++)
            {
                heroStrength += LogicStrengthUtil.GetHeroStrength(unlockedHeroes[i], heroUpgLevel[i], false);
            }

            for (int i = 0; i < unlockedCharacters.Size(); i++)
            {
                chStrengths[i] = LogicStrengthUtil.GetCharacterStrength(unlockedCharacters[i], characterUpgLevel[i]);
            }

            float housingCharacterStrength = LogicStrengthUtil.GetGlobalCharacterStrength(unlockedCharacters, chStrengths, townHallLevel);
            float characterStrength = totalHousingSpace * 0.01f * housingCharacterStrength;

            for (int i = 0; i < unlockedSpells.Size(); i++)
            {
                housingSpellStrength += LogicStrengthUtil.GetSpellStrength(unlockedSpells[i], spellUpgLevel[i]);
            }

            float spellStrength = totalSpellHousingSpace * 0.5f * housingSpellStrength;

            Debugger.HudPrint(string.Format("(Attack Strength) Heroes: {0}", heroStrength));
            Debugger.HudPrint(string.Format("(Attack Strength) Troops: ({0} * {1} army capacity) {2}", housingCharacterStrength, totalHousingSpace, characterStrength));
            Debugger.HudPrint(string.Format("(Attack Strength) Spells: ({0} * {1} spell capacity) {2}", housingSpellStrength, totalSpellHousingSpace, spellStrength));

            return castleStrength + heroStrength + characterStrength + spellStrength;
        }

        public static float GetHeroStrength(LogicHeroData data, int upgLevel, bool useHeroStrengthWeight)
        {
            if (data.IsProductionEnabled())
            {
                float startStrength = data.GetHitpoints(upgLevel) * 0.04f + data.GetAttackerItemData(upgLevel).GetDamagePerMS(0, false) * 0.2f;
                return startStrength * 0.1f * (useHeroStrengthWeight ? data.GetStrengthWeight2(upgLevel) : data.GetStrengthWeight(upgLevel));
            }

            return 0f;
        }

        public static float GetCharacterStrength(LogicCharacterData data, int upgLevel)
        {
            if (data.IsProductionEnabled())
            {
                float attackStrength = data.GetHitpoints(upgLevel) * 0.04f +
                                       LogicMath.Abs(data.GetAttackerItemData(upgLevel).GetDamagePerMS(0, false)) * 0.2f;

                if (data.GetUnitsInCamp(upgLevel) > 0 && data.GetUnitsInCamp(0) > 0)
                {
                    attackStrength = (float) data.GetUnitsInCamp(upgLevel) / data.GetUnitsInCamp(0) * attackStrength;
                }

                for (int i = data.GetSpecialAbilityLevel(upgLevel); i > 0; i--)
                {
                    attackStrength *= 1.1f;
                }

                return attackStrength * 0.01f * data.GetStrengthWeight(upgLevel) / data.GetHousingSpace() * 10f;
            }

            return 0f;
        }

        public static float GetSpellStrength(LogicSpellData data, int upgLevel)
        {
            if (data.IsProductionEnabled())
            {
                int damage = data.GetDamage(upgLevel);

                if (!data.IsDamageSpell())
                {
                    damage = -damage;
                }

                int buildingDamage = data.GetBuildingDamagePermil(upgLevel);
                int troopDamage = data.GetTroopDamagePermil(upgLevel);

                int sumDamage = damage / 100 + buildingDamage + troopDamage;

                if (data.GetDamageBoostPercent(upgLevel) > 0)
                {
                    sumDamage += data.GetDamageBoostPercent(upgLevel) - 100;
                }

                int damageStrength = sumDamage + data.GetSpeedBoost(upgLevel);

                if (damageStrength == 0)
                {
                    if (data.GetJumpBoostMS(upgLevel) != 0)
                    {
                        damageStrength = data.GetJumpBoostMS(upgLevel) / 1000;
                    }

                    if (data.GetFreezeTimeMS(upgLevel) != 0)
                    {
                        damageStrength = data.GetFreezeTimeMS(upgLevel) / 1000;
                    }

                    if (data.GetDuplicateHousing(upgLevel) != 0)
                    {
                        damageStrength = data.GetDuplicateHousing(upgLevel);
                    }

                    if (data.GetUnitsToSpawn(upgLevel) != 0)
                    {
                        damageStrength = data.GetUnitsToSpawn(upgLevel);
                    }
                }

                return damageStrength * 0.014286f * data.GetStrengthWeight(upgLevel);
            }

            return 0f;
        }

        public static float GetGlobalCharacterStrength(LogicArrayList<LogicCharacterData> characterData, float[] strength, int townHallLevel)
        {
            LogicTownhallLevelData townhallLevelData = LogicDataTables.GetTownHallLevel(townHallLevel);
            Debugger.HudPrint(string.Format("TH{0} counts up to {1} troops (out of {2} available):", townHallLevel + 1, townhallLevelData.GetStrengthMaxTroopTypes(),
                                            characterData.Size()));

            float sumStrength = 0;

            for (int i = LogicMath.Min(townhallLevelData.GetStrengthMaxTroopTypes(), characterData.Size()); i > 0; i--)
            {
                float max = 0f;
                int maxIdx = 0;

                for (int j = 0; j < characterData.Size(); j++)
                {
                    if (strength[j] > max)
                    {
                        max = strength[j];
                        maxIdx = j;
                    }
                }

                if (max == 0f)
                {
                    break;
                }

                sumStrength += max;
                strength[maxIdx] = 0f;

                Debugger.HudPrint(string.Format("{0}. {1} ({2})", i + 1, characterData[maxIdx].GetName(), max));
            }

            return sumStrength;
        }

        public static float GetDefenseStrength(LogicArrayList<LogicGameObject> buildings, LogicArrayList<LogicGameObject> traps, LogicArrayList<LogicHeroData> heroes,
                                               int[] heroUpgLevel)
        {
            float heroStrength = 0f;
            float buildingStrength = 0f;
            float trapStrength = 0;

            for (int i = 0; i < heroes.Size(); i++)
            {
                heroStrength += LogicStrengthUtil.GetHeroStrength(heroes[i], heroUpgLevel[i], true);
            }

            for (int i = 0; i < buildings.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) buildings[i];

                if (!building.IsLocked())
                {
                    if (!building.IsConstructing() || building.IsUpgrading())
                    {
                        buildingStrength += LogicStrengthUtil.GetBuildingStrength(building.GetBuildingData(), building.GetUpgradeLevel());
                    }
                }
            }

            for (int i = 0; i < traps.Size(); i++)
            {
                LogicTrap trap = (LogicTrap) traps[i];

                if (!trap.IsConstructing() || trap.IsUpgrading())
                {
                    trapStrength += LogicStrengthUtil.GetTrapStrength(trap.GetTrapData(), trap.GetUpgradeLevel());
                }
            }

            return buildingStrength + trapStrength + heroStrength;
        }

        public static float GetBuildingStrength(LogicBuildingData data, int upgLevel)
        {
            float startStrength = data.GetHitpoints(upgLevel) * 0.04f + data.GetAttackerItemData(upgLevel).GetDamagePerMS(0, false) * 0.2f;
            return startStrength * 0.1f * data.GetStrengthWeight(upgLevel);
        }

        public static float GetTrapStrength(LogicTrapData data, int upgLevel)
        {
            int damageStrength = data.GetDamage(upgLevel);

            if (damageStrength == 0)
            {
                if (data.GetNumSpawns(upgLevel) != 0)
                {
                    damageStrength = data.GetNumSpawns(upgLevel);
                }

                if (data.GetEjectHousingLimit(upgLevel) != 0)
                {
                    damageStrength = data.GetEjectHousingLimit(upgLevel);
                }

                if (data.GetPushback() != 0)
                {
                    damageStrength = data.GetPushback();
                }
            }

            return damageStrength * 0.014286f * data.GetStrengthWeight(upgLevel);
        }
    }
}