namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Titan.DataStream;

    public static class AvatarChangeFactory
    {
        public static AvatarChange Decode(ByteStream stream)
        {
            AvatarChange avatarChange = AvatarChangeFactory.CreateByType((AvatarChangeType) stream.ReadVInt());
            avatarChange.Decode(stream);
            return avatarChange;
        }

        public static void Encode(ByteStream stream, AvatarChange avatarChange)
        {
            stream.WriteVInt((int) avatarChange.GetAvatarChangeType());
            avatarChange.Encode(stream);
        }

        private static AvatarChange CreateByType(AvatarChangeType type)
        {
            switch (type)
            {
                case AvatarChangeType.DIAMOND: return new DiamondAvatarChange();
                case AvatarChangeType.COMMODITY_COUNT: return new CommodityCountAvatarChange();
                case AvatarChangeType.WAR_PREFERENCE: return new WarPreferenceAvatarChange();
                case AvatarChangeType.EXP_POINTS: return new ExpPointsAvatarChange();
                case AvatarChangeType.EXP_LEVEL: return new ExpLevelAvatarChange();
                case AvatarChangeType.ALLIANCE_JOINED: return new AllianceJoinedAvatarChange();
                case AvatarChangeType.ALLIANCE_LEFT: return new AllianceLeftAvatarChange();
                case AvatarChangeType.ALLIANCE_LEVEL: return new AllianceLevelAvatarChange();
                case AvatarChangeType.ALLIANCE_UNIT_REMOVED: return new AllianceUnitRemovedAvatarChange();
                case AvatarChangeType.ALLIANCE_UNIT_ADDED: return new AllianceUnitAddedAvatarChange();
                case AvatarChangeType.ALLIANCE_UNIT_COUNT: return new AllianceUnitCountAvatarChange();
                case AvatarChangeType.ALLIANCE_CASTLE_LEVEL: return new AllianceCastleLevelAvatarChange();
                case AvatarChangeType.TOWN_HALL_LEVEL: return new TownHallLevelAvatarChange();
                case AvatarChangeType.TOWN_HALL_V2_LEVEL: return new TownHallV2LevelAvatarChange();
                case AvatarChangeType.LEGEND_SEASON_SCORE: return new LegendSeasonScoreAvatarChange();
                case AvatarChangeType.SCORE: return new ScoreAvatarChange();
                case AvatarChangeType.DUEL_SCORE: return new DuelScoreAvatarChange();
                case AvatarChangeType.LEAGUE: return new LeagueAvatarChange();
                case AvatarChangeType.ATTACK_SHIELD_REDUCE_COUNTER: return new AttackShieldReduceCounterAvatarChange();
                case AvatarChangeType.DEFENSE_VILLAGE_GUARD_COUNTER: return new DefenseVillageGuardCounterAvatarChange();
                case AvatarChangeType.NAME: return new NameAvatarChange();

                default:
                    Logging.Error("Unknown avatar change type: " + type);
                    return null;
            }
        }
    }
}