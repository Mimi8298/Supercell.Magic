namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicLoadTurretCommand : LogicCommand
    {
        private readonly LogicArrayList<int> m_gameObjectIds;

        public LogicLoadTurretCommand()
        {
            this.m_gameObjectIds = new LogicArrayList<int>();
        }

        public override void Decode(ByteStream stream)
        {
            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                this.m_gameObjectIds.Add(stream.ReadInt());
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectIds.Size());

            for (int i = 0; i < this.m_gameObjectIds.Size(); i++)
            {
                encoder.WriteInt(this.m_gameObjectIds[i]);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.LOAD_TURRET;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_gameObjectIds.Size() > 0)
            {
                LogicResourceData ammoResourceData = null;
                int ammoCost = 0;

                int idx = 0;

                do
                {
                    LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectIds[idx]);

                    if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        LogicBuilding building = (LogicBuilding) gameObject;

                        if (building.GetData().GetVillageType() == level.GetVillageType())
                        {
                            LogicCombatComponent combatComponent = building.GetCombatComponent(false);

                            if (combatComponent != null && combatComponent.UseAmmo())
                            {
                                if (combatComponent.GetAmmoCount() < combatComponent.GetMaxAmmo() && !building.IsUpgrading())
                                {
                                    LogicBuildingData buildingData = building.GetBuildingData();

                                    ammoResourceData = buildingData.GetAmmoResourceData(0);
                                    ammoCost += buildingData.GetAmmoCost(building.GetUpgradeLevel(), combatComponent.GetMaxAmmo() - combatComponent.GetAmmoCount());
                                }
                            }
                        }
                        else
                        {
                            return -32;
                        }
                    }
                } while (++idx < this.m_gameObjectIds.Size());

                if (ammoResourceData != null && ammoCost > 0)
                {
                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                    if (playerAvatar.HasEnoughResources(ammoResourceData, ammoCost, true, this, false))
                    {
                        for (int i = 0; i < this.m_gameObjectIds.Size(); i++)
                        {
                            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectIds[i]);

                            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                            {
                                LogicBuilding building = (LogicBuilding) gameObject;
                                LogicCombatComponent combatComponent = building.GetCombatComponent(false);

                                if (combatComponent != null && combatComponent.UseAmmo())
                                {
                                    if (combatComponent.GetAmmoCount() < combatComponent.GetMaxAmmo())
                                    {
                                        int upgradeLevel = building.GetUpgradeLevel();
                                        LogicBuildingData buildingData = building.GetBuildingData();
                                        LogicResourceData loadAmmoResourceData = buildingData.GetAmmoResourceData(upgradeLevel);
                                        int loadAmmoCost = buildingData.GetAmmoCost(upgradeLevel, combatComponent.GetMaxAmmo() - combatComponent.GetAmmoCount());


                                        if (playerAvatar.HasEnoughResources(loadAmmoResourceData, loadAmmoCost, true, this, false))
                                        {
                                            playerAvatar.CommodityCountChangeHelper(0, loadAmmoResourceData, -loadAmmoCost);
                                            combatComponent.LoadAmmo();

                                            continue;
                                        }
                                    }
                                }
                            }

                            break;
                        }

                        return 0;
                    }

                    return -2;
                }
            }

            return -1;
        }
    }
}