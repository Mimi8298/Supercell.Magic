namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.GameObject;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicSellBuildingCommand : LogicCommand
    {
        private int m_gameObjectId;

        public LogicSellBuildingCommand()
        {
            // LogicSellBuildingCommand.
        }

        public LogicSellBuildingCommand(int gameObjectId)
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
            return LogicCommandType.SELL_BUILDING;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override int Execute(LogicLevel level)
        {
            LogicGameObject gameObject = level.GetGameObjectManager().GetGameObjectByID(this.m_gameObjectId);

            if (gameObject != null)
            {
                LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                if (gameObject.GetGameObjectType() == LogicGameObjectType.BUILDING)
                {
                    LogicBuilding building = (LogicBuilding) gameObject;

                    if (building.CanSell())
                    {
                        playerAvatar.CommodityCountChangeHelper(0, building.GetSellResource(), building.GetSellPrice());
                        building.OnSell();

                        level.GetGameObjectManager().RemoveGameObject(building);

                        return 0;
                    }
                }
                else if (gameObject.GetGameObjectType() == LogicGameObjectType.DECO)
                {
                    LogicDeco deco = (LogicDeco) gameObject;
                    LogicDecoData data = deco.GetDecoData();
                    LogicResourceData buildResourceData = data.GetBuildResource();

                    int sellPrice = data.GetSellPrice();

                    if (buildResourceData.IsPremiumCurrency())
                    {
                        playerAvatar.SetDiamonds(playerAvatar.GetDiamonds() + sellPrice);
                        playerAvatar.SetFreeDiamonds(playerAvatar.GetFreeDiamonds() + sellPrice);
                        playerAvatar.GetChangeListener().FreeDiamondsAdded(sellPrice, 6);
                    }
                    else
                    {
                        playerAvatar.CommodityCountChangeHelper(0, buildResourceData, sellPrice);
                    }

                    level.GetGameObjectManager().RemoveGameObject(deco);

                    return 0;
                }
            }

            return -1;
        }
    }
}