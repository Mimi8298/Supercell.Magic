namespace Supercell.Magic.Logic.Mission
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicMission
    {
        private LogicLevel m_level;
        private LogicMissionData m_data;

        private int m_progress;
        private int m_requireProgress;

        private bool m_finished;

        public LogicMission(LogicMissionData data, LogicLevel level)
        {
            if (data == null)
            {
                Debugger.Error("LogicMission::constructor - pData is NULL!");
            }

            this.m_data = data;
            this.m_level = level;
            this.m_requireProgress = 1;

            switch (data.GetMissionType())
            {
                case 1:
                case 16:
                case 17:
                case 18:
                case 19:
                    this.m_requireProgress = 2;
                    break;
                case 0:
                case 5:
                    this.m_requireProgress = data.GetBuildBuildingCount();
                    break;
                case 4:
                    this.m_requireProgress = data.GetTrainTroopCount();
                    break;
            }

            if (data.GetMissionCategory() == 1)
            {
                this.m_requireProgress = 0;
            }
        }

        public void Destruct()
        {
            this.m_data = null;
            this.m_level = null;
            this.m_progress = 0;
            this.m_requireProgress = 0;
        }

        public int GetMissionType()
        {
            return this.m_data.GetMissionType();
        }

        public LogicMissionData GetMissionData()
        {
            return this.m_data;
        }

        public int GetProgress()
        {
            return this.m_progress;
        }

        public void RefreshProgress()
        {
            LogicGameObjectManager gameObjectManager = this.m_level.GetGameObjectManager();

            switch (this.m_data.GetMissionType())
            {
                case 0:
                case 5:
                    this.m_progress = 0;

                    if (this.m_level.GetState() == 1)
                    {
                        LogicArrayList<LogicGameObject> gameObjects = gameObjectManager.GetGameObjects(LogicGameObjectType.BUILDING);

                        for (int i = 0; i < gameObjects.Size(); i++)
                        {
                            LogicBuilding building = (LogicBuilding) gameObjects[i];

                            if (building.GetBuildingData() == this.m_data.GetBuildBuildingData() && (!building.IsConstructing() || building.IsUpgrading()) &&
                                building.GetUpgradeLevel() >= this.m_data.GetBuildBuildingLevel())
                            {
                                ++this.m_progress;
                            }
                        }
                    }

                    break;
                case 4:
                    this.m_progress = this.m_level.GetPlayerAvatar().GetUnitsTotalCapacity();
                    break;
                case 6:
                    if (this.m_level.GetPlayerAvatar().GetNameSetByUser())
                    {
                        this.m_progress = 1;
                    }

                    break;
                case 13:
                    this.m_progress = 0;

                    if (this.m_level.GetState() == 1)
                    {
                        LogicArrayList<LogicGameObject> gameObjects = gameObjectManager.GetGameObjects(LogicGameObjectType.VILLAGE_OBJECT);

                        for (int i = 0; i < gameObjects.Size(); i++)
                        {
                            LogicVillageObject villageObject = (LogicVillageObject) gameObjects[i];

                            if (villageObject.GetVillageObjectData() == this.m_data.GetFixVillageObjectData() &&
                                villageObject.GetUpgradeLevel() >= this.m_data.GetBuildBuildingLevel())
                            {
                                ++this.m_progress;
                            }
                        }
                    }

                    break;
                case 14:
                    this.m_progress = 0;

                    if (this.m_level.GetState() == 1 && this.m_level.GetVillageType() == 1)
                    {
                        ++this.m_progress;
                    }

                    break;
                case 15:
                    this.m_progress = 0;

                    if (this.m_level.GetState() == 1)
                    {
                        LogicArrayList<LogicGameObject> gameObjects = gameObjectManager.GetGameObjects(LogicGameObjectType.BUILDING);

                        for (int i = 0; i < gameObjects.Size(); i++)
                        {
                            LogicBuilding building = (LogicBuilding) gameObjects[i];

                            if (building.GetBuildingData() == this.m_data.GetBuildBuildingData() && !building.IsLocked())
                            {
                                ++this.m_progress;
                            }
                        }
                    }

                    break;
                case 17:
                    this.m_progress = 0;

                    if (this.m_level.GetState() == 1 && this.m_level.GetVillageType() == 1)
                    {
                        if (this.m_level.GetPlayerAvatar().GetUnitUpgradeLevel(this.m_data.GetCharacterData()) > 0)
                        {
                            this.m_progress = 2;
                        }
                    }

                    break;
            }

            if (this.m_progress >= this.m_requireProgress)
            {
                this.m_progress = this.m_requireProgress;
                this.Finished();
            }
        }

        public void StateChangeConfirmed()
        {
            switch (this.m_data.GetMissionType())
            {
                case 1:
                    if (this.m_progress == 0)
                    {
                        this.m_level.GetGameMode().StartDefendState(LogicNpcAvatar.GetNpcAvatar(this.m_data.GetDefendNpcData()));
                        this.m_progress = 1;
                    }

                    break;
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 20:
                case 21:
                    this.m_progress = 1;
                    this.Finished();

                    break;
                case 16:
                    if (this.m_progress == 0)
                    {
                        // ?
                    }

                    this.m_progress += 1;
                    break;
                case 19:
                    if (this.m_progress == 1)
                    {
                        LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                        int duelScoreGain = LogicDataTables.GetGlobals().GetVillage2FirstVictoryTrophies();

                        playerAvatar.AddDuelReward(LogicDataTables.GetGlobals().GetVillage2FirstVictoryGold(), LogicDataTables.GetGlobals().GetVillage2FirstVictoryElixir(), 0, 0,
                                                   null);
                        playerAvatar.SetDuelScore(playerAvatar.GetDuelScore() + LogicDataTables.GetGlobals().GetVillage2FirstVictoryTrophies());
                        playerAvatar.GetChangeListener().DuelScoreChanged(playerAvatar.GetAllianceId(), duelScoreGain, -1, false);

                        this.m_progress = 2;
                        this.Finished();
                    }

                    break;
            }
        }

        public void Finished()
        {
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

            if (!playerAvatar.IsMissionCompleted(this.m_data))
            {
                playerAvatar.SetMissionCompleted(this.m_data, true);
                playerAvatar.GetChangeListener().CommodityCountChanged(0, this.m_data, 1);

                this.AddRewardUnits();

                LogicResourceData rewardResourceData = this.m_data.GetRewardResourceData();

                if (rewardResourceData != null)
                {
                    playerAvatar.AddMissionResourceReward(rewardResourceData, this.m_data.GetRewardResourceCount());
                }

                int rewardXp = this.m_data.GetRewardXp();

                if (rewardXp > 0)
                {
                    playerAvatar.XpGainHelper(rewardXp);
                }
            }

            this.m_finished = true;
        }

        public bool IsOpenTutorialMission()
        {
            if (this.m_data.GetVillageType() == this.m_level.GetVillageType())
            {
                if (this.m_data.GetMissionCategory() == 2)
                {
                    LogicAvatar homeOwnerAvatar = this.m_level.GetHomeOwnerAvatar();

                    if (homeOwnerAvatar == null || !homeOwnerAvatar.IsNpcAvatar() || this.m_level.GetVillageType() != 1)
                    {
                        LogicGameObjectManager gameObjectManager = this.m_level.GetGameObjectManagerAt(0);
                        LogicVillageObject shipyard = gameObjectManager.GetShipyard();

                        if (shipyard == null || shipyard.GetUpgradeLevel() != 0)
                        {
                            int missionType = this.m_data.GetMissionType();

                            if ((missionType == 16 || missionType == 14) && this.m_level.GetState() == 1 && this.m_level.GetVillageType() == 0)
                            {
                                if (gameObjectManager.GetShipyard().IsConstructing())
                                {
                                    return false;
                                }
                            }

                            return this.m_data.GetMissionCategory() != 1;
                        }

                        return false;
                    }

                    return true;
                }

                return this.m_data.GetMissionCategory() != 1;
            }

            return false;
        }

        public void Tick()
        {
            int missionType = this.m_data.GetMissionType();

            switch (missionType)
            {
                case 1:
                    if (this.m_level.GetState() == 1 && this.m_progress == 1)
                    {
                        this.Finished();
                    }

                    break;
                case 2:
                    if (this.m_level.GetHomeOwnerAvatar().IsNpcAvatar())
                    {
                        if (this.m_level.GetState() == 2)
                        {
                            this.Finished();
                            this.m_level.GetGameListener().ShowTroopPlacementTutorial(this.m_data.GetCustomData());
                        }
                    }

                    break;
                case 18:
                    if (this.m_progress == 0)
                    {
                        if (this.m_level.GetHomeOwnerAvatar().IsNpcAvatar() && this.m_level.GetState() == 2)
                        {
                            this.m_progress = 1;
                            this.m_level.GetGameListener().ShowTroopPlacementTutorial(this.m_data.GetCustomData());
                        }
                    }
                    else if (this.m_progress == 1)
                    {
                        if (this.m_level.GetHomeOwnerAvatar().IsNpcAvatar() && this.m_level.GetState() == 2)
                        {
                            if (this.m_level.GetBattleLog().GetBattleEnded())
                            {
                                this.m_progress = 2;
                                this.Finished();
                            }
                        }
                    }

                    break;
                case 19:
                    if (this.m_level.GetState() == 1 && this.m_progress == 0)
                    {
                        this.m_progress = 1;
                    }

                    break;
            }
        }

        public bool IsFinished()
        {
            return this.m_finished;
        }

        public void AddRewardUnits()
        {
            LogicCharacterData characterData = this.m_data.GetRewardCharacterData();

            if (characterData != null)
            {
                int characterCount = this.m_data.GetRewardCharacterCount();

                if (characterCount > 0)
                {
                    LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();
                    LogicComponentFilter filter = new LogicComponentFilter();

                    for (int i = 0; i < characterCount; i++)
                    {
                        filter.RemoveAllIgnoreObjects();

                        while (true)
                        {
                            LogicUnitStorageComponent component =
                                (LogicUnitStorageComponent) this.m_level.GetComponentManagerAt(this.m_level.GetVillageType()).GetClosestComponent(0, 0, filter);

                            if (component != null)
                            {
                                if (component.CanAddUnit(characterData))
                                {
                                    playerAvatar.CommodityCountChangeHelper(0, characterData, 1);
                                    component.AddUnit(characterData);

                                    if (this.m_level.GetState() == 1 || this.m_level.GetState() == 3)
                                    {
                                        if (component.GetParentListener() != null)
                                        {
                                            component.GetParentListener().ExtraCharacterAdded(characterData, null);
                                        }
                                    }

                                    break;
                                }

                                filter.AddIgnoreObject(component.GetParent());
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}