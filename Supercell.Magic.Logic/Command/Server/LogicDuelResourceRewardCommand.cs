namespace Supercell.Magic.Logic.Command.Server
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Level;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LogicDuelResourceRewardCommand : LogicServerCommand
    {
        private int m_goldCount;
        private int m_elixirCount;
        private int m_bonusGoldCount;
        private int m_bonusElixirCount;

        private LogicLong m_matchId;

        public override void Destruct()
        {
            base.Destruct();
        }

        public override void Decode(ByteStream stream)
        {
            this.m_goldCount = stream.ReadInt();
            this.m_elixirCount = stream.ReadInt();
            this.m_bonusGoldCount = stream.ReadInt();
            this.m_bonusElixirCount = stream.ReadInt();
            stream.ReadInt();
            this.m_matchId = stream.ReadLong();

            base.Decode(stream);
        }

        public override void Encode(ChecksumEncoder encoder)
        {
            encoder.WriteInt(this.m_goldCount);
            encoder.WriteInt(this.m_elixirCount);
            encoder.WriteInt(this.m_bonusGoldCount);
            encoder.WriteInt(this.m_bonusElixirCount);
            encoder.WriteInt(0);
            encoder.WriteLong(this.m_matchId);

            base.Encode(encoder);
        }

        public override int Execute(LogicLevel level)
        {
            LogicClientAvatar playerAvatar = level.GetPlayerAvatar();

            if (playerAvatar != null)
            {
                playerAvatar.AddDuelReward(this.m_goldCount, this.m_elixirCount, this.m_bonusGoldCount, this.m_bonusElixirCount, this.m_matchId);
                return 0;
            }

            return -1;
        }

        public override LogicCommandType GetCommandType()
        {
            return LogicCommandType.DUEL_RESOURCE_REWARD;
        }

        public void SetDatas(int goldCount, int elixirCount, int bonusGoldCount, int bonusElixirCount, LogicLong matchId)
        {
            this.m_goldCount = goldCount;
            this.m_elixirCount = elixirCount;
            this.m_bonusGoldCount = bonusGoldCount;
            this.m_bonusElixirCount = bonusElixirCount;
            this.m_matchId = matchId;
        }
    }
}