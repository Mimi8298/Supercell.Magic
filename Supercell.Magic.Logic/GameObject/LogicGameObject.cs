namespace Supercell.Magic.Logic.GameObject
{
    using System.Runtime.CompilerServices;
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.GameObject.Listener;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public class LogicGameObject
    {
        public const int GAMEOBJECT_TYPE_COUNT = 9;
        
        protected readonly LogicGameObjectData m_data;
        protected readonly LogicLevel m_level;
        protected readonly LogicComponent[] m_components;

        protected LogicVector2 m_position;
        protected LogicGameObjectListener m_listener;

        protected int m_villageType;
        protected int m_globalId;
        protected int m_seed;

        private int m_freezeTime;
        private int m_freezeDelay;
        private int m_damageTime;
        private int m_preventsHealingTime;
        private int m_stealthTime;

        public LogicGameObject(LogicGameObjectData data, LogicLevel level, int villageType)
        {
            Debugger.DoAssert(villageType < 2, "VillageType not set! Game object has not been added to LogicGameObjectManager.");

            this.m_data = data;
            this.m_level = level;
            this.m_villageType = villageType;

            this.m_position = new LogicVector2();
            this.m_listener = new LogicGameObjectListener();
            this.m_components = new LogicComponent[LogicComponent.COMPONENT_TYPE_COUNT];
        }

        public virtual void Destruct()
        {
            if (this.m_level != null)
            {
                this.m_level.GetTileMap().RemoveGameObject(this);
            }

            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                if (this.m_components[i] != null)
                {
                    this.m_components[i].Destruct();
                    this.m_components[i] = null;
                }
            }

            if (this.m_position != null)
            {
                this.m_position.Destruct();
                this.m_position = null;
            }

            if (this.m_listener != null)
            {
                this.m_listener.Destruct();
                this.m_listener = null;
            }
        }

        public virtual void RemoveGameObjectReferences(LogicGameObject gameObject)
        {
            // RemoveGameObjectReferences.
        }

        public virtual void DeathEvent()
        {
            this.m_level.GetTileMap().RefreshPassable(this);

            if (this.m_listener != null)
            {
                this.m_listener.RefreshState();
            }
        }

        public virtual void SpawnEvent(LogicCharacterData data, int count, int upgLevel)
        {
        }

        public void AddComponent(LogicComponent component)
        {
            LogicComponentType componentType = component.GetComponentType();

            if (this.m_components[(int) componentType] == null)
            {
                this.m_level.GetComponentManagerAt(this.m_villageType).AddComponent(component);
                this.m_components[(int) componentType] = component;
            }
            else
            {
                Debugger.Error("LogicGameObject::addComponent - Component is already added.");
            }
        }

        public void EnableComponent(LogicComponentType componentType, bool enable)
        {
            LogicComponent component = this.m_components[(int) componentType];

            if (component != null)
            {
                component.SetEnabled(enable);
            }
        }

        public int GetX()
        {
            return this.m_position.m_x;
        }

        public int GetY()
        {
            return this.m_position.m_y;
        }

        public int GetTileX()
        {
            return this.m_position.m_x >> 9;
        }

        public virtual int GetMidX()
        {
            return this.m_position.m_x + (this.GetWidthInTiles() << 8);
        }

        public int GetTileY()
        {
            return this.m_position.m_y >> 9;
        }

        public virtual int GetMidY()
        {
            return this.m_position.m_y + (this.GetHeightInTiles() << 8);
        }

        public int GetDistanceSquaredTo(LogicGameObject gameObject)
        {
            int midX = this.GetMidX() - gameObject.GetMidX();
            int midY = this.GetMidY() - gameObject.GetMidY();

            return midX * midX + midY * midY;
        }

        public int GetVillageType()
        {
            return this.m_villageType;
        }

        public int GetGlobalID()
        {
            return this.m_globalId;
        }

        public LogicGameObjectData GetData()
        {
            return this.m_data;
        }

        public LogicLevel GetLevel()
        {
            return this.m_level;
        }

        public LogicGameObjectListener GetListener()
        {
            return this.m_listener;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogicComponent GetComponent(LogicComponentType componentType)
        {
            LogicComponent component = this.m_components[(int) componentType];

            if (component != null && component.IsEnabled())
            {
                return component;
            }

            return null;
        }

        public LogicCombatComponent GetCombatComponent(bool enabledOnly)
        {
            LogicCombatComponent component = (LogicCombatComponent) this.m_components[(int) LogicComponentType.COMBAT];

            if (component != null && (!enabledOnly || component.IsEnabled()))
            {
                return component;
            }

            return null;
        }

        public LogicCombatComponent GetCombatComponent()
        {
            return (LogicCombatComponent) this.GetComponent(LogicComponentType.COMBAT);
        }

        public LogicHitpointComponent GetHitpointComponent()
        {
            return (LogicHitpointComponent) this.GetComponent(LogicComponentType.HITPOINT);
        }

        public LogicMovementComponent GetMovementComponent()
        {
            return (LogicMovementComponent) this.GetComponent(LogicComponentType.MOVEMENT);
        }

        public LogicResourceProductionComponent GetResourceProductionComponent()
        {
            return (LogicResourceProductionComponent) this.GetComponent(LogicComponentType.RESOURCE_PRODUCTION);
        }

        public LogicBunkerComponent GetBunkerComponent()
        {
            return (LogicBunkerComponent) this.GetComponent(LogicComponentType.BUNKER);
        }

        public LogicTriggerComponent GetTriggerComponent()
        {
            return (LogicTriggerComponent) this.GetComponent(LogicComponentType.TRIGGER);
        }

        public LogicLayoutComponent GetLayoutComponent()
        {
            return (LogicLayoutComponent) this.GetComponent(LogicComponentType.LAYOUT);
        }

        public LogicDefenceUnitProductionComponent GetDefenceUnitProduction()
        {
            return (LogicDefenceUnitProductionComponent) this.GetComponent(LogicComponentType.DEFENCE_UNIT_PRODUCTION);
        }

        public void RemoveComponent(LogicComponentType componentType)
        {
            if (this.m_components[(int) componentType] != null)
            {
                this.m_components[(int) componentType].Destruct();
                this.m_components[(int) componentType] = null;
            }
        }

        public void Shrink(int time, int speedBoost)
        {
            LogicCombatComponent combatComponent = this.GetCombatComponent();

            if (combatComponent != null)
            {
                combatComponent.Boost(0, speedBoost, time);
            }

            LogicMovementComponent movementComponent = this.GetMovementComponent();

            if (movementComponent != null)
            {
                movementComponent.GetMovementSystem().Boost(speedBoost, time * 4);
            }
        }

        public void Freeze(int time, int delay)
        {
            if (this.m_freezeTime > 0 && this.m_freezeDelay == 0)
            {
                this.m_freezeTime = LogicMath.Max(time - delay, this.m_freezeTime);
            }
            else if (this.m_freezeDelay > 0)
            {
                this.m_freezeDelay = LogicMath.Max(delay, this.m_freezeDelay);
                this.m_freezeTime = LogicMath.Max(time, this.m_freezeTime);
            }
            else
            {
                this.m_freezeTime = time;
                this.m_freezeDelay = delay;
            }
        }

        public bool IsFrozen()
        {
            if (this.m_freezeTime <= 0 || this.m_freezeDelay != 0)
            {
                return false;
            }

            LogicMovementComponent movementComponent = this.GetMovementComponent();

            if (movementComponent != null)
            {
                return !movementComponent.IsInNotPassablePosition();
            }

            return true;
        }

        public bool IsPreventsHealing()
        {
            return this.m_preventsHealingTime > 0;
        }

        public bool IsDamagedRecently()
        {
            return this.m_damageTime > 0;
        }

        public bool IsStealthy()
        {
            return this.m_stealthTime > 0;
        }

        public void SetStealthTime(int time)
        {
            this.m_stealthTime = time;
        }

        public void SetDamageTime(int time)
        {
            this.m_damageTime = time;
        }

        public void SetPreventsHealingTime(int time)
        {
            this.m_preventsHealingTime = time;
        }

        public LogicComponentManager GetComponentManager()
        {
            return this.m_level.GetComponentManagerAt(this.m_villageType);
        }

        public LogicGameObjectManager GetGameObjectManager()
        {
            return this.m_level.GetGameObjectManagerAt(this.m_villageType);
        }

        public void RefreshPassable()
        {
            this.m_level.GetTileMap().RefreshPassable(this);
        }

        public LogicVector2 GetPosition()
        {
            return this.m_position;
        }

        public void SetPosition(LogicVector2 vector2)
        {
            this.m_position.Set(vector2.m_x, vector2.m_y);
        }

        public void SetPositionXY(int x, int y)
        {
            if (this.m_position.m_x != x || this.m_position.m_y != y)
            {
                int prevX = this.GetTileX();
                int prevY = this.GetTileY();

                this.m_position.Set(x, y);

                LogicLayoutComponent layoutComponent = this.GetLayoutComponent();

                if (layoutComponent != null)
                {
                    layoutComponent.SetPositionLayout(this.m_level.GetActiveLayout(), x >> 9, y >> 9);
                }

                if (this.m_globalId != 0)
                {
                    this.m_level.GetTileMap().GameObjectMoved(this, prevX, prevY);
                }
            }
        }

        public LogicVector2 GetPositionLayout(int layoutId, bool editMode)
        {
            LogicLayoutComponent layoutComponent = this.GetLayoutComponent();
            Debugger.DoAssert(layoutComponent != null, "LayoutComponent is null");

            if (editMode)
            {
                return layoutComponent.GetEditModePositionLayout(layoutId);
            }

            return layoutComponent.GetPositionLayout(layoutId);
        }

        public void SetPositionLayoutXY(int tileX, int tileY, int activeLayout, bool editMode)
        {
            if (this.m_components[(int) LogicComponentType.LAYOUT] != null)
            {
                LogicLayoutComponent layoutComponent = (LogicLayoutComponent) this.m_components[(int) LogicComponentType.LAYOUT];

                if (layoutComponent.IsEnabled())
                {
                    if (editMode)
                    {
                        layoutComponent.SetEditModePositionLayout(activeLayout, tileX, tileY);
                    }
                    else
                    {
                        layoutComponent.SetPositionLayout(activeLayout, tileX, tileY);
                    }
                }
            }
        }

        public void SetGlobalID(int globalId)
        {
            this.m_globalId = globalId;
        }

        public void SetSeed(int seed)
        {
            this.m_seed = seed;
        }

        public int Rand(int rnd)
        {
            int seed = this.m_seed + rnd;

            if (seed == 0)
            {
                seed = -1;
            }

            int tmp1 = seed ^ (seed << 14) ^ ((seed ^ (seed << 14)) >> 16);
            int tmp2 = (tmp1 ^ (32 * tmp1)) & 0x7FFFFFFF;

            return tmp2;
        }

        public void SetListener(LogicGameObjectListener listener)
        {
            this.m_listener = listener;
        }

        public void XpGainHelper(int xp, LogicAvatar homeOwnerAvatar, bool inHomeState)
        {
            LogicClientAvatar playerAvatar = this.m_level.GetPlayerAvatar();

            if (!homeOwnerAvatar.IsInExpLevelCap())
            {
                if (homeOwnerAvatar == playerAvatar && this.m_level.GetState() == 1 && inHomeState)
                {
                    if (this.m_listener != null)
                    {
                        this.m_listener.XpGained(xp);
                    }
                }
            }

            homeOwnerAvatar.XpGainHelper(xp);
        }

        public virtual void SetInitialPosition(int x, int y)
        {
            this.m_position.Set(x, y);

            LogicLayoutComponent layoutComponent = this.GetLayoutComponent();

            if (layoutComponent != null && this.m_level != null)
            {
                layoutComponent.SetPositionLayout(this.m_level.GetActiveLayout(), x >> 9, y >> 9);
            }
        }

        public virtual int GetDirection()
        {
            return 0;
        }

        public virtual int PassableSubtilesAtEdge()
        {
            return 1;
        }

        public virtual LogicGameObjectType GetGameObjectType()
        {
            return 0;
        }

        public virtual bool IsStaticObject()
        {
            return true;
        }

        public virtual bool IsHidden()
        {
            return false;
        }

        public bool IsAlive()
        {
            LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();
            return hitpointComponent == null || hitpointComponent.GetHitpoints() > 0;
        }

        public virtual bool IsBuilding()
        {
            return false;
        }

        public virtual bool IsFlying()
        {
            LogicMovementComponent movementComponent = this.GetMovementComponent();
            return movementComponent != null && movementComponent.IsFlying();
        }

        public virtual bool IsPassable()
        {
            return true;
        }

        public virtual bool IsUnbuildable()
        {
            return true;
        }

        public virtual bool IsWall()
        {
            return false;
        }

        public virtual bool IsHero()
        {
            return false;
        }

        public virtual int PathFinderCost()
        {
            return 0;
        }

        public virtual int GetHeightInTiles()
        {
            return 1;
        }

        public virtual int GetWidthInTiles()
        {
            return 1;
        }

        public virtual int GetRemainingBoostTime()
        {
            return 0;
        }

        public virtual bool IsBoostPaused()
        {
            return false;
        }

        public virtual void StopBoost()
        {
            // StopBoost.
        }

        public virtual int GetMaxFastForwardTime()
        {
            return -1;
        }

        public virtual bool ShouldDestruct()
        {
            return false;
        }

        public virtual int GetStrengthWeight()
        {
            return 0;
        }

        public virtual void FastForwardTime(int secs)
        {
            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicComponent component = this.m_components[i];

                if (component != null && component.IsEnabled())
                {
                    component.FastForwardTime(secs);
                }
            }
        }

        public virtual void FastForwardBoost(int secs)
        {
            // FastForwardBoost.
        }

        public virtual int GetHitEffectOffset()
        {
            return 0;
        }

        public virtual void GetChecksum(ChecksumHelper checksum, bool includeGameObjects)
        {
            if (includeGameObjects)
            {
                checksum.StartObject("LogicGameObject");

                checksum.WriteValue("type", (int) this.GetGameObjectType());
                checksum.WriteValue("globalID", this.m_globalId);
                checksum.WriteValue("dataGlobalID", this.m_data.GetGlobalID());
                checksum.WriteValue("x", this.GetX());
                checksum.WriteValue("y", this.GetY());
                checksum.WriteValue("seed", this.m_seed);

                LogicHitpointComponent hitpointComponent = this.GetHitpointComponent();

                if (hitpointComponent != null)
                {
                    checksum.WriteValue("m_hp", hitpointComponent.GetHitpoints());
                    checksum.WriteValue("m_maxHP", hitpointComponent.GetMaxHitpoints());
                }

                LogicCombatComponent combatComponent = this.GetCombatComponent();

                if (combatComponent != null)
                {
                    LogicGameObject target = combatComponent.GetTarget(0);

                    if (target != null)
                    {
                        checksum.WriteValue("target", target.GetGlobalID());
                    }
                }

                checksum.EndObject();
            }
        }

        public virtual void SubTick()
        {
            // SubTick.
        }

        public virtual void Tick()
        {
            if (this.m_freezeTime <= 0 || this.m_freezeDelay != 0)
            {
                if (this.m_freezeDelay > 0)
                {
                    this.m_freezeDelay -= 1;
                }
            }
            else
            {
                this.m_freezeTime -= 1;
            }

            if (this.m_preventsHealingTime > 0)
            {
                this.m_preventsHealingTime -= 1;
            }

            if (this.m_stealthTime > 0)
            {
                this.m_stealthTime -= 1;
            }

            if (this.m_damageTime > 0)
            {
                this.m_damageTime -= 1;
            }
        }

        public virtual void Load(LogicJSONObject jsonObject)
        {
            this.LoadPosition(jsonObject);

            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicComponent component = this.m_components[i];

                if (component != null)
                {
                    component.Load(jsonObject);
                }
            }
        }

        public virtual void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            LogicJSONNumber xNumber = jsonObject.GetJSONNumber("x");
            LogicJSONNumber yNumber = jsonObject.GetJSONNumber("y");

            if (xNumber == null || yNumber == null)
            {
                Debugger.Error("LogicGameObject::load - x or y is NULL!");
            }

            this.SetInitialPosition(xNumber.GetIntValue() << 9, yNumber.GetIntValue() << 9);

            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicComponent component = this.m_components[i];

                if (component != null)
                {
                    component.LoadFromSnapshot(jsonObject);
                }
            }
        }

        public void LoadPosition(LogicJSONObject jsonObject)
        {
            LogicJSONNumber xNumber = jsonObject.GetJSONNumber("x");
            LogicJSONNumber yNumber = jsonObject.GetJSONNumber("y");

            if (xNumber == null || yNumber == null)
            {
                Debugger.Error("LogicGameObject::load - x or y is NULL!");
            }

            this.SetInitialPosition(xNumber.GetIntValue() << 9, yNumber.GetIntValue() << 9);
        }

        public virtual void Save(LogicJSONObject jsonObject, int villageType)
        {
            jsonObject.Put("x", new LogicJSONNumber(this.GetTileX() & 63));
            jsonObject.Put("y", new LogicJSONNumber(this.GetTileY() & 63));

            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicComponent component = this.m_components[i];

                if (component != null)
                {
                    component.Save(jsonObject, villageType);
                }
            }
        }

        public virtual void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicComponent component = this.m_components[i];

                if (component != null)
                {
                    component.SaveToSnapshot(jsonObject, layoutId);
                }
            }
        }

        public virtual void LoadingFinished()
        {
            for (int i = 0; i < LogicComponent.COMPONENT_TYPE_COUNT; i++)
            {
                LogicComponent component = this.m_components[i];

                if (component != null)
                {
                    component.LoadingFinished();
                }
            }
        }
    }

    public enum LogicGameObjectType
    {
        BUILDING,
        CHARACTER,
        PROJECTILE,
        OBSTACLE,
        TRAP,
        ALLIANCE_PORTAL,
        DECO,
        SPELL,
        VILLAGE_OBJECT
    }
}