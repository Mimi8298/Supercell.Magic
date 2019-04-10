namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicPlaceUnplacedObjectCommand : LogicCommand
    {
        private int m_x;
        private int m_y;
        private int m_upgradeLevel;

        private LogicGameObjectData m_gameObjectData;

        public LogicPlaceUnplacedObjectCommand()
        {
            // LogicPlaceUnplacedObjectCommand.
        }

        public LogicPlaceUnplacedObjectCommand(int x, int y, int upgradeLevel, LogicGameObjectData gameObjectData)
        {
            this.m_x = x;
            this.m_y = y;
            this.m_upgradeLevel = upgradeLevel;
            this.m_gameObjectData = gameObjectData;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_gameObjectData = (LogicGameObjectData) ByteStreamHelper.ReadDataReference(stream);
            this.m_upgradeLevel = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            ByteStreamHelper.WriteDataReference(encoder, this.m_gameObjectData);
            encoder.WriteInt(this.m_upgradeLevel);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.PLACE_UNPLACED_OBJECT;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_gameObjectData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_gameObjectData != null && level.GetUnplacedObjectCount(this.m_gameObjectData) > 0)
            {
                if (level.GetVillageType() == this.m_gameObjectData.GetVillageType())
                {
                    LogicDataType dataType = this.m_gameObjectData.GetDataType();

                    if (dataType == LogicDataType.BUILDING)
                    {
                        LogicBuildingData buildingData = (LogicBuildingData) this.m_gameObjectData;

                        if (level.IsValidPlaceForBuilding(this.m_x, this.m_y, buildingData.GetWidth(), buildingData.GetHeight(), null))
                        {
                            if (!level.RemoveUnplacedObject(this.m_gameObjectData, this.m_upgradeLevel))
                            {
                                return -63;
                            }

                            LogicBuilding building = (LogicBuilding) LogicGameObjectFactory.CreateGameObject(this.m_gameObjectData, level, level.GetVillageType());

                            building.SetPositionXY(this.m_x << 9, this.m_y << 9);
                            level.GetGameObjectManager().AddGameObject(building, -1);
                            building.FinishConstruction(false, true);
                            building.SetUpgradeLevel(this.m_upgradeLevel);
                        }

                        return 0;
                    }

                    if (dataType == LogicDataType.TRAP)
                    {
                        LogicTrapData trapData = (LogicTrapData) this.m_gameObjectData;

                        if (level.IsValidPlaceForBuilding(this.m_x, this.m_y, trapData.GetWidth(), trapData.GetHeight(), null))
                        {
                            if (!level.RemoveUnplacedObject(this.m_gameObjectData, this.m_upgradeLevel))
                            {
                                return -64;
                            }

                            LogicTrap trap = (LogicTrap) LogicGameObjectFactory.CreateGameObject(this.m_gameObjectData, level, level.GetVillageType());

                            trap.SetPositionXY(this.m_x << 9, this.m_y << 9);
                            trap.FinishConstruction(false);
                            trap.SetUpgradeLevel(this.m_upgradeLevel);
                            level.GetGameObjectManager().AddGameObject(trap, -1);
                        }

                        return 0;
                    }

                    if (dataType == LogicDataType.DECO)
                    {
                        LogicDecoData decoData = (LogicDecoData) this.m_gameObjectData;

                        if (level.IsValidPlaceForBuilding(this.m_x, this.m_y, decoData.GetWidth(), decoData.GetHeight(), null))
                        {
                            if (!level.RemoveUnplacedObject(this.m_gameObjectData, this.m_upgradeLevel))
                            {
                                return -65;
                            }

                            LogicDeco deco = (LogicDeco) LogicGameObjectFactory.CreateGameObject(this.m_gameObjectData, level, level.GetVillageType());

                            deco.SetPositionXY(this.m_x << 9, this.m_y << 9);
                            level.RemoveUnplacedObject(this.m_gameObjectData, this.m_upgradeLevel);
                            level.GetGameObjectManager().AddGameObject(deco, -1);
                        }

                        return 0;
                    }

                    return -3;
                }

                return -35;
            }

            return 0;
        }
    }
}