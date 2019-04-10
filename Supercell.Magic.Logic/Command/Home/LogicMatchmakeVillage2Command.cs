namespace Supercell.Magic.Logic.Command.Battle
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicMatchmakeVillage2Command : LogicCommand
    {
        private LogicResourceData m_buyResourceData;
        private int m_buyResourceCount;

        public override void Decode(ByteStream stream)
        {
            this.m_buyResourceData = (LogicResourceData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.RESOURCE);
            this.m_buyResourceCount = stream.ReadVInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_buyResourceData);
            encoder.WriteVInt(this.m_buyResourceCount);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.MATCHMAKE_VILLAGE2;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_buyResourceData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 1)
            {
                if (level.IsUnitsTrainedVillage2())
                {
                    if (level.GetState() == 1)
                    {
                        if (this.m_buyResourceCount > 0)
                        {
                            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                            if (playerAvatar.GetUnusedResourceCap(LogicDataTables.GetGold2Data()) < this.m_buyResourceCount)
                            {
                                return -1;
                            }

                            int buyResourceCost = LogicGamePlayUtil.GetResourceDiamondCost(this.m_buyResourceCount, LogicDataTables.GetGold2Data());

                            if (playerAvatar.HasEnoughDiamonds(buyResourceCost, true, level))
                            {
                                return -2;
                            }

                            playerAvatar.UseDiamonds(buyResourceCost);
                            playerAvatar.GetChangeListener().DiamondPurchaseMade(5, LogicDataTables.GetGold2Data().GetGlobalID(), this.m_buyResourceCount, buyResourceCost,
                                                                                 level.GetVillageType());
                            playerAvatar.CommodityCountChangeHelper(0, LogicDataTables.GetGold2Data(), this.m_buyResourceCount);
                        }

                        level.GetGameListener().MatchmakingVillage2CommandExecuted();
                        return 0;
                    }

                    return -3;
                }

                return -24;
            }

            return -32;
        }
    }
}