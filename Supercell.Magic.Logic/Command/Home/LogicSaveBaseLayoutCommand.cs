namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicSaveBaseLayoutCommand : LogicCommand
    {
        private int m_layoutId;
        private int m_state;

        public override void Decode(ByteStream stream)
        {
            this.m_layoutId = stream.ReadInt();
            this.m_state = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_layoutId);
            encoder.WriteInt(this.m_state);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.SAVE_BASE_LAYOUT;
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
                    LogicGameObjectFilter filter = new LogicGameObjectFilter();
                    LogicArrayList<LogicGameObject> gameObjects = new LogicArrayList<LogicGameObject>(500);

                    int moved = 0;

                    filter.AddGameObjectType(LogicGameObjectType.BUILDING);
                    filter.AddGameObjectType(LogicGameObjectType.TRAP);
                    filter.AddGameObjectType(LogicGameObjectType.DECO);

                    level.GetGameObjectManager().GetGameObjects(gameObjects, filter);

                    for (int i = 0; i < gameObjects.Size(); i++)
                    {
                        LogicGameObject gameObject = gameObjects[i];
                        LogicVector2 position = gameObjects[i].GetPositionLayout(this.m_layoutId, true);

                        int x = position.m_x;
                        int y = position.m_y;

                        if (x != -1 && y != -1)
                        {
                            int tmp1 = x + gameObject.GetWidthInTiles();
                            int tmp2 = y + gameObject.GetHeightInTiles();

                            for (int j = 0; j < gameObjects.Size(); j++)
                            {
                                LogicGameObject tmp = gameObjects[j];

                                if (tmp != gameObject)
                                {
                                    LogicVector2 position2 = tmp.GetPositionLayout(this.m_layoutId, true);

                                    int x2 = position2.m_x;
                                    int y2 = position2.m_y;

                                    if (x2 != -1 && y2 != -1)
                                    {
                                        int tmp3 = x2 + tmp.GetWidthInTiles();
                                        int tmp4 = y2 + tmp.GetHeightInTiles();

                                        if (tmp1 > x2 && tmp2 > y2 && x < tmp3 && y < tmp4)
                                        {
                                            Debugger.Warning("LogicSaveBaseLayoutCommand: overlap");
                                            return -1;
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
                        LogicVector2 layoutPosition = gameObject.GetPositionLayout(this.m_layoutId, false);

                        if (gameObject.GetGameObjectType() != LogicGameObjectType.DECO)
                        {
                            if (layoutPosition.m_x != editModePosition.m_x || layoutPosition.m_y != editModePosition.m_y)
                            {
                                ++moved;
                            }
                        }

                        gameObject.SetPositionLayoutXY(editModePosition.m_x, editModePosition.m_y, this.m_layoutId, false);
                    }

                    filter.Destruct();

                    if (moved > 0)
                    {
                        LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                        if (homeOwnerAvatar.GetTownHallLevel() >= LogicDataTables.GetGlobals().GetChallengeBaseCooldownEnabledTownHall())
                        {
                            level.SetLayoutCooldownSecs(this.m_layoutId, LogicDataTables.GetGlobals().GetChallengeBaseSaveCooldown());
                        }
                    }

                    return 0;
                }

                return -11;
            }

            return 10;
        }
    }
}