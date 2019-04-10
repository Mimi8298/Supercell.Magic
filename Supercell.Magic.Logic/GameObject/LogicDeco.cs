namespace Supercell.Magic.Logic.GameObject
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;

    public sealed class LogicDeco : LogicGameObject
    {
        public LogicDeco(LogicGameObjectData data, LogicLevel level, int villageType) : base(data, level, villageType)
        {
            this.AddComponent(new LogicLayoutComponent(this));
        }

        public LogicDecoData GetDecoData()
        {
            return (LogicDecoData) this.m_data;
        }

        public override LogicGameObjectType GetGameObjectType()
        {
            return LogicGameObjectType.DECO;
        }

        public override int GetWidthInTiles()
        {
            return this.GetDecoData().GetWidth();
        }

        public override int GetHeightInTiles()
        {
            return this.GetDecoData().GetHeight();
        }

        public override bool IsPassable()
        {
            return this.GetDecoData().IsPassable();
        }
    }
}