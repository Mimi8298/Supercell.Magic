namespace Supercell.Magic.Logic.Worker
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Util;

    public class LogicWorkerManager
    {
        private LogicLevel m_level;
        private LogicArrayList<LogicGameObject> m_constructions;

        private int m_workerCount;

        public LogicWorkerManager(LogicLevel level)
        {
            this.m_level = level;
            this.m_constructions = new LogicArrayList<LogicGameObject>();
        }

        public void Destruct()
        {
            if (this.m_constructions != null)
            {
                this.m_constructions.Destruct();
                this.m_constructions = null;
            }

            this.m_level = null;
            this.m_workerCount = 0;
        }

        public int GetFreeWorkers()
        {
            return this.m_workerCount - this.m_constructions.Size();
        }

        public int GetTotalWorkers()
        {
            return this.m_workerCount;
        }

        public void AllocateWorker(LogicGameObject gameObject)
        {
            if (this.m_constructions.IndexOf(gameObject) != -1)
            {
                Debugger.Warning("LogicWorkerManager::allocateWorker called twice for same target!");
                return;
            }

            this.m_constructions.Add(gameObject);
        }

        public void DeallocateWorker(LogicGameObject gameObject)
        {
            int index = this.m_constructions.IndexOf(gameObject);

            if (index != -1)
            {
                this.m_constructions.Remove(index);
            }
        }

        public void RemoveGameObjectReference(LogicGameObject gameObject)
        {
            this.DeallocateWorker(gameObject);
        }

        public void IncreaseWorkerCount()
        {
            ++this.m_workerCount;
        }

        public void DecreaseWorkerCount()
        {
            if (this.m_workerCount-- <= 0)
            {
                Debugger.Error("LogicWorkerManager - Total worker count below 0");
            }
        }

        public LogicGameObject GetShortestTaskGO()
        {
            LogicGameObject gameObject = null;

            for (int i = 0, minRemaining = -1, tmpRemaining = 0; i < this.m_constructions.Size(); i++, tmpRemaining = 0)
            {
                LogicGameObject tmp = this.m_constructions[i];

                switch (this.m_constructions[i].GetGameObjectType())
                {
                    case LogicGameObjectType.BUILDING:
                        LogicBuilding building = (LogicBuilding) tmp;

                        if (building.IsConstructing())
                        {
                            tmpRemaining = building.GetRemainingConstructionTime();
                        }
                        else
                        {
                            LogicHeroBaseComponent heroBaseComponent = building.GetHeroBaseComponent();

                            if (heroBaseComponent == null)
                            {
                                Debugger.Warning("LogicWorkerManager - Worker allocated to building with remaining construction time 0");
                            }
                            else
                            {
                                if (heroBaseComponent.IsUpgrading())
                                {
                                    tmpRemaining = heroBaseComponent.GetRemainingUpgradeSeconds();
                                }
                                else
                                {
                                    Debugger.Warning("LogicWorkerManager - Worker allocated to altar/herobase without hero upgrading");
                                }
                            }
                        }

                        break;
                    case LogicGameObjectType.OBSTACLE:
                        LogicObstacle obstacle = (LogicObstacle) tmp;

                        if (obstacle.IsClearingOnGoing())
                        {
                            tmpRemaining = obstacle.GetRemainingClearingTime();
                        }
                        else
                        {
                            Debugger.Warning("LogicWorkerManager - Worker allocated to obstacle with remaining clearing time 0");
                        }

                        break;
                    case LogicGameObjectType.TRAP:
                        LogicTrap trap = (LogicTrap) tmp;

                        if (trap.IsConstructing())
                        {
                            tmpRemaining = trap.GetRemainingConstructionTime();
                        }
                        else
                        {
                            Debugger.Warning("LogicWorkerManager - Worker allocated to trap with remaining construction time 0");
                        }

                        break;
                    case LogicGameObjectType.VILLAGE_OBJECT:
                        LogicVillageObject villageObject = (LogicVillageObject) tmp;

                        if (villageObject.IsConstructing())
                        {
                            tmpRemaining = villageObject.GetRemainingConstructionTime();
                        }
                        else
                        {
                            Debugger.Error("LogicWorkerManager - Worker allocated to building with remaining construction time 0 (vilobj)");
                        }

                        break;
                }

                if (gameObject == null || minRemaining > tmpRemaining)
                {
                    gameObject = tmp;
                    minRemaining = tmpRemaining;
                }
            }

            return gameObject;
        }

        public bool FinishTaskOfOneWorker()
        {
            LogicGameObject gameObject = this.GetShortestTaskGO();

            if (gameObject != null)
            {
                switch (gameObject.GetGameObjectType())
                {
                    case LogicGameObjectType.BUILDING:
                        LogicBuilding building = (LogicBuilding) gameObject;

                        if (building.IsConstructing())
                        {
                            return building.SpeedUpConstruction();
                        }

                        if (building.GetHeroBaseComponent() != null)
                        {
                            return building.GetHeroBaseComponent().SpeedUp();
                        }

                        break;
                    case LogicGameObjectType.OBSTACLE:
                        LogicObstacle obstacle = (LogicObstacle) gameObject;

                        if (obstacle.IsClearingOnGoing())
                        {
                            return obstacle.SpeedUpClearing();
                        }

                        break;
                    case LogicGameObjectType.TRAP:
                        LogicTrap trap = (LogicTrap) gameObject;

                        if (trap.IsConstructing())
                        {
                            return trap.SpeedUpConstruction();
                        }

                        break;
                    case LogicGameObjectType.VILLAGE_OBJECT:
                        LogicVillageObject villageObject = (LogicVillageObject) gameObject;

                        if (villageObject.IsConstructing())
                        {
                            return villageObject.SpeedUpCostruction();
                        }

                        break;
                }
            }

            return false;
        }
    }
}