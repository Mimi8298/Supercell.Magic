namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicUpgradeHeroCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicUpgradeHeroCommand()
        {
            // LogicUpgradeHeroCommand.
        }

        public LogicUpgradeHeroCommand(int gameObjectId)
        {
            this.m_gameObjectId = gameObjectId;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.UPGRADE_HERO;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicBuilding building = (LogicBuilding) level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (!building.IsLocked())
            {
                if (building.GetBuildingData().GetVillageType() == level.GetVillageType())
                {
                    LogicHeroBaseComponent heroBaseComponent = building.GetHeroBaseComponent();

                    if (heroBaseComponent != null && heroBaseComponent.CanStartUpgrading(true))
                    {
                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                        LogicHeroData heroData = building.GetBuildingData().GetHeroData();
                        int upgLevel = playerAvatar.GetUnitUpgradeLevel(heroData);
                        int upgradeCost = heroData.GetUpgradeCost(upgLevel);
                        LogicResourceData upgradeResourceData = heroData.GetUpgradeResource(upgLevel);

                        if (playerAvatar.HasEnoughResources(upgradeResourceData, upgradeCost, true, this, false))
                        {
                            if (level.HasFreeWorkers(this, -1))
                            {
                                playerAvatar.CommodityCountChangeHelper(0, upgradeResourceData, -upgradeCost);
                                heroBaseComponent.StartUpgrading();

                                return 0;
                            }
                        }
                    }

                    return -1;
                }

                return -32;
            }

            return -23;
        }
    }
}