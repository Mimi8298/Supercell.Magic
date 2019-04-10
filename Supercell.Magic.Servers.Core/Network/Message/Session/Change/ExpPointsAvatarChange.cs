namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class ExpPointsAvatarChange : AvatarChange
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
            avatar.SetExpPoints(avatar.GetExpPoints() + this.Points);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.EXP_POINTS;
        }
    }
}