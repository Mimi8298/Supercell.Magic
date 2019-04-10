namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;

    public class LogicTransactionsRevokedCommand : LogicServerCommand
    {
        private int m_diamondCount;

        public void SetAmount(int diamondCount)
        {
            this.m_diamondCount = diamondCount;
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            this.m_diamondCount = stream.ReadInt();
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            base.Encode(encoder);
            encoder.WriteInt(this.m_diamondCount);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.SetDiamonds(playerAvatar.GetDiamonds() - this.m_diamondCount);

                if (playerAvatar.GetFreeDiamonds() > playerAvatar.GetDiamonds())
                {
                    playerAvatar.SetFreeDiamonds(playerAvatar.GetDiamonds());
                }

                playerAvatar.AddCumulativePurchasedDiamonds(-this.m_diamondCount);

                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TRANSACTIONS_REVOKED;
        }
    }
}