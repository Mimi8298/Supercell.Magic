namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class ExpLevelAvatarChange : AvatarChange
    {
        public int Points { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Points = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Points);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetExpPoints(this.Points);
            avatar.SetExpLevel(avatar.GetExpLevel() + 1);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
            memberEntry.SetExpLevel(memberEntry.GetExpLevel() + 1);
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.EXP_LEVEL;
        }
    }
}