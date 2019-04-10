namespace Supercell.Magic.Logic
{
    using System;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;

    public class LogicTargetList
    {
        private int m_charVersusCharRandomDistance;
        private int m_targetListSize;

        private readonly LogicGameObject[] m_targets;
        private readonly int[] m_targetCosts;

        public LogicTargetList()
        {
            this.m_targets = new LogicGameObject[10];
            this.m_targetCosts = new int[10];

            this.Clear();

            this.m_charVersusCharRandomDistance = LogicDataTables.GetGlobals().GetCharVersusCharRandomDistanceLimit();
            this.m_targetListSize = LogicDataTables.GetGlobals().GetTargetListSize();
        }

        public void Destruct()
        {
            this.Clear();

            this.m_charVersusCharRandomDistance = 0;
            this.m_targetListSize = 3;
        }

        public void Clear()
        {
            for (int i = 0; i < 10; i++)
            {
                this.m_targets[i] = null;
                this.m_targetCosts[i] = 0x7FFFFFFF;
            }
        }

        public LogicGameObject EvaluateTargets(LogicMovementComponent component)
        {
            if (component != null && !component.IsFlying() && this.m_targetListSize > 1)
            {
                bool fullChar = true;

                int count = 0;

                int minCost = 0x7FFFFFFF;
                int maxCost = 0;

                LogicGameObject minCostTarget = null;

                for (int i = 0; i < this.m_targetListSize; i++)
                {
                    LogicGameObject target = this.m_targets[i];

                    if (target != null)
                    {
                        LogicCombatComponent combatComponent = component.GetParent().GetCombatComponent();

                        if (combatComponent != null && combatComponent.IsInRange(target))
                        {
                            return target;
                        }

                        int targetCost = component.EvaluateTargetCost(target);

                        if (target.GetMovementComponent() == null)
                        {
                            fullChar = false;
                        }

                        if (targetCost > maxCost)
                        {
                            maxCost = targetCost;
                        }

                        if (targetCost < minCost)
                        {
                            minCost = targetCost;
                            minCostTarget = target;
                        }

                        ++count;
                    }
                }

                if (count >= 2 && fullChar && minCost != 0x7FFFFFFF && maxCost - minCost < this.m_charVersusCharRandomDistance)
                {
                    return this.m_targets[component.GetParent().GetGlobalID() % count];
                }

                return minCostTarget;
            }

            return this.m_targets[0];
        }

        public void AddCandidate(LogicGameObject target, int cost)
        {
            int index = -1;

            for (int i = 0; i < this.m_targetListSize; i++)
            {
                if (this.m_targetCosts[i] > cost)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                Array.Copy(this.m_targets, index, this.m_targets, index + 1, this.m_targetListSize - index);
                Array.Copy(this.m_targetCosts, index, this.m_targetCosts, index + 1, this.m_targetListSize - index);

                this.m_targets[index] = target;
                this.m_targetCosts[index] = cost;
            }
        }
    }
}