namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public abstract class AvatarChange
    {
        public delegate void AvatarChangeExecuted(LogicClientAvatar playerAvatar);
        
        public abstract void Decode(ByteStream stream);
        public abstract void Encode(ByteStream stream);
        public abstract void ApplyAvatarChange(LogicClientAvatar avatar);
        public abstract void ApplyAvatarChange(AllianceMemberEntry memberEntry);

        public abstract AvatarChangeType GetAvatarChangeType();
    }

    public enum AvatarChangeType
    {
        DIAMOND,
        COMMODITY_COUNT,
        WAR_PREFERENCE,
        EXP_POINTS,
        EXP_LEVEL,
        ALLIANCE_JOINED,
        ALLIANCE_LEFT,
        ALLIANCE_LEVEL,
        ALLIANCE_UNIT_REMOVED,
        ALLIANCE_UNIT_ADDED,
        ALLIANCE_UNIT_COUNT,
        ALLIANCE_CASTLE_LEVEL,
        TOWN_HALL_LEVEL,
        TOWN_HALL_V2_LEVEL,
        LEGEND_SEASON_SCORE,
        SCORE,
        DUEL_SCORE,
        LEAGUE,
        ATTACK_SHIELD_REDUCE_COUNTER,
        DEFENSE_VILLAGE_GUARD_COUNTER,
        RED_PACKAGE_STATE_CHANGED,
        NAME
    }
}