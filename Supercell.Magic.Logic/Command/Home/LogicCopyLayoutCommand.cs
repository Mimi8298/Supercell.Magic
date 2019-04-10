namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicCopyLayoutCommand : LogicCommand
    {
        private int m_inputLayoutId;
        private int m_outputLayoutId;

        public LogicCopyLayoutCommand()
        {
            this.m_inputLayoutId = -1;
            this.m_outputLayoutId = -1;
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);

            this.m_inputLayoutId = stream.ReadInt();
            this.m_outputLayoutId = stream.ReadInt();

            this.m_inputLayoutId = LogicMath.Clamp(this.m_inputLayoutId, 0, 7);
            this.m_outputLayoutId = LogicMath.Clamp(this.m_outputLayoutId, 0, 7);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);

            encoder.WriteInt(this.m_inputLayoutId);
            encoder.WriteInt(this.m_outputLayoutId);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.COPY_LAYOUT;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_inputLayoutId != 6)
            {
                if (this.m_outputLayoutId != 6)
                {
                    if (this.m_inputLayoutId != 7)
                    {
                        if (this.m_outputLayoutId != 7)
                        {
                            int townHallLevel = level.GetTownHallLevel(level.GetVillageType());

                            if (townHallLevel >= level.GetRequiredTownHallLevelForLayout(this.m_inputLayoutId, -1) &&
                                townHallLevel >= level.GetRequiredTownHallLevelForLayout(this.m_outputLayoutId, -1))
                            {
                                LogicGameObjectFilter filter = new LogicGameObjectFilter();
                                LogicArrayList<LogicGameObject> gameObjects = new LogicArrayList<LogicGameObject>(500);

                                filter.AddGameObjectType(LogicGameObjectType.BUILDING);
                                filter.AddGameObjectType(LogicGameObjectType.TRAP);
                                filter.AddGameObjectType(LogicGameObjectType.DECO);

                                level.GetGameObjectManager().GetGameObjects(gameObjects, filter);

                                if (this.m_outputLayoutId == level.GetActiveLayout())
                                {
                                    LogicMoveMultipleBuildingsCommand moveMultipleBuildingsCommand = new LogicMoveMultipleBuildingsCommand();

                                    for (int i = 0; i < gameObjects.Size(); i++)
                                    {
                                        LogicGameObject gameObject = gameObjects[i];
                                        LogicVector2 position = gameObject.GetPositionLayout(this.m_inputLayoutId, false);

                                        moveMultipleBuildingsCommand.AddNewMove(gameObject.GetGlobalID(), position.m_x, position.m_y);
                                    }

                                    int result = moveMultipleBuildingsCommand.Execute(level);

                                    moveMultipleBuildingsCommand.Destruct();

                                    if (result != 0)
                                    {
                                        filter.Destruct();
                                        return -2;
                                    }
                                }

                                for (int i = 0; i < gameObjects.Size(); i++)
                                {
                                    LogicGameObject gameObject = gameObjects[i];
                                    LogicVector2 layoutPosition = gameObject.GetPositionLayout(this.m_inputLayoutId, false);
                                    LogicVector2 editModePosition = gameObject.GetPositionLayout(this.m_inputLayoutId, true);

                                    gameObject.SetPositionLayoutXY(layoutPosition.m_x, layoutPosition.m_y, this.m_outputLayoutId, false);
                                    gameObject.SetPositionLayoutXY(editModePosition.m_x, editModePosition.m_y, this.m_outputLayoutId, true);

                                    if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                                    {
                                        LogicCombatComponent combatComponent = gameObject.GetCombatComponent(false);

                                        if (combatComponent != null)
                                        {
                                            if (combatComponent.HasAltAttackMode())
                                            {
                                                if (combatComponent.UseAltAttackMode(this.m_inputLayoutId, false) ^ combatComponent.UseAltAttackMode(this.m_outputLayoutId, false))
                                                {
                                                    combatComponent.ToggleAttackMode(this.m_outputLayoutId, false);
                                                }

                                                if (combatComponent.UseAltAttackMode(this.m_inputLayoutId, true) ^ combatComponent.UseAltAttackMode(this.m_outputLayoutId, true))
                                                {
                                                    combatComponent.ToggleAttackMode(this.m_outputLayoutId, true);
                                                }
                                            }

                                            if (combatComponent.GetAttackerItemData().GetTargetingConeAngle() != 0)
                                            {
                                                int aimAngle1 = combatComponent.GetAimAngle(this.m_inputLayoutId, false);
                                                int aimAngle2 = combatComponent.GetAimAngle(this.m_outputLayoutId, false);

                                                if (aimAngle1 != aimAngle2)
                                                {
                                                    combatComponent.ToggleAimAngle(aimAngle1 - aimAngle2, this.m_outputLayoutId, false);
                                                }
                                            }
                                        }
                                    }
                                    else if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                                    {
                                        LogicTrap trap = (LogicTrap) gameObject;

                                        if (trap.HasAirMode())
                                        {
                                            if (trap.IsAirMode(this.m_inputLayoutId, false) ^ trap.IsAirMode(this.m_outputLayoutId, false))
                                            {
                                                trap.ToggleAirMode(this.m_outputLayoutId, false);
                                            }

                                            if (trap.IsAirMode(this.m_inputLayoutId, true) ^ trap.IsAirMode(this.m_outputLayoutId, true))
                                            {
                                                trap.ToggleAirMode(this.m_outputLayoutId, true);
                                            }
                                        }
                                    }
                                }

                                filter.Destruct();
                                level.SetLayoutState(this.m_outputLayoutId, level.GetVillageType(), level.GetLayoutState(this.m_inputLayoutId, level.GetVillageType()));

                                LogicAvatar homeOwnerAvatar = level.GetHomeOwnerAvatar();

                                if (homeOwnerAvatar.GetTownHallLevel() >= LogicDataTables.GetGlobals().GetChallengeBaseCooldownEnabledTownHall())
                                {
                                    level.SetLayoutCooldownSecs(this.m_outputLayoutId, level.GetLayoutCooldown(this.m_inputLayoutId) / 15);
                                }

                                return 0;
                            }

                            return -1;
                        }

                        return -8;
                    }

                    return -7;
                }

                return -6;
            }

            return -5;
        }
    }
}