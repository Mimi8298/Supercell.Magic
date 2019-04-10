namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Unit;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicResumeBoostTrainingCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicResumeBoostTrainingCommand()
        {
            // LogicResumeBoostTrainingCommand.
        }

        public LogicResumeBoostTrainingCommand(int gameObjectId)
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
            return LogicCommandType.RESUME_BOOST_TRAINING;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (LogicDataTables.GetGlobals().UseNewTraining())
            {
                LogicUnitProduction unitProduction = this.m_gameObjectId == -2
                    ? level.GetGameObjectManager().GetUnitProduction()
                    : this.m_gameObjectId == -1
                        ? level.GetGameObjectManager().GetSpellProduction()
                        : null;

                if (unitProduction != null)
                {
                    unitProduction.SetBoostPause(false);
                    this.UpdateProductionHouseListeners(level);
                }

                return 0;
            }

            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null &&
                gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING &&
                gameObject.IsBoostPaused())
            {
                LogicBuilding building = (LogicBuilding) gameObject;

                if (building.CanStopBoost())
                {
                    building.SetBoostPause(false);
                    building.GetListener().RefreshState();

                    return 0;
                }
            }

            return -1;
        }

        public void UpdateProductionHouseListeners(LogicLevel level)
        {
            LogicArrayList<LogicGameObject> gameObjects = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.BUILDING);

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];

                if (building.GetBuildingData().GetUnitProduction(0) > 0)
                {
                    building.GetListener().RefreshState();
                }
            }
        }
    }
}