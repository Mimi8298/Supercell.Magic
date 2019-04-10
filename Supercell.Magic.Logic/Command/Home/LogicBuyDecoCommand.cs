namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicBuyDecoCommand : LogicCommand
    {
        private int m_x;
        private int m_y;
        private LogicDecoData m_decoData;

        public LogicBuyDecoCommand()
        {
            // LogicBuyDecoCommand.
        }

        public LogicBuyDecoCommand(int x, int y, LogicDecoData decoData)
        {
            this.m_x = x;
            this.m_y = y;
            this.m_decoData = decoData;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
            this.m_decoData = (LogicDecoData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.DECO);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_x);
            encoder.WriteInt(this.m_y);
            ByteStreamHelper.WriteDataReference(encoder, this.m_decoData);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BUY_DECO;
        }

        public override void Destruct()
        {
            base.Destruct();

            this.m_x = 0;
            this.m_y = 0;
            this.m_decoData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_decoData != null)
            {
                if (this.m_decoData.GetVillageType() == level.GetVillageType())
                {
                    if (level.IsValidPlaceForBuilding(this.m_x, this.m_y, this.m_decoData.GetWidth(), this.m_decoData.GetHeight(), null))
                    {
                        LogicClientAvatar playerAvatar = level.GetPlayerAvatar();
                        LogicResourceData buildResourceData = this.m_decoData.GetBuildResource();

                        int buildCost = this.m_decoData.GetBuildCost();

                        if (playerAvatar.HasEnoughResources(buildResourceData, buildCost, true, this, false) && !level.IsDecoCapReached(this.m_decoData, true))
                        {
                            if (buildResourceData.IsPremiumCurrency())
                            {
                                playerAvatar.UseDiamonds(buildCost);
                                playerAvatar.GetChangeListener().DiamondPurchaseMade(1, this.m_decoData.GetGlobalID(), 0, buildCost, level.GetVillageType());
                            }
                            else
                            {
                                playerAvatar.CommodityCountChangeHelper(0, buildResourceData, -buildCost);
                            }

                            LogicDeco deco = (LogicDeco) LogicGameObjectFactory.CreateGameObject(this.m_decoData, level, level.GetVillageType());

                            deco.SetInitialPosition(this.m_x << 9, this.m_y << 9);
                            level.GetGameObjectManager().AddGameObject(deco, -1);

                            int width = deco.GetWidthInTiles();
                            int height = deco.GetHeightInTiles();

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

                    return -1;
                }

                return -32;
            }

            return -1;
        }
    }
}