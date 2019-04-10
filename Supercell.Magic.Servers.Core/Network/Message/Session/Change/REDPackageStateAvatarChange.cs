namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class REDPackageStateAvatarChange : AvatarChange
    {
        public int State { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.State = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.State);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetRedPackageState(this.State);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.RED_PACKAGE_STATE_CHANGED;
        }
    }
}