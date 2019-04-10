namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class AllianceUnitAddedAvatarChange : AvatarChange
    {
        public LogicCombatItemData Data { get; set; }
        public int UpgradeLevel { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Data = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream);
            this.UpgradeLevel = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            ByteStreamHelper.WriteDataReference(stream, this.Data);
            stream.WriteVInt(this.UpgradeLevel);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.AddAllianceUnit(this.Data, this.UpgradeLevel);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.ALLIANCE_UNIT_ADDED;
        }
    }
}