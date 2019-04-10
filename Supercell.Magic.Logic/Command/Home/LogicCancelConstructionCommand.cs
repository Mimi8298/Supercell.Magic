namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicCancelConstructionCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicCancelConstructionCommand()
        {
            // LogicCancelConstructionCommand.
        }

        public LogicCancelConstructionCommand(int gameObjectId)
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
            return LogicCommandType.CANCEL_CONSTRUCTION;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null)
            {
                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                {
                    LogicBuilding building = (LogicBuilding) gameObject;

                    if (!LogicDataTables.GetGlobals().AllowCancelBuildingConstruction() &&
                        building.GetUpgradeLevel() == 0 &&
                        building.IsConstructing())
                    {
                        if (!building.IsUpgrading())
                        {
                            return -2;
                        }
                    }

                    if (building.IsConstructing())
                    {
                        building.GetListener().CancelNotification();
                        building.CancelConstruction();

                        return 0;
                    }
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.OBSTACLE)
                {
                    LogicObstacle obstacle = (LogicObstacle) gameObject;

                    if (obstacle.IsClearingOnGoing())
                    {
                        LogicObstacleData data = obstacle.GetObstacleData();
                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                        playerAvatar.CommodityCountChangeHelper(0, data.GetClearResourceData(), data.GetClearCost());
                        obstacle.CancelClearing();

                        return 0;
                    }
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                {
                    LogicTrap trap = (LogicTrap) gameObject;

                    if (trap.IsConstructing())
                    {
                        trap.GetListener().CancelNotification();
                        trap.CancelConstruction();

                        return 0;
                    }
                }
            }

            return -1;
        }
    }
}