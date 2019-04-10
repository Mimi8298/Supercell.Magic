namespace Supercell.Magic.Logic.GameObject.Component
{
    public sealed class LogicComponentFilter : LogicGameObjectFilter
    {
        private LogicComponentType m_componentType;
        
        public void SetComponentType(LogicComponentType type)
        {
            this.m_componentType = type;
        }

        public LogicComponentType GetComponentType()
        {
            return this.m_componentType;
        }

        public override bool IsComponentFilter()
        {
            return true;
        }

        public override bool TestGameObject(LogicGameObject gameObject)
        {
            if (gameObject.GetComponent(this.m_componentType) != null)
            {
                return base.TestGameObject(gameObject);
            }

            return false;
        }

        public bool TestComponent(LogicComponent component)
        {
            return this.TestGameObject(component.GetParent());
        }
    }
}