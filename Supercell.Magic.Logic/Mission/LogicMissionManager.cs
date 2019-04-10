namespace Supercell.Magic.Logic.Mission
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Util;

    public class LogicMissionManager
    {
        private LogicLevel m_level;
        private LogicArrayList<LogicMission> m_missions;

        public LogicMissionManager(LogicLevel level)
        {
            this.m_level = level;
            this.m_missions = new LogicArrayList<LogicMission>();
        }

        public void Destruct()
        {
            if (this.m_missions != null)
            {
                for (int i = this.m_missions.Size() - 1; i >= 0; i--)
                {
                    this.m_missions[i].Destruct();
                    this.m_missions.Remove(i);
                }

                this.m_missions = null;
            }

            this.m_level = null;
        }

        public void LoadingFinished()
        {
            this.RefreshOpenMissions();
        }

        public void Tick()
        {
            bool refresh = false;

            for (int i = 0; i < this.m_missions.Size(); i++)
            {
                LogicMission mission = this.m_missions[i];

                if (mission != null)
                {
                    mission.RefreshProgress();

                    if (mission.IsFinished())
                    {
                        this.m_missions.Remove(i--);

                        mission.Destruct();
                        refresh = true;
                    }
                    else
                    {
                        mission.Tick();
                    }
                }
            }

            if (refresh)
            {
                this.RefreshOpenMissions();
            }
        }

        public void RefreshOpenMissions()
        {
            if (this.m_level.GetState() != 4)
            {
                LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                LogicDataTable missionTable = LogicDataTables.GetTable(LogicDataType.MISSION);

                for (int i = 0; i < missionTable.GetItemCount(); i++)
                {
                    LogicMissionData missionData = (LogicMissionData) missionTable.GetItemAt(i);

                    if (missionData.IsOpenForAvatar(playerAvatar))
                    {
                        if (this.GetMissionByData(missionData) == null)
                        {
                            LogicMission mission = new LogicMission(missionData, this.m_level);
                            mission.RefreshProgress();
                            this.m_missions.Add(mission);
                        }
                    }
                }
            }
        }

        public LogicMission GetMissionByData(LogicMissionData data)
        {
            for (int i = 0; i < this.m_missions.Size(); i++)
            {
                LogicMission mission = this.m_missions[i];

                if (mission.GetMissionData() == data)
                {
                    return mission;
                }
            }

            return null;
        }

        public LogicMission GetMissionByCategory(int category)
        {
            for (int i = 0; i < this.m_missions.Size(); i++)
            {
                LogicMission mission = this.m_missions[i];

                if (mission.GetMissionData().GetMissionCategory() == category)
                {
                    return mission;
                }
            }

            return null;
        }

        public bool IsTutorialFinished()
        {
            for (int i = 0; i < this.m_missions.Size(); i++)
            {
                if (this.m_missions[i].GetMissionData().GetMissionCategory() == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsVillage2TutorialOpen()
        {
            int openMissionCount = 0;

            for (int i = 0; i < this.m_missions.Size(); i++)
            {
                if (this.m_missions[i].GetMissionData().GetMissionCategory() == 2)
                {
                    if (this.m_missions[i].IsOpenTutorialMission())
                    {
                        openMissionCount += 1;
                    }
                }
            }

            if (this.m_level.GetGameObjectManagerAt(0).GetShipyard() == null)
            {
                return false;
            }

            return this.m_level.GetGameObjectManagerAt(0).GetShipyard().IsConstructing() || openMissionCount > 0;
        }

        public bool HasTravel(LogicAvatar playerAvatar)
        {
            LogicDataTable missionTable = LogicDataTables.GetTable(LogicDataType.MISSION);

            for (int i = 0; i < missionTable.GetItemCount(); i++)
            {
                LogicMissionData missionData = (LogicMissionData) missionTable.GetItemAt(i);

                if (missionData.GetMissionType() == 14)
                {
                    if (playerAvatar.IsMissionCompleted(missionData))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void DebugCompleteAllTutorials(bool onlyHomeMissions, bool completeNameMission, bool completeWarMissions)
        {
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.MISSION);

            bool restartMission = false;

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicMissionData data = (LogicMissionData) table.GetItemAt(i);

                if (!completeWarMissions && data.GetMissionCategory() == 1)
                    continue;

                if (onlyHomeMissions)
                {
                    if (data.GetMissionCategory() != 0)
                        continue;
                }
                else if (data.GetMissionCategory() == 2 &&
                         this.m_level.GetGameObjectManagerAt(0).GetShipyard().GetUpgradeLevel() == 0 &&
                         this.m_level.GetVillageType() == 0)
                {
                    continue;
                }

                if (restartMission)
                {
                    playerAvatar.SetMissionCompleted(data, false);
                    playerAvatar.GetChangeListener().CommodityCountChanged(0, data, 0);
                }

                if (!completeNameMission)
                {
                    if (data.GetMissionType() == 6)
                    {
                        restartMission = true;
                        continue;
                    }
                }

                playerAvatar.SetMissionCompleted(data, true);
                playerAvatar.GetChangeListener().CommodityCountChanged(0, data, 1);
            }

            this.RefreshOpenMissions();
        }

        public void DebugResetAllTutorials()
        {
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.MISSION);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicMissionData data = (LogicMissionData) table.GetItemAt(i);

                playerAvatar.SetMissionCompleted(data, false);
                playerAvatar.GetChangeListener().CommodityCountChanged(0, data, 0);
            }

            while (this.m_missions.Size() > 0)
            {
                this.m_missions[0].Destruct();
                this.m_missions.Remove(0);
            }

            this.RefreshOpenMissions();
        }

        public void DebugResetWarTutorials()
        {
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.MISSION);

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicMissionData data = (LogicMissionData)table.GetItemAt(i);

                if (data.GetMissionCategory() == 1)
                {
                    playerAvatar.SetMissionCompleted(data, false);
                    playerAvatar.GetChangeListener().CommodityCountChanged(0, data, 0);
                }
            }

            while (this.m_missions.Size() > 0)
            {
                this.m_missions[0].Destruct();
                this.m_missions.Remove(0);
            }

            this.RefreshOpenMissions();
        }
    }
}