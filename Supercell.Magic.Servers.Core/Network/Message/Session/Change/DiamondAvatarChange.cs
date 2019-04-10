namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class DiamondAvatarChange : AvatarChange
    {
        public int Count { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Count = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Count);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetDiamonds(avatar.GetDiamonds() + this.Count);
            avatar.SetFreeDiamonds(avatar.GetFreeDiamonds() + this.Count);

            if (avatar.GetFreeDiamonds() < 0)
                avatar.SetFreeDiamonds(0);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.DIAMOND;
        }
    }
}