namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSpeedUpUpgradeUnitCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicSpeedUpUpgradeUnitCommand()
        {
            // LogicSpeedUpUpgradeUnitCommand.
        }

        public LogicSpeedUpUpgradeUnitCommand(int gameObjectId)
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
            return LogicCommandType.SPEED_UP_UPGRADE_UNIT;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_gameObjectId = 0;
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;
                LogicUnitUpgradeComponent unitUpgradeComponent = building.GetUnitUpgradeComponent();

                if (unitUpgradeComponent.GetCurrentlyUpgradedUnit() != null)
                {
                    return unitUpgradeComponent.SpeedUp() ? 0 : -2;
                }
            }

            return -1;
        }
    }
}