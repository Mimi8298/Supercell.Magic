namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Util;

    public sealed class LogicRepairTrapsCommand : LogicCommand
    {
        private readonly LogicArrayList<int> m_gameObjectIds;

        public LogicRepairTrapsCommand()
        {
            this.m_gameObjectIds = new LogicArrayList<int>(50);
        }

        public override void Decode(ByteStream stream)
        {
            for (int i = stream.ReadInt(); i > 0; i--)
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
            return LogicCommandType.REPAIR_TRAPS;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
            LogicGameObjectManager gameObjectManager = level.GetGameObjectManager();
            LogicResourceData repairResourceData = null;

            int repairCost = 0;

            for (int i = 0; i < this.m_gameObjectIds.Size(); i++)
            {
                LogicGameObject gameObject = gameObjectManager.GetGameObjectByID(this.m_gameObjectIds[i]);

                if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                {
                    LogicTrap trap = (LogicTrap) gameObject;

                    if (trap.IsDisarmed() && !trap.IsConstructing())
                    {
                        LogicTrapData data = trap.GetTrapData();

                        repairResourceData = data.GetBuildResource();
                        repairCost += data.GetRearmCost(trap.GetUpgradeLevel());
                    }
                }
            }

            if (repairResourceData != null && repairCost != 0)
            {
                if (playerAvatar.HasEnoughResources(repairResourceData, repairCost, true, this, false))
                {
                    playerAvatar.CommodityCountChangeHelper(0, repairResourceData, -repairCost);

                    for (int i = 0; i < this.m_gameObjectIds.Size(); i++)
                    {
                        LogicGameObject gameObject = gameObjectManager.GetGameObjectByID(this.m_gameObjectIds[i]);

                        if (gameObject != null && gameObject.GetGameObjectType() == LogicGameObjectType.TRAP)
                        {
                            LogicTrap trap = (LogicTrap) gameObject;

                            if (trap.IsDisarmed() && !trap.IsConstructing())
                            {
                                trap.RepairTrap();
                            }
                        }
                    }

                    return 0;
                }

                return -2;
            }

            return -1;
        }
    }
}