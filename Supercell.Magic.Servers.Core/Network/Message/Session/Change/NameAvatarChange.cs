namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class NameAvatarChange : AvatarChange
    {
        public string Name { get; set; }
        public int NameChangeState { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Name = stream.ReadString(900000);
            this.NameChangeState = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteString(this.Name);
            stream.WriteVInt(this.NameChangeState);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetName(this.Name);
            avatar.SetNameSetByUser(true);
            avatar.SetNameChangeState(this.NameChangeState);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
            memberEntry.SetName(this.Name);
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.NAME;
        }
    }
}