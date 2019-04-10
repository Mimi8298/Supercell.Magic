namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.Debug;
    using Supercell.Magic.Titan.Json;

    public sealed class LogicAlliancePortal : LogicGameObject
    {
        public LogicAlliancePortal(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            LogicBunkerComponent bunkerComponent = new LogicBunkerComponent(this, 0);
            bunkerComponent.SetComponentMode(0);
            this.AddComponent(bunkerComponent);
        }

        public LogicAlliancePortalData GetAlliancePortalData()
        {
            return (LogicAlliancePortalData) this.m_data;
        }

        public override int GetWidthInTiles()
        {
            return 1;
        }

        public override int GetHeightInTiles()
        {
            return 1;
        }

        public override void Save(LogicJSONObject jsonObject, int villageType)
        {
            Debugger.Error("LogicAlliancePortal can't be saved");
        }

        public override void SaveToSnapshot(LogicJSONObject jsonObject, int layoutId)
        {
            Debugger.Error("LogicAlliancePortal can't be saved");
        }

        public override void Load(LogicJSONObject jsonObject)
        {
            Debugger.Error("LogicAlliancePortal can't be loaded");
        }

        public override void LoadFromSnapshot(LogicJSONObject jsonObject)
        {
            Debugger.Error("LogicAlliancePortal can't be loaded");
        }

        public override void LoadingFinished()
        {
            base.LoadingFinished();

            if (this.m_listener != null)
            {
                this.m_listener.LoadedFromJSON();
            }
        }

        public override bool ShouldDestruct()
        {
            return !this.m_level.IsInCombatState();
        }

        public override bool IsPassable()
        {
            return true;
        }

        public override LogicGameObjectType GetGameObjectType()
        {
            return LogicGameObjectType.ALLIANCE_PORTAL;
        }
    }
}