namespace Supercell.Magic.Logic.Command.Home
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Logic.Util;
    using Supercell.Magic.Titan.DataStream;

    public sealed class LogicBuyResourcesCommand : LogicCommand
    {
        private LogicCommand m_command;
        private LogicResourceData m_resourceData;
        private LogicResourceData m_resource2Data;
        private int m_resourceCount;
        private int m_resource2Count;

        public LogicBuyResourcesCommand()
        {
            // LogicBuyResourcesCommand.
        }

        public LogicBuyResourcesCommand(LogicResourceData data, int resourceCount, LogicResourceData resource2Data, int resource2Count, LogicCommand resourceCommand)
        {
            this.m_resourceData = data;
            this.m_resource2Data = resource2Data;
            this.m_command = resourceCommand;
            this.m_resourceCount = resourceCount;
            this.m_resource2Count = resource2Count;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_resourceCount = stream.ReadInt();
            this.m_resourceData = (LogicResourceData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.RESOURCE);
            this.m_resource2Count = stream.ReadInt();

            if (this.m_resource2Count > 0)
            {
                this.m_resource2Data = (LogicResourceData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.RESOURCE);
            }

            if (stream.ReadBoolean())
            {
                this.m_command = LogicCommandManager.DecodeCommand(stream);
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_resourceCount);
            ByteStreamHelper.WriteDataReference(encoder, this.m_resourceData);
            encoder.WriteInt(this.m_resource2Count);

            if (this.m_resource2Count > 0)
            {
                ByteStreamHelper.WriteDataReference(encoder, this.m_resource2Data);
            }

            if (this.m_command != null)
            {
                encoder.WriteBoolean(true);
                LogicCommandManager.EncodeCommand(encoder, this.m_command);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            base.Encode(encoder);
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.BUY_RESOURCES;
        }

        public override void Destruct()
        {
            base.Destruct();

            if (this.m_command != null)
            {
                this.m_command.Destruct();
                this.m_command = null;
            }

            this.m_resourceData = null;
            this.m_resource2Data = null;
        }

        public override int Execute(LogicLevel level)
        {
            if (this.m_resourceData != null && this.m_resourceCount > 0 && !this.m_resourceData.IsPremiumCurrency())
            {
                LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

                if (this.m_resource2Data != null && this.m_resource2Count > 0)
                {
                    if (playerAvatar.GetUnusedResourceCap(this.m_resourceData) >= this.m_resourceCount &&
                        playerAvatar.GetUnusedResourceCap(this.m_resource2Data) >= this.m_resource2Count)
                    {
                        int resourceCost = LogicGamePlayUtil.GetResourceDiamondCost(this.m_resourceCount, this.m_resourceData);
                        int resourceCost2 = LogicGamePlayUtil.GetResourceDiamondCost(this.m_resource2Count, this.m_resource2Data);

                        if (playerAvatar.HasEnoughDiamonds(resourceCost + resourceCost2, true, level))
                        {
                            playerAvatar.UseDiamonds(resourceCost + resourceCost2);
                            playerAvatar.CommodityCountChangeHelper(0, this.m_resourceData, this.m_resourceCount);
                            playerAvatar.CommodityCountChangeHelper(0, this.m_resource2Data, this.m_resource2Count);
                            playerAvatar.GetChangeListener().DiamondPurchaseMade(5, this.m_resource2Data.GetGlobalID(), this.m_resource2Count, resourceCost + resourceCost2,
                                                                                 level.GetVillageType());

                            if (this.m_command != null)
                            {
                                int cmdType = (int) this.m_command.GetCommandType();

                                if (cmdType < 1000)
                                {
                                    if (cmdType >= 500 && cmdType < 700)
                                    {
                                        this.m_command.Execute(level);
                                    }
                                }
                            }

                            return 0;
                        }
                    }
                }
                else
                {
                    if (playerAvatar.GetUnusedResourceCap(this.m_resourceData) >= this.m_resourceCount)
                    {
                        int resourceCost = LogicGamePlayUtil.GetResourceDiamondCost(this.m_resourceCount, this.m_resourceData);

                        if (playerAvatar.HasEnoughDiamonds(resourceCost, true, level))
                        {
                            playerAvatar.UseDiamonds(resourceCost);
                            playerAvatar.CommodityCountChangeHelper(0, this.m_resourceData, this.m_resourceCount);
                            playerAvatar.GetChangeListener().DiamondPurchaseMade(5, this.m_resourceData.GetGlobalID(), this.m_resourceCount, resourceCost, level.GetVillageType());

                            if (this.m_command != null)
                            {
                                int cmdType = (int) this.m_command.GetCommandType();

                                if (cmdType < 1000)
                                {
                                    if (cmdType >= 500 && cmdType < 700)
                                    {
                                        this.m_command.Execute(level);
                                    }
                                }
                            }

                            return 0;
                        }
                    }
                }
            }

            return -1;
        }
    }
}