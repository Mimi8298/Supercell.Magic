namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSpeedUpBoostCooldownCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicSpeedUpBoostCooldownCommand()
        {
            // LogicSpeedUpBoostCooldownCommand.
        }

        public LogicSpeedUpBoostCooldownCommand(int gameObjectId)
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
            return LogicCommandType.SPEED_UP_BOOST_COOLDOWN;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;

                bool success = building.SpeedUpBoostCooldown();

                if (building.GetBuildingData().IsClockTower())
                {
                    building.GetListener().RefreshState();
                }

                return success ? 0 : -2;
            }

            return -1;
        }
    }
}