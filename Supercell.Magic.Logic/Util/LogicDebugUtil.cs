namespace Supercell.Magic.Logic.Util
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public static class LogicDebugUtil
    {
        public static void LoadDebugJSON(LogicLevel level, string json)
        {
            LogicJSONObject jsonObject = LogicJSONParser.ParseObject(json);

            if (jsonObject != null)
            {
                LogicArrayList<LogicComponent> unitStorageComponents = level.GetComponentManager().GetComponents(LogicComponentType.UNIT_STORAGE);

                for (int i = 0; i < unitStorageComponents.Size(); i++)
                {
                    ((LogicUnitStorageComponent) unitStorageComponents[i]).RemoveAllUnits();
                }

                level.SetLoadingVillageType(0);

                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("buildings"), LogicGameObjectType.BUILDING, 0);
                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("obstacles"), LogicGameObjectType.OBSTACLE, 0);
                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("traps"), LogicGameObjectType.TRAP, 0);
                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("decos"), LogicGameObjectType.DECO, 0);

                level.SetLoadingVillageType(1);

                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("buildings2"), LogicGameObjectType.BUILDING, 1);
                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("obstacles2"), LogicGameObjectType.OBSTACLE, 1);
                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("traps2"), LogicGameObjectType.TRAP, 1);
                LogicDebugUtil.LoadDebugJSONArray(level, jsonObject.GetJSONArray("decos2"), LogicGameObjectType.DECO, 1);

                level.SetLoadingVillageType(-1);
            }
        }

        public static void LoadDebugJSONArray(LogicLevel level, LogicJSONArray jsonArray, LogicGameObjectType gameObjectType, int villageType)
        {
            if (jsonArray != null)
            {
                LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(villageType);
                LogicArrayList<LogicGameObject> prevGameObjects = new LogicArrayList<LogicGameObject>();

                prevGameObjects.AddAll(gameObjectManager.GetGameObjects(gameObjectType));

                for (int i = 0; i < prevGameObjects.Size(); i++)
                {
                    gameObjectManager.RemoveGameObject(prevGameObjects[i]);
                }

                for (int i = 0; i < jsonArray.Size(); i++)
                {
                    LogicJSONObject jsonObject = jsonArray.GetJSONObject(i);
                    LogicJSONNumber dataNumber = jsonObject.GetJSONNumber("data");
                    LogicJSONNumber lvlNumber = jsonObject.GetJSONNumber("lvl");
                    LogicJSONBoolean lockedBoolean = jsonObject.GetJSONBoolean("locked");
                    LogicJSONNumber xNumber = jsonObject.GetJSONNumber("x");
                    LogicJSONNumber yNumber = jsonObject.GetJSONNumber("y");

                    if (dataNumber != null && xNumber != null && yNumber != null)
                    {
                        LogicGameObjectData data = (LogicGameObjectData) LogicDataTables.GetDataById(dataNumber.GetIntValue());

                        if (data != null)
                        {
                            LogicGameObject gameObject = LogicGameObjectFactory.CreateGameObject(data, level, villageType);

                            if (gameObjectType == LogicGameObjectType.BUILDING)
                            {
                                ((LogicBuilding) gameObject).StartConstructing(true);
                            }

                            if (lockedBoolean != null && lockedBoolean.IsTrue())
                            {
                                ((LogicBuilding) gameObject).Lock();
                            }

                            gameObject.Load(jsonObject);
                            gameObjectManager.AddGameObject(gameObject, -1);

                            if (lvlNumber != null)
                            {
                                LogicDebugUtil.SetBuildingUpgradeLevel(level, gameObject.GetGlobalID(), lvlNumber.GetIntValue(), villageType);
                            }
                        }
                    }
                }
            }
        }

        public static void SetBuildingUpgradeLevel(LogicLevel level, int gameObjectId, int upgLevel, int villageType)
        {
            LogicGameObject gameObject = level.GetGameObjectManagerAt(villageType).GetGameObjectByID(gameObjectId);

            if (gameObject != null)
            {
                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                {
                    LogicBuilding building = (LogicBuilding) gameObject;
                    LogicBuildingData buildingData = building.GetBuildingData();

                    if (building.GetGearLevel() > 0 || building.IsGearing())
                    {
                        if (buildingData.GetMinUpgradeLevelForGearUp() > upgLevel)
                        {
                            Debugger.HudPrint("Can't downgrade geared up building below gear up limit!");
                            upgLevel = buildingData.GetMinUpgradeLevelForGearUp();
                        }
                    }

                    if (buildingData.IsTownHall())
                    {
                        level.GetPlayerAvatar().SetTownHallLevel(upgLevel);
                    }

                    building.SetUpgradeLevel(LogicMath.Max(upgLevel - 1, 0));
                    building.FinishConstruction(false, true);
                    building.SetUpgradeLevel(upgLevel);

                    if (building.GetListener() != null)
                        building.GetListener().RefreshState();
                    if (buildingData.IsTownHall() || buildingData.IsTownHallVillage2())
                        level.RefreshNewShopUnlocks(buildingData.GetVillageType());
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                {
                    LogicTrap trap = (LogicTrap) gameObject;

                    trap.SetUpgradeLevel(LogicMath.Max(upgLevel - 1, 0));
                    trap.FinishConstruction(false);
                    trap.SetUpgradeLevel(upgLevel);
                    trap.RepairTrap();

                    if (trap.GetListener() != null)
                    {
                        trap.GetListener().RefreshState();
                    }
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.VILLAGE_OBJECT)
                {
                    LogicVillageObject villageObject = (LogicVillageObject) gameObject;

                    villageObject.SetUpgradeLevel(LogicMath.Max(upgLevel - 1, 0));
                    villageObject.SetUpgradeLevel(upgLevel);

                    if (villageObject.GetListener() != null)
                    {
                        villageObject.GetListener().RefreshState();
                    }
                }
            }
        }

        public static void AddDebugTroopsPreset(LogicLevel level, int townHallLevel, LogicClientAvatar playerAvatar)
        {
            if (playerAvatar != null)
            {
                LogicDataTable characterTable = LogicDataTables.GetTable(LogicDataType.CHARACTER);
                LogicDataTable spellTable = LogicDataTables.GetTable(LogicDataType.SPELL);
                LogicBuildingData laboratoryData = LogicDataTables.GetBuildingByName("Laboratory", null);

                int laboratoryLevel = laboratoryData.GetMaxUpgradeLevelForTownHallLevel(townHallLevel);
                int totalHousing = LogicDebugUtil.GetTotalCharacterMaxHousing(townHallLevel, true) / 5;

                for (int i = 0; i < characterTable.GetItemCount(); i++)
                {
                    playerAvatar.SetUnitCount((LogicCharacterData) characterTable.GetItemAt(i), 0);
                }

                for (int i = 0; i < spellTable.GetItemCount(); i++)
                {
                    playerAvatar.SetUnitCount((LogicSpellData) spellTable.GetItemAt(i), 0);
                }

                for (int i = 0; i < 7; i++)
                {
                    if (i != 2 && i != 5)
                    {
                        LogicCharacterData characterData = (LogicCharacterData) characterTable.GetItemAt(i);

                        if (characterData.GetVillageType() == 0)
                        {
                            int upgradeLevel = 0;

                            for (int j = characterData.GetUpgradeLevelCount(); j >= 2; j--)
                            {
                                int requiredLaboratoryLevel = characterData.GetRequiredLaboratoryLevel(j - 1);

                                if (laboratoryLevel >= requiredLaboratoryLevel)
                                {
                                    upgradeLevel = j - 1;
                                    break;
                                }
                            }

                            playerAvatar.SetUnitCount(characterData, totalHousing / characterData.GetHousingSpace());
                            playerAvatar.SetUnitUpgradeLevel(characterData, upgradeLevel);
                        }
                    }
                }
            }
            else
            {
                Debugger.Warning("addDebugTroopsPreset: pAvatar is NULL");
            }
        }

        public static int GetTotalCharacterMaxHousing(int townHallLevel, bool includeAllianceCastle)
        {
            LogicTownhallLevelData townhallLevelData = LogicDataTables.GetTownHallLevel(townHallLevel);
            LogicDataTable table = LogicDataTables.GetTable(LogicDataType.BUILDING);

            int housingSpace = 0;

            for (int i = 0; i < table.GetItemCount(); i++)
            {
                LogicBuildingData buildingData = (LogicBuildingData) table.GetItemAt(i);

                if (includeAllianceCastle || buildingData != LogicDataTables.GetAllianceCastleData())
                {
                    int unlockedBuildingCount = townhallLevelData.GetUnlockedBuildingCount(buildingData);

                    if (unlockedBuildingCount > 0 && !buildingData.IsForgesSpells())
                        housingSpace += unlockedBuildingCount * buildingData.GetUnitStorageCapacity(buildingData.GetMaxUpgradeLevelForTownHallLevel(townHallLevel));
                }
            }

            return housingSpace;
        }
    }
}