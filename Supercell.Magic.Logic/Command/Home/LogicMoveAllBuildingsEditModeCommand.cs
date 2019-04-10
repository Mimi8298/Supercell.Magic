namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicMoveAllBuildingsEditModeCommand : LogicCommand
    {
        private int m_layoutId;
        private int m_x;
        private int m_y;

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_layoutId = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            encoder.WriteInt(this.m_layoutId);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.MOVE_ALL_BUILDINGS_EDIT_MODE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicRect playArea = level.GetPlayArea();
            LogicArrayList<LogicGameObject> gameObjects = new LogicArrayList<LogicGameObject>();
            LogicArrayList<LogicGameObject> staticObjects = new LogicArrayList<LogicGameObject>();
            LogicGameObjectFilter filter = new LogicGameObjectFilter();

            filter.AddGameObjectType(LogicGameObjectType.BUILDING);
            filter.AddGameObjectType(LogicGameObjectType.TRAP);
            filter.AddGameObjectType(LogicGameObjectType.DECO);

            LogicArrayList<LogicGameObject> buildings = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.BUILDING);
            LogicArrayList<LogicGameObject> obstacles = level.GetGameObjectManager().GetGameObjects(LogicGameObjectType.OBSTACLE);

            staticObjects.AddAll(obstacles);

            for (int i = 0; i < buildings.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) buildings[i];

                if (building.IsLocked())
                {
                    filter.AddIgnoreObject(building);
                    staticObjects.Add(building);
                }
            }

            level.GetGameObjectManager().GetGameObjects(gameObjects, filter);

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicGameObject gameObject = gameObjects[i];
                LogicVector2 editModePosition = gameObject.GetPositionLayout(this.m_layoutId, true);

                if (editModePosition.m_x != -1 && editModePosition.m_y != -1)
                {
                    int newX = editModePosition.m_x + this.m_x;
                    int newY = editModePosition.m_y + this.m_y;
                    int width = gameObject.GetWidthInTiles();
                    int height = gameObject.GetHeightInTiles();

                    if (newX < playArea.GetStartX() ||
                        newY < playArea.GetStartY() ||
                        newX + width > playArea.GetEndX() ||
                        newY + height > playArea.GetEndY())
                    {
                        return -1;
                    }

                    if (this.m_layoutId <= 3 && this.m_layoutId != 1)
                    {
                        for (int j = 0; j < staticObjects.Size(); j++)
                        {
                            LogicGameObject staticObject = staticObjects[j];

                            if (staticObject != gameObject)
                            {
                                int staticX = staticObject.GetTileX();
                                int staticY = staticObject.GetTileY();
                                int staticWidth = staticObject.GetWidthInTiles();
                                int staticHeight = staticObject.GetHeightInTiles();

                                if (staticX != -1 && staticY != -1)
                                {
                                    if (newX + width > staticX &&
                                        newY + height > staticY &&
                                        newX < staticX + staticWidth &&
                                        newY < staticY + staticHeight)
                                    {
                                        return -1;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicGameObject gameObject = gameObjects[i];
                LogicVector2 editModePosition = gameObject.GetPositionLayout(this.m_layoutId, true);

                if (editModePosition.m_x != -1 && editModePosition.m_y != -1)
                {
                    gameObject.SetPositionLayoutXY(editModePosition.m_x + this.m_x, editModePosition.m_y + this.m_y, this.m_layoutId, true);
                }
            }

            filter.Destruct();

            return 0;
        }
    }
}