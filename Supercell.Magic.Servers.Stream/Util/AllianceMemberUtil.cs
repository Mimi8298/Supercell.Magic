namespace Supercell.Magic.Servers.Stream.Util
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Servers.Stream.Logic;
    using Supercell.Magic.Titan.Math;

    public class AllianceMemberUtil
    {
        public static AllianceMemberEntry GetAllianceMemberEntryFromAvatar(LogicClientAvatar entry)
        {
            AllianceMemberEntry allianceMemberEntry = new AllianceMemberEntry();
            AllianceMemberUtil.SetLogicClientAvatarToAllianceMemberEntry(entry, allianceMemberEntry);
            return allianceMemberEntry;
        }

        public static void SetLogicClientAvatarToAllianceMemberEntry(LogicClientAvatar avatar, AllianceMemberEntry allianceMemberEntry, Alliance alliance = null)
        {
            bool updateScoring = avatar.GetScore() != avatar.GetScore();

            allianceMemberEntry.SetAvatarId(avatar.GetId());
            allianceMemberEntry.SetHomeId(avatar.GetCurrentHomeId());
            allianceMemberEntry.SetName(avatar.GetName());
            allianceMemberEntry.SetExpLevel(avatar.GetExpLevel());
            allianceMemberEntry.SetLeagueType(avatar.GetLeagueType());
            allianceMemberEntry.SetScore(avatar.GetScore());
            allianceMemberEntry.SetDuelScore(avatar.GetDuelScore());
            allianceMemberEntry.SetWarPreference(avatar.GetWarPreference());

            if (alliance != null)
            {
                if (updateScoring)
                    alliance.UpdateScoring();
            }
        }
    }
}