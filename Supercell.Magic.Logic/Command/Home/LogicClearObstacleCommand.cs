namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicClearObstacleCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicClearObstacleCommand()
        {
            // LogicClearObstacleCommand.
        }

        public LogicClearObstacleCommand(int gameObjectId)
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
            return LogicCommandType.CLEAR_OBSTACLE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.OBSTACLE)
            {
                LogicObstacle obstacle = (LogicObstacle) gameObject;

                if (obstacle.GetObstacleData().GetVillageType() == level.GetVillageType())
                {
                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                    if (obstacle.CanStartClearing())
                    {
                        LogicObstacleData obstacleData = obstacle.GetObstacleData();

                        if (obstacle.GetVillageType() == 1)
                        {
                            int village2TownHallLevel = playerAvatar.GetVillage2TownHallLevel();

                            if (village2TownHallLevel < LogicDataTables.GetGlobals().GetMinVillage2TownHallLevelForDestructObstacle() &&
                                obstacleData.GetClearCost() > 0)
                            {
                                return 0;
                            }
                        }

                        LogicResourceData clearResourceData = obstacleData.GetClearResourceData();
                        int clearCost = obstacleData.GetClearCost();

                        if (playerAvatar.HasEnoughResources(clearResourceData, clearCost, true, this, false))
                        {
                            if (obstacleData.GetClearTime() == 0 || level.HasFreeWorkers(this, -1))
                            {
                                playerAvatar.CommodityCountChangeHelper(0, clearResourceData, -clearCost);
                                obstacle.StartClearing();

                                if (obstacle.IsTombstone())
                                {
                                    int tombGroup = obstacle.GetTombGroup();

                                    if (tombGroup != 2)
                                    {
                                        LogicArrayList<LogicGameObject> gameObjects = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.OBSTACLE);

                                        for (int i = 0; i < gameObjects.Size(); i++)
                                        {
                                            LogicObstacle go = (LogicObstacle) gameObjects[i];

                                            if (go.IsTombstone() && go.GetTombGroup() == tombGroup)
                                            {
                                                go.StartClearing();
                                            }
                                        }
                                    }
                                }
                            }

                            return 0;
                        }
                    }

                    return -1;
                }

                return -32;
            }

            return -1;
        }
    }
}