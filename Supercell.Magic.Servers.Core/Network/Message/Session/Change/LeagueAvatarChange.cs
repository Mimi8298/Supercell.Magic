namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.League.Entry;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class LeagueAvatarChange : AvatarChange
    {
        public int LeagueType { get; set; }
        public LogicLong LeagueInstanceId { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.LeagueType = stream.ReadVInt();

            if (stream.ReadBoolean())
            {
                this.LeagueInstanceId = stream.ReadLong();
            }
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.LeagueType);

            if (this.LeagueInstanceId != null)
            {
                stream.WriteBoolean(true);
                stream.WriteLong(this.LeagueInstanceId);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetLeagueType(this.LeagueType);

            if (this.LeagueType != 0)
            {
                avatar.SetLeagueInstanceId(this.LeagueInstanceId);
            }
            else
            {
                avatar.SetLeagueInstanceId(null);
                avatar.SetAttackWinCount(0);
                avatar.SetAttackLoseCount(0);
                avatar.SetDefenseWinCount(0);
                avatar.SetDefenseLoseCount(0);
            }
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
            memberEntry.SetLeagueType(this.LeagueType);
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.LEAGUE;
        }
    }
}