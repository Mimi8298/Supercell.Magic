namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicUpgradeUnitCommand : LogicCommand
    {
        private LogicCombatItemData m_unitData;

        private LogicDataType m_unitType;
        private int m_gameObjectId;

        public LogicUpgradeUnitCommand()
        {
            // LogicUpgradeUnitCommand.
        }

        public LogicUpgradeUnitCommand(LogicCombatItemData combatItemData, int gameObjectId)
        {
            this.m_unitData = combatItemData;
            this.m_gameObjectId = gameObjectId;
            this.m_unitType = this.m_unitData.GetDataType();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_unitType = (LogicDataType) stream.ReadInt();
            this.m_unitData = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream, this.m_unitType != 0 ? LogicDataType.SPELL : LogicDataType.CHARACTER);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt((int) this.m_unitType);
            ByteStreamHelper.WriteDataReference(encoder, this.m_unitData);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.UPGRADE_UNIT;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_gameObjectId = 0;
            this.m_unitType = 0;
            this.m_unitData = null;
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;

                if (this.m_unitData != null)
                {
                    LogicUnitUpgradeComponent unitUpgradeComponent = building.GetUnitUpgradeComponent();

                    if (unitUpgradeComponent != null && unitUpgradeComponent.CanStartUpgrading(this.m_unitData))
                    {
                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                        int upgradeLevel = playerAvatar.GetUnitUpgradeLevel(this.m_unitData);
                        int upgradeCost = this.m_unitData.GetUpgradeCost(upgradeLevel);
                        LogicResourceData upgradeResourceData = this.m_unitData.GetUpgradeResource(upgradeLevel);

                        if (playerAvatar.HasEnoughResources(upgradeResourceData, upgradeCost, true, this, false))
                        {
                            playerAvatar.CommodityCountChangeHelper(0, upgradeResourceData, -upgradeCost);
                            unitUpgradeComponent.StartUpgrading(this.m_unitData);

                            return 0;
                        }
                    }
                }
            }

            return -1;
        }
    }
}