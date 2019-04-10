namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicCancelUnitProductionVillage2Command : LogicCommand
    {
        private int m_gameObjectId;

        public LogicCancelUnitProductionVillage2Command()
        {
            // LogicCancelUnitProductionVillage2Command.
        }

        public LogicCancelUnitProductionVillage2Command(int gameObjectId)
        {
            this.m_gameObjectId = gameObjectId;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_gameObjectId = stream.ReadInt();
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectId);
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.CANCEL_UNIT_PRODUCTION_VILLAGE_2;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_gameObjectId = 0;
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
            LogicGameObject gameObject = gameObjectManager.GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
            {
                LogicBuilding building = (LogicBuilding) gameObject;
                LogicVillage2UnitComponent village2UnitComponent = building.GetVillage2UnitComponent();

                if (village2UnitComponent != null)
                {
                    LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                    playerAvatar.CommodityCountChangeHelper(7, village2UnitComponent.GetUnitData(), -village2UnitComponent.GetUnitCount());
                    village2UnitComponent.RemoveUnits();

                    return 0;
                }
            }

            return -1;
        }
    }
}