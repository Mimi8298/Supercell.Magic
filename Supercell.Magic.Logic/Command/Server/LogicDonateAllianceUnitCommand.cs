namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public class LogicDonateAllianceUnitCommand : LogicServerCommand
    {
        private LogicLong m_streamId;
        private LogicCombatItemData m_unitData;

        private bool m_quickDonate;

        public void SetData(LogicCombatItemData data, LogicLong streamId, bool quickDonate)
        {
            this.m_unitData = data;
            this.m_streamId = streamId;
            this.m_quickDonate = quickDonate;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_streamId = null;
            this.m_unitData = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_streamId = stream.ReadLong();
            this.m_unitData = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream, stream.ReadInt() != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);
            this.m_quickDonate = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteLong(this.m_streamId);
            encoder.WriteInt(this.m_unitData.GetCombatItemType());
            ByteStreamHelper.WriteDataReference(encoder, this.m_unitData);
            encoder.WriteBoolean(this.m_quickDonate);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (this.m_unitData != null)
                {
                    if (this.m_unitData.GetVillageType() == 0)
                    {
                        if (!this.m_unitData.IsDonationDisabled())
                        {
                            bool removeFromTrainQueue = false;
                            int upgLevel = playerAvatar.GetUnitUpgradeLevel(this.m_unitData);

                            LogicUnitProductionComponent unitProductionComponent = null;

                            if (this.m_quickDonate)
                            {
                                int cost = this.m_unitData.GetDonateCost();

                                if (!playerAvatar.HasEnoughDiamonds(cost, true, level) ||
                                    !this.m_unitData.IsUnlockedForProductionHouseLevel(
                                        level.GetGameObjectManagerAt(0).GetHighestBuildingLevel(this.m_unitData.GetProductionHouseData())) ||
                                    !LogicDataTables.GetGlobals().EnableQuickDonate())
                                {
                                    playerAvatar.GetChangeListener().AllianceUnitDonateFailed(this.m_unitData, upgLevel, this.m_streamId, this.m_quickDonate);
                                    return 0;
                                }
                            }
                            else
                            {
                                LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(0);

                                if (LogicDataTables.GetGlobals().UseNewTraining())
                                {
                                    LogicUnitProduction unitProduction = gameObjectManager.GetUnitProduction();
                                    LogicUnitProduction spellProduction = gameObjectManager.GetSpellProduction();

                                    if (unitProduction.GetWaitingForSpaceUnitCount(this.m_unitData) > 0)
                                    {
                                        if (unitProduction.GetUnitProductionType() == this.m_unitData.GetDataType())
                                        {
                                            removeFromTrainQueue = true;
                                        }
                                    }

                                    if (spellProduction.GetWaitingForSpaceUnitCount(this.m_unitData) > 0)
                                    {
                                        if (spellProduction.GetUnitProductionType() == this.m_unitData.GetDataType())
                                        {
                                            removeFromTrainQueue = true;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0, c = gameObjectManager.GetNumGameObjects(); i < c; i++)
                                    {
                                        LogicGameObject gameObject = gameObjectManager.GetGameObjectByIndex(i);

                                        if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                        {
                                            LogicBuilding building = (LogicBuilding) gameObject;
                                            LogicUnitProductionComponent component = building.GetUnitProductionComponent();

                                            if (component != null)
                                            {
                                                unitProductionComponent = component;

                                                if (component.ContainsUnit(this.m_unitData))
                                                {
                                                    if (component.GetRemainingSeconds() == 0 && component.GetCurrentlyTrainedUnit() == this.m_unitData)
                                                    {
                                                        removeFromTrainQueue = true;
                                                    }
                                                }
                                                else
                                                {
                                                    unitProductionComponent = null;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!removeFromTrainQueue)
                                {
                                    if (playerAvatar.GetUnitCount(this.m_unitData) <= 0)
                                    {
                                        playerAvatar.GetChangeListener().AllianceUnitDonateFailed(this.m_unitData, upgLevel, this.m_streamId, this.m_quickDonate);
                                        return 0;
                                    }
                                }
                            }

                            if (this.m_unitData.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                            {
                                playerAvatar.XpGainHelper(this.m_unitData.GetHousingSpace() * LogicDataTables.GetGlobals().GetDarkSpellDonationXP());
                                level.GetAchievementManager().AlianceSpellDonated((LogicSpellData) this.m_unitData);
                            }
                            else
                            {
                                playerAvatar.XpGainHelper(((LogicCharacterData) this.m_unitData).GetDonateXP());
                                level.GetAchievementManager().AlianceUnitDonated((LogicCharacterData) this.m_unitData);
                            }

                            playerAvatar.GetChangeListener().AllianceUnitDonateOk(this.m_unitData, upgLevel, this.m_streamId, this.m_quickDonate);

                            if (this.m_quickDonate)
                            {
                                int cost = this.m_unitData.GetDonateCost();

                                playerAvatar.UseDiamonds(cost);
                                playerAvatar.GetChangeListener().DiamondPurchaseMade(12, this.m_unitData.GetGlobalID(), 0, cost, level.GetVillageType());

                                if ((level.GetState() == 1 || level.GetState() == 3) && this.m_unitData.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                                {
                                    LogicGameObject gameObject = level.GetGameObjectManagerAt(0).GetHighestBuilding(this.m_unitData.GetProductionHouseData());

                                    if (gameObject != null)
                                    {
                                        // Listener.
                                    }
                                }

                                return 0;
                            }

                            if (!removeFromTrainQueue)
                            {
                                playerAvatar.CommodityCountChangeHelper(0, this.m_unitData, -1);
                            }

                            LogicResourceData trainingResource = this.m_unitData.GetTrainingResource();

                            int trainingCost = level.GetGameMode().GetCalendar().GetTrainingCost(this.m_unitData, upgLevel);
                            int refund = playerAvatar.GetTroopDonationRefund() * trainingCost / 100;

                            playerAvatar.CommodityCountChangeHelper(0, trainingResource, LogicMath.Max(refund, 0));

                            if (level.GetState() == 1 || level.GetState() == 3)
                            {
                                if (removeFromTrainQueue)
                                {
                                    if (LogicDataTables.GetGlobals().UseNewTraining())
                                    {
                                        LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(0);
                                        LogicUnitProduction unitProduction = this.m_unitData.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER
                                            ? gameObjectManager.GetSpellProduction()
                                            : gameObjectManager.GetUnitProduction();

                                        unitProduction.RemoveTrainedUnit(this.m_unitData);
                                    }

                                    if (this.m_unitData.GetCombatItemType() == LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER)
                                    {
                                        LogicBuilding productionHouse = null;

                                        if (unitProductionComponent != null)
                                        {
                                            productionHouse = (LogicBuilding) unitProductionComponent.GetParent();
                                        }
                                        else
                                        {
                                            if (LogicDataTables.GetGlobals().UseTroopWalksOutFromTraining())
                                            {
                                                LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(0);
                                                int gameObjectCount = gameObjectManager.GetNumGameObjects();

                                                for (int i = 0; i < gameObjectCount; i++)
                                                {
                                                    LogicGameObject gameObject = gameObjectManager.GetGameObjectByIndex(i);

                                                    if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                                    {
                                                        LogicBuilding tmpBuilding = (LogicBuilding) gameObject;
                                                        LogicUnitProductionComponent tmpComponent = tmpBuilding.GetUnitProductionComponent();

                                                        if (tmpComponent != null)
                                                        {
                                                            if (tmpComponent.GetProductionType() == this.m_unitData.GetCombatItemType())
                                                            {
                                                                if (tmpBuilding.GetBuildingData().GetProducesUnitsOfType() == this.m_unitData.GetUnitOfType() &&
                                                                    !tmpBuilding.IsUpgrading() &&
                                                                    !tmpBuilding.IsConstructing())
                                                                {
                                                                    if (this.m_unitData.IsUnlockedForProductionHouseLevel(tmpBuilding.GetUpgradeLevel()))
                                                                    {
                                                                        if (productionHouse != null)
                                                                        {
                                                                            int seed = playerAvatar.GetExpPoints();

                                                                            if (tmpBuilding.Rand(seed) % 1000 > 750)
                                                                            {
                                                                                productionHouse = tmpBuilding;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            productionHouse = tmpBuilding;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (productionHouse != null)
                                        {
                                            // TODO: Implement listener.
                                        }
                                    }
                                }
                                else
                                {
                                    LogicArrayList<LogicComponent> components = level.GetComponentManagerAt(0).GetComponents(LogicComponentType.UNIT_STORAGE);

                                    for (int i = 0; i < components.Size(); i++)
                                    {
                                        LogicUnitStorageComponent unitStorageComponent = (LogicUnitStorageComponent) components[i];
                                        int idx = unitStorageComponent.GetUnitTypeIndex(this.m_unitData);

                                        if (idx != -1)
                                        {
                                            if (unitStorageComponent.GetUnitCount(idx) > 0)
                                            {
                                                unitStorageComponent.RemoveUnits(this.m_unitData, 1);

                                                if (LogicDataTables.GetGlobals().UseNewTraining())
                                                {
                                                    LogicGameObjectManager gameObjectManager = level.GetGameObjectManagerAt(0);
                                                    LogicUnitProduction unitProduction = this.m_unitData.GetCombatItemType() != LogicCombatItemData.COMBAT_ITEM_TYPE_CHARACTER
                                                        ? gameObjectManager.GetSpellProduction()
                                                        : gameObjectManager.GetUnitProduction();

                                                    unitProduction.MergeSlots();
                                                    unitProduction.UnitRemoved();
                                                }

                                                break;
                                            }
                                        }
                                    }
                                }

                                // TODO: Finish this.
                            }

                            return 0;
                        }

                        return -91;
                    }

                    return -45;
                }
            }

            return 0;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.DONATE_ALLIANCE_UNIT;
        }
    }
}