namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicDiamondsAddedCommand : LogicServerCommand
    {
        private bool m_freeDiamonds;
        private bool m_bundlePackage;

        private int m_source;
        private int m_diamondsCount;
        private int m_billingPackageId;

        private string m_transactionId;

        public void SetData(bool free, int diamondCount, int billingPackage, bool bundlePackage, int source, string transactionId)
        {
            this.m_freeDiamonds = free;
            this.m_diamondsCount = diamondCount;
            this.m_billingPackageId = billingPackage;
            this.m_bundlePackage = bundlePackage;
            this.m_source = source;
            this.m_transactionId = transactionId;
        }

        public override void Destruct()
        {
            base.Destruct();
            this.m_transactionId = null;
        }

        public override void Decode(ByteStream stream)
        {
            this.m_freeDiamonds = stream.ReadBoolean();
            this.m_diamondsCount = stream.ReadInt();
            this.m_billingPackageId = stream.ReadInt();
            this.m_bundlePackage = stream.ReadBoolean();
            this.m_source = stream.ReadInt();
            this.m_transactionId = stream.ReadString(900000);

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteBoolean(this.m_freeDiamonds);
            encoder.WriteInt(this.m_diamondsCount);
            encoder.WriteInt(this.m_billingPackageId);
            encoder.WriteBoolean(this.m_bundlePackage);
            encoder.WriteInt(this.m_source);
            encoder.WriteString(this.m_transactionId);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                if (this.m_source == 1)
                {
                    // listener.
                }

                playerAvatar.SetDiamonds(playerAvatar.GetDiamonds() + this.m_diamondsCount);
                playerAvatar.GetChangeListener().FreeDiamondsAdded(this.m_diamondsCount, 0);

                if (this.m_freeDiamonds)
                {
                    int freeDiamonds = playerAvatar.GetFreeDiamonds();

                    if (this.m_diamondsCount < 0)
                    {
                        if (freeDiamonds - this.m_diamondsCount >= 0 && playerAvatar.GetDiamonds() != freeDiamonds)
                        {
                            playerAvatar.SetFreeDiamonds(freeDiamonds + this.m_diamondsCount);
                        }
                    }
                    else
                    {
                        playerAvatar.SetFreeDiamonds(freeDiamonds + this.m_diamondsCount);
                    }
                }
                else
                {
                    if (this.m_billingPackageId > 0)
                    {
                        LogicBillingPackageData billingPackageData = (LogicBillingPackageData) LogicDataTables.GetDataById(this.m_billingPackageId, LogicDataType.BILLING_PACKAGE);

                        if (billingPackageData != null)
                        {
                            if (billingPackageData.IsRED() && !this.m_bundlePackage)
                            {
                                int redPackageState = playerAvatar.GetRedPackageState();
                                int newRedPackageState = redPackageState | 0x10;

                                if ((redPackageState & 3) != 3)
                                {
                                    newRedPackageState = (int) (newRedPackageState & 0xFFFFFFFC);
                                }

                                playerAvatar.SetRedPackageState(newRedPackageState);
                            }
                        }
                    }

                    level.GetGameListener().DiamondsBought();
                    playerAvatar.AddCumulativePurchasedDiamonds(this.m_diamondsCount);
                }

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.DIAMONDS_ADDED;
        }
    }
}