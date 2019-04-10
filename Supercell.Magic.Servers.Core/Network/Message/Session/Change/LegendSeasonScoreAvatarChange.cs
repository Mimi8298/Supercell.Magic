namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.League.Entry;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class LegendSeasonScoreAvatarChange : AvatarChange
    {
        public LogicLegendSeasonEntry Entry { get; set; }

        public int ScoreChange { get; set; }
        public bool BestSeason { get; set; }
        public int VillageType { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Entry = new LogicLegendSeasonEntry();
            this.Entry.Decode(stream);
            this.ScoreChange = stream.ReadVInt();
            this.BestSeason = stream.ReadBoolean();
            this.VillageType = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            this.Entry.Encode(stream);

            stream.WriteVInt(this.ScoreChange);
            stream.WriteBoolean(this.BestSeason);
            stream.WriteVInt(this.VillageType);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            LogicLegendSeasonEntry legendSeasonEntry = this.VillageType == 1 ? avatar.GetLegendSeasonEntryVillage2() : avatar.GetLegendSeasonEntry();

            if (legendSeasonEntry.GetLastSeasonState() != this.Entry.GetLastSeasonState())
            {
                if (this.VillageType == 1)
                {
                    avatar.SetDuelScore(avatar.GetDuelScore() - this.ScoreChange);
                    avatar.SetLegendaryScore(avatar.GetLegendaryScoreVillage2() + this.ScoreChange);
                }
                else
                {
                    avatar.SetScore(avatar.GetScore() - this.ScoreChange);
                    avatar.SetLegendaryScore(avatar.GetLegendaryScore() + this.ScoreChange);
                }

                legendSeasonEntry.SetLastSeasonState(this.Entry.GetLastSeasonState());
                legendSeasonEntry.SetLastSeasonDate(this.Entry.GetLastSeasonYear(), this.Entry.GetLastSeasonMonth());
                legendSeasonEntry.SetLastSeasonRank(this.Entry.GetLastSeasonRank());
                legendSeasonEntry.SetLastSeasonScore(this.Entry.GetLastSeasonScore());

                if (this.BestSeason)
                {
                    legendSeasonEntry.SetBestSeasonState(this.Entry.GetBestSeasonState());
                    legendSeasonEntry.SetBestSeasonDate(this.Entry.GetBestSeasonYear(), this.Entry.GetBestSeasonMonth());
                    legendSeasonEntry.SetBestSeasonRank(this.Entry.GetBestSeasonRank());
                    legendSeasonEntry.SetBestSeasonScore(this.Entry.GetBestSeasonScore());
                }
            }
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
            if (this.VillageType == 1)
                memberEntry.SetDuelScore(memberEntry.GetDuelScore() - this.ScoreChange);
            else
                memberEntry.SetScore(memberEntry.GetScore() - this.ScoreChange);
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.LEGEND_SEASON_SCORE;
        }
    }
}