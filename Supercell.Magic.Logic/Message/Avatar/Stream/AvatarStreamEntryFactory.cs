namespace Supercell.Magic.Logic.Message.Avatar.Stream
{
    public class AvatarStreamEntryFactory
    {
        public static AvatarStreamEntry CreateStreamEntryByType(AvatarStreamEntryType type)
        {
            switch (type)
            {
                case AvatarStreamEntryType.DEFENDER_BATTLE_REPORT: return new BattleReportStreamEntry(AvatarStreamEntryType.DEFENDER_BATTLE_REPORT);
                case AvatarStreamEntryType.ATTACKER_BATTLE_REPORT: return new BattleReportStreamEntry(AvatarStreamEntryType.ATTACKER_BATTLE_REPORT);
                case AvatarStreamEntryType.JOIN_ALLIANCE_RESPONSE: return new JoinAllianceResponseAvatarStreamEntry();
                case AvatarStreamEntryType.ALLIANCE_INVITATION: return new AllianceInvitationAvatarStreamEntry();
                case AvatarStreamEntryType.ALLIANCE_KICKOUT: return new AllianceKickOutStreamEntry();
                case AvatarStreamEntryType.ALLIANCE_MAIL: return new AllianceMailAvatarStreamEntry();
                case AvatarStreamEntryType.ADMIN_MESSAGE: return new AdminMessageAvatarStreamEntry();

                default: return null;
            }
        }
    }
}