namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicRotateBuildingCommand : LogicCommand
    {
        private int m_gameObjectId;
        private int m_layout;
        private int m_baseLayout;

        private bool m_draftLayout;
        private bool m_baseDraftLayout;
        private bool m_updateListener;

        public LogicRotateBuildingCommand()
        {
            // LogicRotateBuildingCommand.
        }

        public LogicRotateBuildingCommand(int gameObjectId, int layout, bool draftLayout, bool updateListener, int baseLayout, bool baseDraftLayout)
        {
            this.m_gameObjectId = gameObjectId;
            this.m_layout = layout;
            this.m_draftLayout = draftLayout;
            this.m_updateListener = updateListener;
            this.m_baseLayout = baseLayout;
            this.m_baseDraftLayout = baseDraftLayout;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_layout = stream.ReadInt();
            this.m_draftLayout = stream.ReadBoolean();
            this.m_updateListener = stream.ReadBoolean();
            this.m_baseLayout = stream.ReadInt();
            this.m_baseDraftLayout = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_layout);
            encoder.WriteBoolean(this.m_draftLayout);
            encoder.WriteBoolean(this.m_updateListener);
            encoder.WriteInt(this.m_baseLayout);
            encoder.WriteBoolean(this.m_baseDraftLayout);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.ROTATE_BUILDING;
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
                    LogicBuildingData buildingData = building.GetBuildingData();
                    LogicCombatComponent combatComponent = building.GetCombatComponent(false);

                    if (combatComponent != null && combatComponent.GetAttackerItemData().GetTargetingConeAngle() != 0)
                    {
                        if (this.m_baseLayout == -1)
                        {
                            combatComponent.ToggleAimAngle(buildingData.GetAimRotateStep(), this.m_layout, this.m_draftLayout);
                        }
                        else
                        {
                            int draftAngle = combatComponent.GetAimAngle(this.m_baseLayout, this.m_baseDraftLayout);
                            int currentAngle = combatComponent.GetAimAngle(this.m_layout, this.m_draftLayout);

                            combatComponent.ToggleAimAngle(draftAngle - currentAngle, this.m_layout, this.m_draftLayout);
                        }

                        return 0;
                    }
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                {
                    LogicTrap trap = (LogicTrap) gameObject;

                    if (trap.GetTrapData().GetDirectionCount() > 0)
                    {
                        if (this.m_baseLayout == -1)
                        {
                            trap.ToggleDirection(this.m_layout, this.m_draftLayout);
                        }
                        else
                        {
                            trap.SetDirection(this.m_layout, this.m_draftLayout, trap.GetDirection(this.m_baseLayout, this.m_baseDraftLayout));
                        }

                        return 0;
                    }

                    return -21;
                }
            }

            return -1;
        }
    }
}