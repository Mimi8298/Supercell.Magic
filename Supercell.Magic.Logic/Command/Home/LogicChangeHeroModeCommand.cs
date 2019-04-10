namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicChangeHeroModeCommand : LogicCommand
    {
        private int m_gameObjectId;
        private int m_mode;

        public LogicChangeHeroModeCommand()
        {
            // LogicChangeHeroModeCommand.
        }

        public LogicChangeHeroModeCommand(int gameObjectId, int mode)
        {
            this.m_gameObjectId = gameObjectId;
            this.m_mode = mode;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_mode = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteInt(this.m_mode);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CHANGE_HERO_MODE;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;
                LogicHeroBaseComponent heroBaseComponent = building.GetHeroBaseComponent();

                if (heroBaseComponent != null && (uint) this.m_mode <= 1)
                {
                    return heroBaseComponent.SetHeroMode(this.m_mode) ? 0 : -2;
                }
            }

            return -1;
        }
    }
}