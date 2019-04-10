namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceLeftAvatarChange : AvatarChange
    {
        public override void Decode(ByteStream stream)
        {
        }

        public override void Encode(ByteStream stream)
        {
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetAllianceId(null);
            avatar.SetAllianceName(string.Empty);
            avatar.SetAllianceBadgeId(-1);
            avatar.SetAllianceLevel(-1);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.ALLIANCE_LEFT;
        }
    }
}