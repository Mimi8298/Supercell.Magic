namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class WarPreferenceAvatarChange : AvatarChange
    {
        public int Preference { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Preference = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Preference);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetWarPreference(this.Preference);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
            memberEntry.SetWarPreference(this.Preference);
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.WAR_PREFERENCE;
        }
    }
}