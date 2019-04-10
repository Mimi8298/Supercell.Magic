namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicTransferWarResourcesCommand : LogicCommand
    {
        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TRANSFER_WAR_RESOURCES;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicArrayList<LogicGameObject> gameObjects = level.GetGameObjectManagerAt(0).GetGameObjects(LogicGameObjectType.BUILDING);

            for (int i = 0; i < gameObjects.Size(); i++)
            {
                LogicBuilding building = (LogicBuilding) gameObjects[i];

                if (building.GetData() == LogicDataTables.GetAllianceCastleData())
                {
                    LogicWarResourceStorageComponent warResourceStorageComponent = building.GetWarResourceStorageComponent();

                    if (warResourceStorageComponent.IsNotEmpty())
                    {
                        warResourceStorageComponent.CollectResources();
                    }
                }
            }

            return 0;
        }
    }
}