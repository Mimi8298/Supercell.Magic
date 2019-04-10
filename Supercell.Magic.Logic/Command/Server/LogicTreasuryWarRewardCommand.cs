namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LogicTreasuryWarRewardCommand : LogicServerCommand
    {
        private LogicLong m_warInstanceId;

        private int m_goldCount;
        private int m_elixirCount;
        private int m_darkElixirCount;

        public void SetDatas(int diamondCount)
        {
        }

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_goldCount = stream.ReadInt();
            this.m_elixirCount = stream.ReadInt();
            this.m_darkElixirCount = stream.ReadInt();
            stream.ReadInt();

            if (stream.ReadBoolean())
            {
                this.m_warInstanceId = stream.ReadLong();
            }

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_goldCount);
            encoder.WriteInt(this.m_elixirCount);
            encoder.WriteInt(this.m_darkElixirCount);
            encoder.WriteInt(0);

            if (this.m_warInstanceId != null)
            {
                encoder.WriteBoolean(true);
                encoder.WriteLong(this.m_warInstanceId);
            }
            else
            {
                encoder.WriteBoolean(false);
            }

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.AddWarReward(this.m_goldCount, this.m_elixirCount, this.m_darkElixirCount, 0, this.m_warInstanceId);
                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.TREASURY_WAR_REWARD;
        }
    }
}