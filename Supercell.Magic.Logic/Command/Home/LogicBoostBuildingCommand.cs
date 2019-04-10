namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicBoostBuildingCommand : LogicCommand
    {
        private LogicArrayList<int> m_gameObjectIds;

        public LogicBoostBuildingCommand()
        {
            this.m_gameObjectIds = new LogicArrayList<int>();
        }

        public override void Decode(ByteStream stream)
        {
            for (int i = 0, size = stream.ReadInt(); i < size; i++)
            {
                this.m_gameObjectIds.Add(stream.ReadInt());
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_gameObjectIds.Size());

            for (int i = 0; i < this.m_gameObjectIds.Size(); i++)
            {
                encoder.WriteInt(this.m_gameObjectIds[i]);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BOOST_BUILDING;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_gameObjectIds = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_gameObjectIds.Size() > 0)
            {
                int cost = 0;

                for (int i = 0; i < this.m_gameObjectIds.Size(); i++)
                {
                    LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectIds[i]);

                    if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        if (gameObject.GetData().GetVillageType() == level.GetVillageType())
                        {
                            LogicBuilding building = (LogicBuilding) gameObject;

                            if (!building.IsLocked())
                            {
                                if (!LogicDataTables.GetGlobals().UseNewTraining() || building.GetUnitProductionComponent() == null)
                                {
                                    if (building.CanBeBoosted())
                                    {
                                        cost += building.GetBoostCost();
                                    }

                                    continue;
                                }

                                return -3;
                            }

                            return -4;
                        }

                        return -32;
                    }

                    return -5;
                }

                LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                if (cost > 0)
                {
                    if (!playerAvatar.HasEnoughDiamonds(cost, true, level))
                    {
                        return -2;
                    }

                    playerAvatar.UseDiamonds(cost);
                    playerAvatar.GetChangeListener().DiamondPurchaseMade(8, 0, 0, cost, level.GetVillageType());
                }

                for (int i = 0; i < this.m_gameObjectIds.Size(); i++)
                {
                    LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectIds[i]);

                    if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                    {
                        LogicBuilding building = (LogicBuilding) gameObject;

                        if (building.GetMaxBoostTime() != 0)
                        {
                            building.Boost();
                        }
                    }
                }

                return 0;
            }

            return -1;
        }
    }
}