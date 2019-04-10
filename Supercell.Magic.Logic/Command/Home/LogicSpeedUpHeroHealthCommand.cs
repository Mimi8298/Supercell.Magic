namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSpeedUpHeroHealthCommand : LogicCommand
    {
        private int m_gameObjectId;
        private int m_villageType;

        public LogicSpeedUpHeroHealthCommand()
        {
            // LogicSpeedUpHeroUpgradeCommand.
        }

        public LogicSpeedUpHeroHealthCommand(int gameObjectId, int villageType)
        {
            this.m_gameObjectId = gameObjectId;
            this.m_villageType = villageType;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_villageType = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_villageType);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SPEED_UP_HERO_HEALTH;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = this.m_villageType <= 1
                ? level.GetGameObjectManagerAt(this.m_villageType).GetGameObjectByID(this.m_gameObjectId)
                : level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;
                LogicHeroBaseComponent heroBaseComponent = building.GetHeroBaseComponent();

                if (heroBaseComponent != null)
                {
                    return heroBaseComponent.SpeedUpHealth() ? 0 : -2;
                }
            }

            return -1;
        }
    }
}