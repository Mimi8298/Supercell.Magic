namespace Supercell.Magic.Logic.Util
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Math;

    public static class LogicGamePlayUtil
    {
        public static int TimeToExp(int time)
        {
            return LogicMath.Sqrt(time);
        }

        public static int GetSpeedUpCost(int time, int speedUpType, int villageType)
        {
            int multiplier = 100;

            switch (speedUpType)
            {
                case 1:
                    multiplier = LogicDataTables.GetGlobals().UseNewTraining()
                        ? LogicDataTables.GetGlobals().GetSpellTrainingCostMultiplier()
                        : LogicDataTables.GetGlobals().GetSpellSpeedUpCostMultiplier();
                    break;
                case 2:
                    multiplier = LogicDataTables.GetGlobals().GetHeroHealthSpeedUpCostMultipler();
                    break;
                case 3:
                    multiplier = LogicDataTables.GetGlobals().GetTroopRequestSpeedUpCostMultiplier();
                    break;
                case 4:
                    multiplier = LogicDataTables.GetGlobals().GetTroopTrainingCostMultiplier();
                    break;
                case 5:
                    multiplier = LogicDataTables.GetGlobals().GetSpeedUpBoostCooldownCostMultiplier();
                    break;
                case 6:
                    multiplier = LogicDataTables.GetGlobals().GetClockTowerSpeedUpMultiplier();
                    break;
            }

            return LogicDataTables.GetGlobals().GetSpeedUpCost(time, multiplier, villageType);
        }

        public static int GetResourceDiamondCost(int count, LogicResourceData data)
        {
            return LogicDataTables.GetGlobals().GetResourceDiamondCost(count, data);
        }

        public static LogicLeagueVillage2Data GetLeagueVillage2(int score)
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.LEAGUE_VILLAGE2);

            for (int i = table.GetItemCount() - 1; i >= 0; i--)
            {
                LogicLeagueVillage2Data data = (LogicLeagueVillage2Data) table.GetItemAt(i);

                if (data.GetTrophyLimitHigh() > score && data.GetTrophyLimitLow() <= score)
                {
                    return data;
                }
            }

            return (LogicLeagueVillage2Data) table.GetItemAt(0);
        }

        public static void CalculateCombatScore(LogicClientAvatar attacker, LogicClientAvatar defender, int stars, bool ignoreLeague, bool revenge, int destructionPercentage,
                                                int starBonusMultiplier, bool duelMatch)
        {
            int multiplier = stars;

            if (stars <= 0)
            {
                multiplier = LogicDataTables.GetGlobals().GetScoreMultiplierOnAttackLose();
            }

            int attackerScore = attacker.GetScore();
            int defenderScore = defender.GetScore();

            LogicLeagueData attackerLeagueData = attacker.GetLeagueTypeData();
            LogicLeagueData defenderLeagueData = defender.GetLeagueTypeData();

            int newAttackerScore;
            int newDefenderScore;

            if (LogicDataTables.GetGlobals().EloOffsetDampeningEnabled())
            {
                newAttackerScore = LogicELOMath.CalculateNewRating(stars > 0, attackerScore, defenderScore, 20 * multiplier, LogicDataTables.GetGlobals().GetEloDampeningFactor(),
                                                                   LogicDataTables.GetGlobals().GetEloDampeningLimit(), LogicDataTables.GetGlobals().GetEloDampeningScoreLimit());
                newDefenderScore = LogicELOMath.CalculateNewRating(stars <= 0, defenderScore, attackerScore, 20 * multiplier, LogicDataTables.GetGlobals().GetEloDampeningFactor(),
                                                                   LogicDataTables.GetGlobals().GetEloDampeningLimit(), LogicDataTables.GetGlobals().GetEloDampeningScoreLimit());
            }
            else
            {
                newAttackerScore = LogicELOMath.CalculateNewRating(stars > 0, attackerScore, defenderScore, 20 * multiplier);
                newDefenderScore = LogicELOMath.CalculateNewRating(stars <= 0, defenderScore, attackerScore, 20 * multiplier);
            }

            int attackerGainCount = newAttackerScore - attackerScore;
            int defenderGainCount = newDefenderScore - defenderScore;

            if (attackerScore < 1000 && attackerGainCount < 0)
            {
                attackerGainCount = attackerScore * attackerGainCount / 1000;
            }

            if (defenderScore < 1000 && defenderGainCount < 0)
            {
                defenderGainCount = defenderScore * defenderGainCount / 1000;
            }

            if (LogicELOMath.CalculateNewRating(true, attackerScore, defenderScore, 60) > attackerScore)
            {
                if (stars <= 0)
                {
                    if (attackerGainCount >= 0)
                    {
                        attackerGainCount = -1;
                    }
                }
                else
                {
                    if (attackerGainCount <= 0)
                    {
                        attackerGainCount = 1;
                    }

                    if (defenderGainCount >= 0)
                    {
                        defenderGainCount = -1;
                    }
                }
            }

            newAttackerScore = LogicMath.Max(attackerScore + attackerGainCount, 0);
            newDefenderScore = LogicMath.Max(defenderScore + defenderGainCount, 0);

            if (!ignoreLeague)
            {
                attacker.SetScore(newAttackerScore);
                defender.SetScore(newDefenderScore);

                if (LogicDataTables.GetGlobals().EnableLeagues())
                {
                    if (!duelMatch)
                    {
                        if (attackerLeagueData != null)
                        {
                            if (stars <= 0)
                            {
                                attacker.SetAttackLoseCount(attacker.GetAttackLoseCount() + 1);
                            }
                            else
                            {
                                attacker.SetAttackWinCount(attacker.GetAttackWinCount() + 1);
                            }
                        }

                        if (defenderLeagueData != null)
                        {
                            if (stars > 0)
                            {
                                defender.SetDefenseLoseCount(defender.GetDefenseLoseCount() + 1);
                            }
                            else
                            {
                                defender.SetDefenseWinCount(defender.GetDefenseLoseCount() + 1);
                            }
                        }

                        if (stars > 0)
                        {
                            if (!revenge || LogicDataTables.GetGlobals().RevengeGiveLeagueBonus())
                            {
                                int leagueBonusPercentage = LogicDataTables.GetGlobals().GetLeagueBonusPercentage(destructionPercentage);

                                attacker.CommodityCountChangeHelper(0, LogicDataTables.GetGoldData(), attackerLeagueData.GetGoldReward() * leagueBonusPercentage / 100);
                                attacker.CommodityCountChangeHelper(0, LogicDataTables.GetElixirData(), attackerLeagueData.GetElixirReward() * leagueBonusPercentage / 100);

                                if (attacker.IsDarkElixirUnlocked())
                                {
                                    attacker.CommodityCountChangeHelper(0, LogicDataTables.GetDarkElixirData(),
                                                                        attackerLeagueData.GetDarkElixirReward() * leagueBonusPercentage / 100);
                                }
                            }
                        }

                        LogicGamePlayUtil.UpdateLeagueRank(attacker, newAttackerScore, false);
                        LogicGamePlayUtil.UpdateLeagueRank(defender, newDefenderScore, true);
                    }
                }
            }

            attacker.GetChangeListener().ScoreChanged(attacker.GetCurrentHomeId(), attackerGainCount, stars > 0 ? 1 : -1, true, attacker.GetLeagueTypeData(), attackerLeagueData,
                                                      destructionPercentage);
            defender.GetChangeListener().ScoreChanged(defender.GetCurrentHomeId(), defenderGainCount, stars > 0 ? -1 : 1, false, defender.GetLeagueTypeData(), defenderLeagueData,
                                                      destructionPercentage);

            if (stars > 0 && !ignoreLeague && !duelMatch && (!revenge || LogicDataTables.GetGlobals().RevengeGiveStarBonus()) && !attacker.GetStarBonusCooldown())
            {
                int totalStars = stars + attacker.GetStarBonusCounter();

                if (totalStars >= LogicDataTables.GetGlobals().GetStarBonusStarCount())
                {
                    LogicLeagueData leagueData = attacker.GetLeagueTypeData();

                    attacker.AddStarBonusReward(leagueData.GetGoldRewardStarBonus() * starBonusMultiplier, leagueData.GetElixirRewardStarBonus() * starBonusMultiplier,
                                                leagueData.GetDarkElixirRewardStarBonus() * starBonusMultiplier);
                    attacker.StarBonusCollected();

                    if (LogicDataTables.GetGlobals().AllowStarsOverflowInStarBonus() && !attacker.GetStarBonusCooldown())
                    {
                        totalStars %= LogicDataTables.GetGlobals().GetStarBonusStarCount();
                    }
                    else
                    {
                        totalStars = 0;
                    }
                }

                attacker.SetStarBonusCounter(totalStars);
            }
        }

        public static void UpdateLeagueRank(LogicClientAvatar clientAvatar, int score, bool defender)
        {
            if (LogicDataTables.GetGlobals().EnableLeagues())
            {
                LogicDataTable leagueTable = LogicDataTables.GetTable(LogicDataType.LEAGUE);
                LogicLeagueData data = clientAvatar.GetLeagueTypeData();

                int leagueType = clientAvatar.GetLeagueType();

                if (leagueType != 0)
                {
                    int leagueItemCount = leagueTable.GetItemCount();

                    if (data.GetPromoteLimit() <= score)
                    {
                        if (data.IsPromoteEnabled())
                        {
                            while (leagueType + 1 < leagueItemCount)
                            {
                                LogicLeagueData leagueData = (LogicLeagueData) leagueTable.GetItemAt(++leagueType);

                                if (leagueData.GetPromoteLimit() > score || !leagueData.IsPromoteEnabled())
                                {
                                    break;
                                }
                            }

                            if (leagueType != clientAvatar.GetLeagueType())
                            {
                                clientAvatar.SetLeagueType(leagueType);
                            }
                        }
                    }
                    else if (data.GetDemoteLimit() >= score)
                    {
                        if (data.IsDemoteEnabled())
                        {
                            while (leagueType > 0)
                            {
                                LogicLeagueData leagueData = (LogicLeagueData) leagueTable.GetItemAt(--leagueType);

                                if (leagueData.GetDemoteLimit() < score || !leagueData.IsDemoteEnabled())
                                {
                                    break;
                                }
                            }

                            if (leagueType != clientAvatar.GetLeagueType())
                            {
                                clientAvatar.SetLeagueType(leagueType);
                            }
                        }
                    }
                }
                else if (!defender)
                {
                    for (int i = 0; i < leagueTable.GetItemCount(); i++)
                    {
                        LogicLeagueData leagueData = (LogicLeagueData) leagueTable.GetItemAt(i);

                        if (leagueData.GetPlacementLimitLow() <= score && leagueData.GetPlacementLimitHigh() >= score)
                        {
                            if (i != 0)
                            {
                                clientAvatar.SetLeagueType(i);
                            }

                            break;
                        }
                    }
                }
            }
        }

        public static int DPSToSingleHit(int dps, int ms)
        {
            return dps * ms / 10;
        }

        public static bool GetNearestValidAttackPos(LogicLevel level, int x, int y, out int outputX, out int outputY)
        {
            if ((uint) x > 0xFFFFF600)
            {
                x = 0;
            }

            if ((uint) y > 0xFFFFF600)
            {
                y = 0;
            }

            int width = level.GetWidth();
            int height = level.GetHeight();

            if (level.IsNpcVillage())
            {
                if (x <= 0)
                {
                    x = 512;
                }

                if (y <= 0)
                {
                    y = 512;
                }

                if (x - 2048 > width || y - 2048 > height)
                {
                    outputX = 0;
                    outputY = 0;

                    return false;
                }

                width -= 512;
                height -= 512;
            }

            int startX = x;
            int startY = y;

            if (x < width + 2560)
            {
                startX = x <= width ? x : width;
            }

            if (y < height + 2560)
            {
                startY = y <= height ? y : height;
            }

            int tileX = startX / 512 - 1;
            int tileY = startY / 512 - 1;

            for (int i = 0; i < 9; i++)
            {
                int randomPos = (i + 4) % 9;
                int randomX = tileX + randomPos % 3;
                int randomY = tileY + randomPos / 3;

                LogicTile tile = level.GetTileMap().GetTile(randomX, randomY);

                if (tile != null && level.GetTileMap().IsValidAttackPos(randomX, randomY))
                {
                    if (tile.GetPassableFlag() != 0)
                    {
                        if (i == 0)
                        {
                            outputX = startX;
                            outputY = startY;

                            return true;
                        }

                        outputX = randomX << 9;
                        outputY = randomY << 9;

                        return true;
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        if (tile.IsPassablePathFinder(j & 1, j >> 1))
                        {
                            outputX = (randomX << 9) | ((j & 1) << 8);
                            outputY = (randomY << 9) + ((j >> 1) << 8);

                            return true;
                        }
                    }
                }
            }

            outputX = 0;
            outputY = 0;

            return false;
        }

        public static bool FindGoodDuplicatePosAround(LogicLevel level, int x, int y, out int outputX, out int outputY, int radiusInTiles)
        {
            int width = level.GetWidth();
            int height = level.GetHeight();
            int radius = radiusInTiles << 9;

            if (-512 * radiusInTiles < x)
            {
                x = x >= 0 ? x : 0;
            }

            if (-512 * radiusInTiles < y)
            {
                y = y >= 0 ? y : 0;
            }

            if (level.IsNpcVillage())
            {
                if (x <= 0)
                {
                    x = 512;
                }

                if (y <= 0)
                {
                    y = 512;
                }

                width -= 512;
                height -= 512;

                if (x - radius > width || y - radius > height)
                {
                    outputX = 0;
                    outputY = 0;

                    return false;
                }
            }

            int startX = x;
            int startY = y;

            if (x < width + radius)
            {
                startX = x <= width ? x : width;
            }

            if (y < height + radius)
            {
                startY = y <= height ? y : height;
            }

            int tileX = startX / 512 - 1;
            int tileY = startY / 512 - 1;

            for (int i = 0; i < 9; i++)
            {
                int randomPos = (i + 4) % 9;
                int randomX = tileX + randomPos % 3;
                int randomY = tileY + randomPos / 3;

                LogicTile tile = level.GetTileMap().GetTile(randomX, randomY);

                if (tile != null)
                {
                    if (tile.GetPassableFlag() != 0)
                    {
                        if (i == 0)
                        {
                            outputX = startX;
                            outputY = startY;

                            return true;
                        }

                        outputX = randomX << 9;
                        outputY = randomY << 9;

                        return true;
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        if (tile.IsPassablePathFinder(j & 1, j >> 1))
                        {
                            outputX = (randomX << 9) | ((j & 1) << 8);
                            outputY = (randomY << 9) + ((j >> 1) << 8);

                            return true;
                        }
                    }
                }
            }

            outputX = 0;
            outputY = 0;

            return false;
        }
    }
}