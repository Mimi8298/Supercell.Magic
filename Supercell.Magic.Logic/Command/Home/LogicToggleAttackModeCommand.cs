namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicToggleAttackModeCommand : LogicCommand
    {
        private int m_gameObjectId;
        private int m_layout;

        private bool m_draftMode;
        private bool m_updateListener;

        public LogicToggleAttackModeCommand()
        {
            // LogicToggleAttackModeCommand.
        }

        public LogicToggleAttackModeCommand(int gameObjectId, int layout, bool draftMode, bool updateListener)
        {
            this.m_gameObjectId = gameObjectId;
            this.m_layout = layout;
            this.m_draftMode = draftMode;
            this.m_updateListener = updateListener;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_layout = stream.ReadInt();
            this.m_draftMode = stream.ReadBoolean();
            this.m_updateListener = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_layout);
            encoder.WriteBoolean(this.m_draftMode);
            encoder.WriteBoolean(this.m_updateListener);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TOGGLE_ATTACK_MODE;
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

                    if (buildingData.GetGearUpBuildingData() == null || building.GetGearLevel() != 0)
                    {
                        if (building.GetAttackerItemData().HasAlternativeAttackMode())
                        {
                            LogicCombatComponent combatComponent = building.GetCombatComponent(false);

                            if (combatComponent != null)
                            {
                                combatComponent.ToggleAttackMode(this.m_layout, this.m_draftMode);

                                if (this.m_updateListener)
                                {
                                }

                                return 0;
                            }
                        }

                        return -1;
                    }

                    return -95;
                }

                if (gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                {
                    LogicTrap trap = (LogicTrap) gameObject;

                    if (trap.HasAirMode())
                    {
                        trap.ToggleAirMode(this.m_layout, this.m_draftMode);

                        if (this.m_updateListener)
                        {
                        }

                        return 0;
                    }
                }
            }

            return -1;
        }
    }
}