namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicBuyTrapCommand : LogicCommand
    {
        private int m_x;
        private int m_y;
        private LogicTrapData m_trapData;

        public LogicBuyTrapCommand()
        {
            // LogicBuyTrapCommand.
        }

        public LogicBuyTrapCommand(int x, int y, LogicTrapData trapData)
        {
            this.m_x = x;
            this.m_y = y;
            this.m_trapData = trapData;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_trapData = (LogicTrapData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.TRAP);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            ByteStreamHelper.WriteDataReference(encoder, this.m_trapData);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BUY_TRAP;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_x = 0;
            this.m_y = 0;
            this.m_trapData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_trapData != null)
            {
                if (this.m_trapData.GetVillageType() == level.GetVillageType())
                {
                    if (level.IsValidPlaceForBuilding(this.m_x, this.m_y, this.m_trapData.GetWidth(), this.m_trapData.GetHeight(), null))
                    {
                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                        LogicResourceData buildResourceData = this.m_trapData.GetBuildResource();

                        int buildCost = this.m_trapData.GetBuildCost(0);

                        if (playerAvatar.HasEnoughResources(buildResourceData, buildCost, true, this, false) && !level.IsTrapCapReached(this.m_trapData, true))
                        {
                            if (level.GetGameMode().GetCalendar().IsEnabled(this.m_trapData))
                            {
                                if (buildResourceData.IsPremiumCurrency())
                                {
                                    playerAvatar.UseDiamonds(buildCost);
                                    playerAvatar.GetChangeListener().DiamondPurchaseMade(1, this.m_trapData.GetGlobalID(), 0, buildCost, level.GetVillageType());
                                }
                                else
                                {
                                    playerAvatar.CommodityCountChangeHelper(0, buildResourceData, -buildCost);
                                }

                                LogicTrap trap = (LogicTrap) LogicGameObjectFactory.CreateGameObject(this.m_trapData, level, level.GetVillageType());

                                if (this.m_trapData.GetBuildTime(0) == 0)
                                {
                                    trap.FinishConstruction(false);
                                }

                                trap.SetInitialPosition(this.m_x << 9, this.m_y << 9);
                                level.GetGameObjectManager().AddGameObject(trap, -1);

                                if (level.IsTrapCapReached(this.m_trapData, false))
                                {
                                    level.GetGameListener().TrapCapReached(this.m_trapData);
                                }

                                int width = trap.GetWidthInTiles();
                                int height = trap.GetHeightInTiles();

                                for (int i = 0; i < width; i++)
                                {
                                    for (int j = 0; j < height; j++)
                                    {
                                        LogicObstacle tallGrass = level.GetTileMap().GetTile(this.m_x + i, this.m_y + j).GetTallGrass();

                                        if (tallGrass != null)
                                        {
                                            level.GetGameObjectManager().RemoveGameObject(tallGrass);
                                        }
                                    }
                                }

                                return 0;
                            }
                        }
                    }

                    return -1;
                }

                return -32;
            }

            return -1;
        }
    }
}