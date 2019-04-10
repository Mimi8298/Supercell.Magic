namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class TownHallV2LevelAvatarChange : AvatarChange
    {
        public int Level { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Level = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Level);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetVillage2TownHallLevel(this.Level);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.TOWN_HALL_V2_LEVEL;
        }
    }
}