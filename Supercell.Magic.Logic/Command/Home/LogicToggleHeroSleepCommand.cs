namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicToggleHeroSleepCommand : LogicCommand
    {
        private int m_gameObjectId;
        private bool m_enabled;

        public LogicToggleHeroSleepCommand()
        {
            // LogicToggleHeroSleepCommand.
        }

        public LogicToggleHeroSleepCommand(int gameObjectId, bool enabled)
        {
            this.m_gameObjectId = gameObjectId;
            this.m_enabled = enabled;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            this.m_enabled = stream.ReadBoolean();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            encoder.WriteBoolean(this.m_enabled);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TOGGLE_HERO_SLEEP;
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

                if (heroBaseComponent != null)
                {
                    if (!heroBaseComponent.GetHeroData().HasNoDefence())
                    {
                        return heroBaseComponent.SetSleep(this.m_enabled) ? 0 : -2;
                    }

                    return -3;
                }

                return -4;
            }

            return -1;
        }
    }
}