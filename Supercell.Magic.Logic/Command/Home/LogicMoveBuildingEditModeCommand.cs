namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicMoveBuildingEditModeCommand : LogicCommand
    {
        private int m_gameObjectId;
        private int m_layoutId;
        private int m_x;
        private int m_y;

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_gameObjectId = stream.ReadInt();
            this.m_layoutId = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_layoutId);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.MOVE_BUILDING_EDIT_MODE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_layoutId != 6)
            {
                if (this.m_layoutId != 7)
                {
                    LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

                    if (gameObject != null)
                    {
                        LogicGameObjectType gameObjectType = gameObject.GetGameObjectType();

                        if (gameObjectType == LogicGameObjectType.BUILDING ||
                            gameObjectType == LogicGameObjectType.TRAP ||
                            gameObjectType == LogicGameObjectType.DECO)
                        {
                            LogicRect playArea = level.GetPlayArea();

                            if (playArea.IsInside(this.m_x, this.m_y) && playArea.IsInside(this.m_x + gameObject.GetWidthInTiles(), this.m_y + gameObject.GetHeightInTiles()) ||
                                this.m_x == -1 ||
                                this.m_y == -1)
                            {
                                if (gameObjectType == LogicGameObjectType.BUILDING)
                                {
                                    LogicBuilding building = (LogicBuilding) gameObject;

                                    if (building.GetWallIndex() != 0)
                                    {
                                        return -23;
                                    }
                                }

                                gameObject.SetPositionLayoutXY(this.m_x, this.m_y, this.m_layoutId, true);

                                LogicGlobals globals = LogicDataTables.GetGlobals();

                                if (!globals.NoCooldownFromMoveEditModeActive())
                                {
                                    if (level.GetActiveLayout(level.GetVillageType()) == this.m_layoutId)
                                    {
                                        LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                                        if (homeOwnerAvatar.GetExpLevel() >= globals.GetChallengeBaseCooldownEnabledTownHall())
                                        {
                                            level.SetLayoutCooldownSecs(this.m_layoutId, globals.GetChallengeBaseSaveCooldown());
                                        }
                                    }
                                }

                                return 0;
                            }

                            return -2; // EditModeOutsideMap
                        }

                        return -1;
                    }

                    return -3;
                }

                return -6;
            }

            return -5;
        }
    }
}