namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class ScoreAvatarChange : AvatarChange
    {
        public int ScoreGain { get; set; }
        public bool Attacker { get; set; }

        public LogicLeagueData PrevLeagueData { get; set; }
        public LogicLeagueData LeagueData { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.ScoreGain = stream.ReadVInt();
            this.Attacker = stream.ReadBoolean();

            this.PrevLeagueData = (LogicLeagueData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.LEAGUE);
            this.LeagueData = (LogicLeagueData) ByteStreamHelper.ReadDataReference(stream, LogicDataType.LEAGUE);
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.ScoreGain);
            stream.WriteBoolean(this.Attacker);

            ByteStreamHelper.WriteDataReference(stream, this.PrevLeagueData);
            ByteStreamHelper.WriteDataReference(stream, this.LeagueData);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetScore(LogicMath.Max(avatar.GetScore() + this.ScoreGain, 0));
            avatar.SetLeagueType(this.LeagueData.GetInstanceID());

            if (this.PrevLeagueData != null)
            {
                if (this.Attacker)
                {
                    if (this.ScoreGain < 0)
                    {
                        avatar.SetAttackLoseCount(avatar.GetAttackLoseCount() + 1);
                    }
                    else
                    {
                        avatar.SetAttackWinCount(avatar.GetAttackWinCount() + 1);
                    }
                }
                else
                {
                    if (this.ScoreGain < 0)
                    {
                        avatar.SetDefenseLoseCount(avatar.GetDefenseLoseCount() + 1);
                    }
                    else
                    {
                        avatar.SetDefenseWinCount(avatar.GetDefenseWinCount() + 1);
                    }
                }
            }
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
            memberEntry.SetScore(LogicMath.Max(memberEntry.GetScore() + this.ScoreGain, 0));
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.SCORE;
        }
    }
}