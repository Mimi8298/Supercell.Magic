namespace Supercell.Magic.Logic.Command.Battle
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicMatchmakingCommand : LogicCommand
    {
        private LogicResourceData m_buyResourceData;
        private int m_buyResourceCount;

        public override void Decode(ByteStream stream)
        {
            this.m_buyResourceData = (LogicResourceData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.RESOURCE);
            this.m_buyResourceCount = stream.ReadInt();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            ByteStreamHelper.WriteDataReference(encoder, this.m_buyResourceData);
            encoder.WriteInt(this.m_buyResourceCount);

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.MATCHMAKING;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_buyResourceData = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (level.GetVillageType() == 0)
            {
                if (level.GetState() == 2 || level.GetState() == 1)
                {
                    if (this.m_buyResourceData != null)
                    {
                        if (this.m_buyResourceCount > 0 && !this.m_buyResourceData.IsPremiumCurrency())
                        {
                            int cost = LogicGamePlayUtil.GetResourceDiamondCost(this.m_buyResourceCount, this.m_buyResourceData);
                            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                            if (playerAvatar.GetUnusedResourceCap(this.m_buyResourceData) >= this.m_buyResourceCount)
                            {
                                if (playerAvatar.HasEnoughDiamonds(cost, true, level))
                                {
                                    playerAvatar.UseDiamonds(cost);
                                    playerAvatar.GetChangeListener()
                                                .DiamondPurchaseMade(5, this.m_buyResourceData.GetGlobalID(), this.m_buyResourceCount, cost, level.GetVillageType());
                                    playerAvatar.CommodityCountChangeHelper(0, this.m_buyResourceData, this.m_buyResourceCount);
                                }
                                else
                                {
                                    return -2;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                        }
                    }

                    level.GetGameListener().MatchmakingCommandExecuted();

                    return 0;
                }

                return -3;
            }

            return -32;
        }
    }
}