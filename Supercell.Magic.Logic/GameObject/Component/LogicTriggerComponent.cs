namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Command.Battle;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Math;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicTriggerComponent : LogicComponent
    {
        private bool m_airTrigger;
        private bool m_groundTrigger;
        private readonly bool m_healerTrigger;

        private readonly int m_triggerRadius;
        private readonly int m_minTriggerHousingLimit;

        private readonly bool m_triggeredByRadius;
        private bool m_triggered;
        private bool m_cmdTriggered;

        public LogicTriggerComponent(LogicGameObject gameObject, int triggerRadius, bool airTrigger, bool groundTrigger, bool healerTrigger, int minTriggerHousingLimit) :
            base(gameObject)
        {
            this.m_triggerRadius = triggerRadius;
            this.m_airTrigger = airTrigger;
            this.m_groundTrigger = groundTrigger;
            this.m_healerTrigger = healerTrigger;
            this.m_minTriggerHousingLimit = minTriggerHousingLimit;

            int tmp = ((LogicMath.Min(this.m_parent.GetWidthInTiles(), this.m_parent.GetHeightInTiles()) << 9) + 1024) >> 1;

            this.m_triggeredByRadius = tmp < triggerRadius;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.TRIGGER;
        }

        public override void Tick()
        {
            if (this.m_triggeredByRadius)
            {
                LogicLevel level = this.m_parent.GetLevel();

                if (level.IsInCombatState())
                {
                    if (!this.m_triggered)
                    {
                        if (((level.GetLogicTime().GetTick() / 4) & 7) == 0)
                        {
                            LogicArrayList<LogicComponent> components = this.m_parent.GetComponentManager().GetComponents(LogicComponentType.MOVEMENT);

                            for (int i = 0; i < components.Size(); i++)
                            {
                                LogicGameObject gameObject = components[i].GetParent();

                                if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                                {
                                    LogicCharacter character = (LogicCharacter) gameObject;
                                    LogicMovementComponent movementComponent = character.GetMovementComponent();

                                    bool triggerDisabled = false;

                                    if (movementComponent != null)
                                    {
                                        LogicMovementSystem movementSystem = movementComponent.GetMovementSystem();

                                        if (movementSystem != null && movementSystem.IsPushed())
                                        {
                                            triggerDisabled = movementSystem.IgnorePush();
                                        }
                                    }

                                    if (!triggerDisabled && character.GetCharacterData().GetTriggersTraps())
                                    {
                                        this.ObjectClose(character);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ObjectClose(LogicGameObject gameObject)
        {
            LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

            if (hitpointComponent == null || hitpointComponent.GetTeam() != 1)
            {
                if (gameObject.GetGameObjectType() == LogicGameObjectType.CHARACTER)
                {
                    LogicCharacter character = (LogicCharacter) gameObject;
                    LogicCharacterData data = character.GetCharacterData();

                    if (data.GetHousingSpace() < this.m_minTriggerHousingLimit)
                    {
                        return;
                    }
                }

                LogicCombatComponent combatComponent = gameObject.GetCombatComponent();

                if (combatComponent == null || combatComponent.GetUndergroundTime() <= 0)
                {
                    if ((!gameObject.IsFlying() || this.m_airTrigger) && (gameObject.IsFlying() || this.m_groundTrigger))
                    {
                        if (this.m_healerTrigger || combatComponent == null || !combatComponent.IsHealer())
                        {
                            int distanceX = gameObject.GetX() - this.m_parent.GetMidX();
                            int distanceY = gameObject.GetY() - this.m_parent.GetMidY();

                            if (LogicMath.Abs(distanceX) <= this.m_triggerRadius &&
                                LogicMath.Abs(distanceY) <= this.m_triggerRadius &&
                                distanceX * distanceX + distanceY * distanceY < (uint) (this.m_triggerRadius * this.m_triggerRadius))
                            {
                                this.Trigger();
                            }
                        }
                    }
                }
            }
        }

        private void Trigger()
        {
            if (!this.m_triggered)
            {
                Debugger.Print(string.Format("LogicTriggerComponent::trigger() -> {0}", this.m_parent.GetLevel().GetLogicTime().GetTick()));

                if (LogicDataTables.GetGlobals().UseTrapTriggerCommand())
                {
                    if (!this.m_cmdTriggered)
                    {
                        if (this.m_parent.GetLevel().GetState() != 5)
                        {
                            LogicTriggerComponentTriggeredCommand triggerComponentTriggeredCommand = new LogicTriggerComponentTriggeredCommand(this.m_parent);
                            triggerComponentTriggeredCommand.SetExecuteSubTick(this.m_parent.GetLevel().GetLogicTime().GetTick() + 1);
                            this.m_parent.GetLevel().GetGameMode().GetCommandManager().AddCommand(triggerComponentTriggeredCommand);
                        }

                        this.m_cmdTriggered = true;
                    }
                }
                else
                {
                    this.m_triggered = true;
                }
            }
        }

        public void SetAirTrigger(bool value)
        {
            this.m_airTrigger = value;
        }

        public void SetGroundTrigger(bool value)
        {
            this.m_groundTrigger = value;
        }

        public bool IsTriggeredByRadius()
        {
            return this.m_triggeredByRadius;
        }

        public bool IsTriggered()
        {
            return this.m_triggered;
        }

        public void SetTriggered()
        {
            this.m_triggered = true;
        }
    }
}