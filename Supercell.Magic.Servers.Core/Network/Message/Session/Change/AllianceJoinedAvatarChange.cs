namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceJoinedAvatarChange : AvatarChange
    {
        public LogicLong AllianceId { get; set; }
        public string AllianceName { get; set; }
        public int AllianceBadgeId { get; set; }
        public int AllianceExpLevel { get; set; }
        public LogicAvatarAllianceRole AllianceRole { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.AllianceId = stream.ReadLong();
            this.AllianceName = stream.ReadString(900000);
            this.AllianceBadgeId = stream.ReadVInt();
            this.AllianceExpLevel = stream.ReadVInt();
            this.AllianceRole = (LogicAvatarAllianceRole) stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AllianceId);
            stream.WriteString(this.AllianceName);
            stream.WriteVInt(this.AllianceBadgeId);
            stream.WriteVInt(this.AllianceExpLevel);
            stream.WriteVInt((int) this.AllianceRole);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetAllianceId(this.AllianceId);
            avatar.SetAllianceName(this.AllianceName);
            avatar.SetAllianceBadgeId(this.AllianceBadgeId);
            avatar.SetAllianceLevel(this.AllianceExpLevel);
            avatar.SetAllianceRole(this.AllianceRole);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.ALLIANCE_JOINED;
        }
    }
}