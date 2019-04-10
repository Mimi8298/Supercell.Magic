namespace Supercell.Magic.Logic.Achievement
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicAchievementManager
    {
        private LogicLevel m_level;

        public LogicAchievementManager(LogicLevel level)
        {
            this.m_level = level;
        }

        public void Destruct()
        {
            this.m_level = null;
        }

        public void ObstacleCleared()
        {
            LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();
            LogicDataTable dataTable = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);

            if (homeOwnerAvatar != null)
            {
                if (homeOwnerAvatar.IsClientAvatar())
                {
                    LogicClientAvatar clientAvatar = (LogicClientAvatar) homeOwnerAvatar;

                    for (int i = 0; i < dataTable.GetItemCount(); i++)
                    {
                        LogicAchievementData achievementData = (LogicAchievementData) dataTable.GetItemAt(i);

                        if (achievementData.GetActionType() == LogicAchievementData.ACTION_TYPE_CLEAR_OBSTACLES)
                        {
                            this.RefreshAchievementProgress(clientAvatar, achievementData, clientAvatar.GetAchievementProgress(achievementData) + 1);
                        }
                    }
                }
            }
        }

        public void RefreshStatus()
        {
            if (this.m_level.GetState() == 1)
            {
                LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                LogicDataTable dataTable = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);

                for (int i = 0, progress = 0; i < dataTable.GetItemCount(); i++, progress = 0)
                {
                    LogicAchievementData achievementData = (LogicAchievementData) dataTable.GetItemAt(i);

                    switch (achievementData.GetActionType())
                    {
                        case LogicAchievementData.ACTION_TYPE_NPC_STARS:
                            progress = playerAvatar.GetTotalNpcStars();
                            break;
                        case LogicAchievementData.ACTION_TYPE_UPGRADE:
                            progress = this.m_level.GetGameObjectManager().GetHighestBuildingLevel(achievementData.GetBuildingData()) + 1;
                            break;
                        case LogicAchievementData.ACTION_TYPE_VICTORY_POINTS:
                            progress = playerAvatar.GetScore();
                            break;
                        case LogicAchievementData.ACTION_TYPE_UNIT_UNLOCK:
                            progress = achievementData.GetCharacterData()
                                                      .IsUnlockedForBarrackLevel(
                                                          LogicMath.Max(this.m_level.GetComponentManagerAt(achievementData.GetVillageType()).GetMaxBarrackLevel(), 0))
                                ? 1
                                : 0;
                            break;
                        case LogicAchievementData.ACTION_TYPE_LEAGUE:
                            progress = playerAvatar.GetLeagueType();
                            break;
                        case LogicAchievementData.ACTION_TYPE_ACCOUNT_BOUND:
                            progress = playerAvatar.IsAccountBound() ? 1 : 0;
                            break;
                        case LogicAchievementData.ACTION_TYPE_VERSUS_BATTLE_TROPHIES:
                            progress = playerAvatar.GetDuelScore();
                            break;
                        case LogicAchievementData.ACTION_TYPE_GEAR_UP:
                            progress = this.m_level.GetGameObjectManager().GetGearUpBuildingCount();
                            break;
                        case LogicAchievementData.ACTION_TYPE_REPAIR_BUILDING:
                            LogicArrayList<LogicAchievementData> achievementLevels = achievementData.GetAchievementLevels();

                            if (achievementLevels.Size() > 0)
                            {
                                for (int j = 0; j < achievementLevels.Size(); j++)
                                {
                                    if (!this.IsBuildingUnlocked(achievementLevels[j].GetBuildingData()))
                                    {
                                        break;
                                    }

                                    if (achievementLevels[j] == achievementData)
                                    {
                                        progress = 1;
                                        break;
                                    }
                                }
                            }

                            break;
                    }

                    this.RefreshAchievementProgress(playerAvatar, achievementData, progress);
                }
            }
        }

        public void RefreshAchievementProgress(LogicClientAvatar avatar, LogicAchievementData data, int value)
        {
            if (this.m_level.GetState() != 5)
            {
                int currentValue = avatar.GetAchievementProgress(data);
                int newValue = LogicMath.Min(value, 2000000000);

                if (currentValue < newValue)
                {
                    avatar.SetAchievementProgress(data, value);
                    avatar.GetChangeListener().CommodityCountChanged(0, data, newValue);
                }

                int tmp = LogicMath.Min(newValue, data.GetActionCount());

                if (currentValue < tmp)
                {
                    LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

                    if (playerAvatar == avatar)
                    {
                        if (tmp == data.GetActionCount())
                        {
                            this.m_level.GetGameListener().AchievementCompleted(data);
                        }
                        else
                        {
                            this.m_level.GetGameListener().AchievementProgress(data);
                        }
                    }
                }
            }
        }

        public void AlianceUnitDonated(LogicCharacterData data)
        {
            if (data != null)
            {
                LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                LogicDataTable dataTable = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);

                for (int i = 0; i < dataTable.GetItemCount(); i++)
                {
                    LogicAchievementData achievementData = (LogicAchievementData) dataTable.GetItemAt(i);

                    if (achievementData.GetActionType() == LogicAchievementData.ACTION_TYPE_DONATE_UNITS)
                    {
                        this.RefreshAchievementProgress(playerAvatar, achievementData, playerAvatar.GetAchievementProgress(achievementData) + data.GetHousingSpace());
                    }
                }
            }
        }

        public void AlianceSpellDonated(LogicSpellData data)
        {
            if (data != null)
            {
                LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                LogicDataTable dataTable = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);

                for (int i = 0; i < dataTable.GetItemCount(); i++)
                {
                    LogicAchievementData achievementData = (LogicAchievementData) dataTable.GetItemAt(i);

                    if (achievementData.GetActionType() == LogicAchievementData.ACTION_TYPE_DONATE_SPELLS)
                    {
                        this.RefreshAchievementProgress(playerAvatar, achievementData, playerAvatar.GetAchievementProgress(achievementData) + data.GetHousingSpace());
                    }
                }
            }
        }

        public void PvpAttackWon()
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicAchievementData data = (LogicAchievementData) table.GetItemAt(i);

                if (data.GetActionType() == LogicAchievementData.ACTION_TYPE_WIN_PVP_ATTACK)
                {
                    this.RefreshAchievementProgress(playerAvatar, data, playerAvatar.GetAchievementProgress(data) + 1);
                }
            }
        }

        public void PvpDefenseWon()
        {
            if (this.m_level.GetHomeOwnerAvatar().IsClientAvatar())
            {
                LogicDataTable table = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);
                LogicClientAvatar homeOwnerAvatar = (LogicClientAvatar) this.m_level.GetHomeOwnerAvatar();

                for (int i = 0; i < table.GetItemCount(); i++)
                {
                    LogicAchievementData data = (LogicAchievementData) table.GetItemAt(i);

                    if (data.GetActionType() == LogicAchievementData.ACTION_TYPE_WIN_PVP_DEFENSE)
                    {
                        this.RefreshAchievementProgress(homeOwnerAvatar, data, homeOwnerAvatar.GetAchievementProgress(data) + 1);
                    }
                }
            }
        }

        public void IncreaseWarStars(int stars)
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicAchievementData data = (LogicAchievementData) table.GetItemAt(i);

                if (data.GetActionType() == LogicAchievementData.ACTION_TYPE_WAR_STARS)
                {
                    this.RefreshAchievementProgress(playerAvatar, data, playerAvatar.GetAchievementProgress(data) + stars);
                }
            }
        }

        public void IncreaseLoot(LogicResourceData resourceData, int count)
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicAchievementData data = (LogicAchievementData) table.GetItemAt(i);

                if (data.GetActionType() == LogicAchievementData.ACTION_TYPE_LOOT && data.GetResourceData() == resourceData)
                {
                    this.RefreshAchievementProgress(playerAvatar, data, playerAvatar.GetAchievementProgress(data) + count);
                }
            }
        }

        public void IncreaseWarGoldResourceLoot(int count)
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicAchievementData data = (LogicAchievementData) table.GetItemAt(i);

                if (data.GetActionType() == LogicAchievementData.ACTION_TYPE_WAR_LOOT)
                {
                    this.RefreshAchievementProgress(playerAvatar, data, playerAvatar.GetAchievementProgress(data) + count);
                }
            }
        }

        public void BuildingDestroyedInPvP(LogicBuildingData buildingData)
        {
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.ACHIEVEMENT);
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicAchievementData data = (LogicAchievementData) table.GetItemAt(i);

                if (data.GetActionType() == LogicAchievementData.ACTION_TYPE_DESTROY && data.GetBuildingData() == buildingData)
                {
                    this.RefreshAchievementProgress(playerAvatar, data, playerAvatar.GetAchievementProgress(data) + 1);
                }
            }
        }

        public bool IsBuildingUnlocked(LogicBuildingData buildingData)
        {
            LogicArrayList<LogicGameObject> gameObjects =
                this.m_level.GetGameObjectManagerAt(buildingData.GetVillageType()).GetGameObjects(LogicGameObjectType.BUILDING);

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];

                if (building.GetData() == buildingData && !building.IsLocked())
                {
                    return true;
                }
            }

            return false;
        }

        public void Tick()
        {
        }
    }
}