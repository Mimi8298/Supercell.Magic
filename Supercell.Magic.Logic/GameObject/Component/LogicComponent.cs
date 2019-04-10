namespace Supercell.Magic.Logic.GameObject.Component
{
    using Supercell.Magic.Logic.GameObject.Listener;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.Json;

    public class LogicComponent
    {
        public const int COMPONENT_TYPE_COUNT = 17;
        
        protected bool m_enabled;
        protected LogicGameObject m_parent;

        public LogicComponent(LogicGameObject gameObject)
        {
            this.m_parent = gameObject;
            this.m_enabled = true;
        }

        public virtual void Destruct()
        {
            this.m_parent.GetLevel().GetComponentManagerAt(this.m_parent.GetVillageType()).RemoveComponent(this);

            this.m_enabled = false;
            this.m_parent = null;
        }

        public LogicGameObject GetParent()
        {
            return this.m_parent;
        }

        public LogicGameObjectListener GetParentListener()
        {
            return this.m_parent.GetListener();
        }

        public bool IsEnabled()
        {
            return this.m_enabled;
        }

        public void SetEnabled(bool value)
        {
            this.m_enabled = value;
        }

        public virtual LogicComponentType GetComponentType()
        {
            return 0;
        }

        public virtual void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            // RemoveGameObjectReferences.
        }

        public virtual void FastForwardTime(int time)
        {
            // FastForwardTime.
        }

        public virtual void GetChecksum(ChecksumHelper checksum)
        {
            // GetChecksum.
        }

        public virtual void SubTick()
        {
            // SubTick.
        }

        public virtual void Tick()
        {
            // Tick.
        }

        public virtual void Load(LogicJSONObject jsonObject)
        {
            // Load.
        }

        public virtual void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            // Load.
        }

        public virtual void Save(LogicJSONObject jsonObject, int villageType)
        {
            // Save.
        }

        public virtual void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            // SaveToSnapshot.
        }

        public virtual void LoadingFinished()
        {
            // LoadingFinished.
        }
    }

    public enum LogicComponentType
    {
        UNIT_STORAGE,
        COMBAT,
        HITPOINT,
        UNIT_PRODUCTION,
        MOVEMENT,
        RESOURCE_PRODUCTION,
        RESOURCE_STORAGE,
        BUNKER,
        TRIGGER,
        UNIT_UPGRADE,
        HERO_BASE,
        WAR_RESOURCE_STORAGE,
        SPAWNER,
        LAYOUT,
        LOOT_CART,
        VILLAGE2_UNIT,
        DEFENCE_UNIT_PRODUCTION
    }
}