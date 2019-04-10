namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Titan.Util;

    public class LogicGameObjectFilter
    {
        private int m_team;
        private bool m_enemyOnly;
        private bool[] m_gameObjectTypes;

        private LogicArrayList<LogicGameObject> m_ignoreGameObjects;

        public LogicGameObjectFilter()
        {
            this.m_team = -1;
        }

        public virtual void Destruct()
        {
            this.m_gameObjectTypes = null;

            if (this.m_ignoreGameObjects != null)
            {
                this.m_ignoreGameObjects.Destruct();
                this.m_ignoreGameObjects = null;
            }
        }

        public bool ContainsGameObjectType(int type)
        {
            if (this.m_gameObjectTypes != null)
                return this.m_gameObjectTypes[type];
            return true;
        }

        public void AddGameObjectType(LogicGameObjectType type)
        {
            if (this.m_gameObjectTypes == null)
                this.m_gameObjectTypes = new bool[LogicGameObject.GAMEOBJECT_TYPE_COUNT];
            this.m_gameObjectTypes[(int) type] = true;
        }


        public virtual bool TestGameObject(LogicGameObject gameObject)
        {
            if (this.m_gameObjectTypes != null && !this.m_gameObjectTypes[(int) gameObject.GetGameObjectType()])
                return false;
            if (this.m_ignoreGameObjects != null && this.m_ignoreGameObjects.IndexOf(gameObject) != -1)
                return false;

            if (this.m_team != -1)
            {
                LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

                if (hitpointComponent != null)
                {
                    if (hitpointComponent.GetHitpoints() > 0)
                    {
                        bool isEnemy = hitpointComponent.IsEnemyForTeam(this.m_team);

                        if (isEnemy || !this.m_enemyOnly)
                        {
                            return this.m_enemyOnly || !isEnemy;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        public virtual bool IsComponentFilter()
        {
            return false;
        }

        public void PassEnemyOnly(LogicGameObject gameObject)
        {
            LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

            if (hitpointComponent != null)
            {
                this.m_team = hitpointComponent.GetTeam();
                this.m_enemyOnly = true;
            }
            else
            {
                this.m_team = -1;
            }
        }

        public void PassFriendlyOnly(LogicGameObject gameObject)
        {
            LogicHitpointComponent hitpointComponent = gameObject.GetHitpointComponent();

            if (hitpointComponent != null)
            {
                this.m_team = hitpointComponent.GetTeam();
                this.m_enemyOnly = false;
            }
            else
            {
                this.m_team = -1;
            }
        }

        public void RemoveAllIgnoreObjects()
        {
            if (this.m_ignoreGameObjects != null)
            {
                this.m_ignoreGameObjects.Destruct();
                this.m_ignoreGameObjects = null;
            }
        }

        public void AddIgnoreObject(LogicGameObject gameObject)
        {
            if (this.m_ignoreGameObjects == null)
                this.m_ignoreGameObjects = new LogicArrayList<LogicGameObject>();
            this.m_ignoreGameObjects.Add(gameObject);
        }
    }
}