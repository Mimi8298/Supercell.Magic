namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Time;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicDefenceUnitProductionComponent : LogicComponent
    {
        private LogicTimer m_spawnCooldownTimer;
        private LogicArrayList<LogicCharacter> m_defenceTroops;
        private LogicCharacterData[] m_defenceTroopData;

        private int m_defenceTroopUpgradeLevel;
        private int m_defenceTroopCooldownSecs;
        private int m_defenceTroopCount;
        private int m_maxDefenceTroopCount;

        public LogicDefenceUnitProductionComponent(LogicGameObject gameObject) : base(gameObject)
        {
            this.m_defenceTroops = new LogicArrayList<LogicCharacter>();
            this.m_defenceTroopData = new LogicCharacterData[2];
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_defenceTroops != null)
            {
                this.m_defenceTroops.Destruct();
                this.m_defenceTroops = null;
            }

            if (this.m_spawnCooldownTimer != null)
            {
                this.m_spawnCooldownTimer.Destruct();
                this.m_spawnCooldownTimer = null;
            }

            this.m_defenceTroopData = null;
        }

        public override void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            for (int i = 0, size = this.m_defenceTroops.Size(); i < size; i++)
            {
                if (this.m_defenceTroops[i] == gameObject)
                {
                    this.m_defenceTroops.Remove(i);
                    this.StartSpawnCooldownTimer();

                    break;
                }
            }
        }

        public override LogicComponentType GetComponentType()
        {
            return LogicComponentType.DEFENCE_UNIT_PRODUCTION;
        }

        public override void Tick()
        {
            if (this.m_parent.IsAlive())
            {
                if (LogicDataTables.GetGlobals().GuardPostNotFunctionalUnderUpgrade())
                {
                    if (this.m_parent.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        LogicBuilding building = (LogicBuilding) this.m_parent;

                        if (building.IsConstructing())
                        {
                            return;
                        }
                    }
                }

                if (this.m_maxDefenceTroopCount > this.m_defenceTroopCount)
                {
                    if (this.m_spawnCooldownTimer == null || this.m_spawnCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) <= 0)
                    {
                        this.SpawnCharacter(this.m_parent.GetX(), this.m_parent.GetY() + (this.m_parent.GetHeightInTiles() << 8));

                        if (this.m_maxDefenceTroopCount > this.m_defenceTroops.Size())
                        {
                            this.StartSpawnCooldownTimer();
                        }
                    }
                }
            }
        }

        public void StartSpawnCooldownTimer()
        {
            if (this.m_spawnCooldownTimer == null)
            {
                this.m_spawnCooldownTimer = new LogicTimer();
            }

            if (this.m_spawnCooldownTimer.GetRemainingSeconds(this.m_parent.GetLevel().GetLogicTime()) <= 0)
            {
                this.m_spawnCooldownTimer.StartTimer(this.m_defenceTroopCooldownSecs, this.m_parent.GetLevel().GetLogicTime(), false, -1);
            }
        }

        private void SpawnCharacter(int x, int y)
        {
            int idx = this.m_defenceTroopCount % 2;

            if (this.m_defenceTroopData[idx] == null)
            {
                idx = 0;
            }

            LogicBuilding building = (LogicBuilding) this.m_parent;
            LogicBuildingData buildingData = building.GetBuildingData();

            if (buildingData.IsEnabledInVillageType(this.m_parent.GetLevel().GetVillageType()) &&
                this.m_parent.GetLevel().GetState() != 1 &&
                this.m_parent.GetLevel().GetState() != 4)
            {
                LogicCharacterData data = this.m_defenceTroopData[idx];
                LogicCharacter character = (LogicCharacter) LogicGameObjectFactory.CreateGameObject(data, this.m_parent.GetLevel(), this.m_parent.GetVillageType());

                character.SetInitialPosition(x, y);
                character.SetUpgradeLevel(this.m_defenceTroopUpgradeLevel - 1);
                character.GetHitpointComponent()?.SetTeam(1);

                if (LogicDataTables.GetGlobals().EnableDefendingAllianceTroopJump())
                {
                    character.GetMovementComponent().EnableJump(3600000);
                }

                this.m_parent.GetGameObjectManager().AddGameObject(character, -1);

                character.GetCombatComponent().SetSearchRadius(LogicDataTables.GetGlobals().GetClanCastleRadius() >> 9);
                character.GetMovementComponent().GetMovementSystem().CreatePatrolArea(this.m_parent, this.m_parent.GetLevel(), true, this.m_defenceTroopCount);

                LogicDefenceUnitProductionComponent defenceUnitProductionComponent = building.GetDefenceUnitProduction();

                if (defenceUnitProductionComponent != null)
                {
                    defenceUnitProductionComponent.m_defenceTroops.Add(character);
                }

                ++this.m_defenceTroopCount;
            }
        }

        public void SetDefenceTroops(LogicCharacterData defenceTroopCharacter1, LogicCharacterData defenceTroopCharacter2, int defenceTroopCount, int defenceTroopLevel,
                                     int defenseTroopCooldownSecs)
        {
            this.m_defenceTroopData[0] = defenceTroopCharacter1;
            this.m_defenceTroopData[1] = defenceTroopCharacter2;
            this.m_maxDefenceTroopCount = defenceTroopCount;
            this.m_defenceTroopUpgradeLevel = defenceTroopLevel;
            this.m_defenceTroopCooldownSecs = defenseTroopCooldownSecs;
        }
    }
}