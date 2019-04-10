namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicMoveBuildingCommand : LogicCommand
    {
        private int m_x;
        private int m_y;
        private int m_gameObjectId;

        public LogicMoveBuildingCommand()
        {
            // LogicMoveBuildingCommand.
        }

        public LogicMoveBuildingCommand(int gameObjectId, int x, int y)
        {
            this.m_x = x;
            this.m_y = y;
            this.m_gameObjectId = gameObjectId;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_gameObjectId = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            encoder.WriteInt(this.m_gameObjectId);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.MOVE_BUILDING;
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
                LogicGameObjectType gameObjectType = gameObject.GetGameObjectType();

                if (gameObjectType == LogicGameObjectType.BUILDING || gameObjectType == LogicGameObjectType.TRAP ||
                    gameObjectType == LogicGameObjectType.DECO)
                {
                    if (gameObjectType != LogicGameObjectType.BUILDING || ((LogicBuildingData) gameObject.GetData()).GetVillageType() == level.GetVillageType())
                    {
                        if (gameObjectType == LogicGameObjectType.BUILDING)
                        {
                            if (((LogicBuilding) gameObject).GetWallIndex() != 0)
                            {
                                return -21;
                            }
                        }

                        int x = gameObject.GetTileX();
                        int y = gameObject.GetTileY();
                        int width = gameObject.GetWidthInTiles();
                        int height = gameObject.GetHeightInTiles();

                        for (int i = 0; i < width; i++)
                        {
                            for (int j = 0; j < height; j++)
                            {
                                LogicObstacle tallGrass = level.GetTileMap().GetTile(this.m_x + i, this.m_y + j).GetTallGrass();

                                if (tallGrass != null)
                                {
                                    level.GetGameObjectManager().RemoveGameObject(tallGrass);
                                }
                            }
                        }

                        if (level.IsValidPlaceForBuilding(this.m_x, this.m_y, width, height, gameObject))
                        {
                            gameObject.SetPositionXY(this.m_x << 9, this.m_y << 9);

                            if (this.m_x != x || this.m_y != y)
                            {
                                LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                                if (homeOwnerAvatar != null)
                                {
                                    if (homeOwnerAvatar.GetTownHallLevel() >= LogicDataTables.GetGlobals().GetChallengeBaseCooldownEnabledTownHall() &&
                                        gameObject.GetGameObjectType() != LogicGameObjectType.DECO)
                                    {
                                        level.SetLayoutCooldownSecs(level.GetActiveLayout(level.GetVillageType()), LogicDataTables.GetGlobals().GetChallengeBaseSaveCooldown());
                                    }
                                }
                            }

                            return 0;
                        }

                        return -3;
                    }

                    return -32;
                }

                return -1;
            }

            return -2;
        }
    }
}