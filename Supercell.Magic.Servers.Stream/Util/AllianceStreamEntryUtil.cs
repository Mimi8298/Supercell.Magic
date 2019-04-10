namespace Supercell.Magic.Servers.Stream.Util
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Logic.Message.Alliance.Stream;

    public class AllianceStreamEntryUtil
    {
        public static void SetSenderInfo(StreamEntry entry, AllianceMemberEntry member)
        {
            entry.SetSenderAvatarId(member.GetAvatarId());
            entry.SetSenderHomeId(member.GetHomeId());
            entry.SetSenderName(member.GetName());
            entry.SetSenderLevel(member.GetExpLevel());
            entry.SetSenderLeagueType(member.GetLeagueType());
            entry.SetSenderRole(member.GetAllianceRole());
        }

        public static void SetSenderInfo(StreamEntry entry, LogicClientAvatar avatar)
        {
            entry.SetSenderAvatarId(avatar.GetId());
            entry.SetSenderHomeId(avatar.GetCurrentHomeId());
            entry.SetSenderName(avatar.GetName());
            entry.SetSenderLevel(avatar.GetExpLevel());
            entry.SetSenderLeagueType(avatar.GetLeagueType());
            entry.SetSenderRole(avatar.GetAllianceRole());
        }
    }
}