namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.GameObject.Component;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicCollectResourcesCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicCollectResourcesCommand()
        {
            // LogicCollectResourcesCommand.
        }

        public LogicCollectResourcesCommand(int gameObjectId)
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
            return LogicCommandType.COLLECT_RESOURCES;
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
                if (gameObject.GetVillageType() == level.GetVillageType())
                {
                    LogicResourceProductionComponent resourceProductionComponent = gameObject.GetResourceProductionComponent();

                    if (resourceProductionComponent != null)
                    {
                        if (LogicDataTables.GetGlobals().CollectAllResourcesAtOnce())
                        {
                            int baseAvailableResources = resourceProductionComponent.GetResourceCount();
                            int baseCollectedResources = resourceProductionComponent.CollectResources(true);

                            bool storageIsFull = baseAvailableResources > 0 && baseCollectedResources == 0;

                            LogicArrayList<LogicComponent> components = level.GetComponentManager().GetComponents(resourceProductionComponent.GetComponentType());

                            for (int i = 0; i < components.Size(); i++)
                            {
                                LogicResourceProductionComponent component = (LogicResourceProductionComponent) components[i];

                                if (resourceProductionComponent != component && resourceProductionComponent.GetResourceData() == component.GetResourceData())
                                {
                                    int availableResources = component.GetResourceCount();
                                    int collectedResources = component.CollectResources(!storageIsFull);

                                    if (availableResources > 0)
                                    {
                                        if (collectedResources == 0)
                                        {
                                            storageIsFull = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            resourceProductionComponent.CollectResources(true);
                        }

                        return 0;
                    }

                    return -1;
                }

                return -3;
            }

            return -2;
        }
    }
}