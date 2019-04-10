namespace Supercell.Magic.Servers.Core.Network.Message.Session.Change
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class CommodityCountAvatarChange : AvatarChange
    {
        public int Type { get; set; }
        public LogicData Data { get; set; }
        public int Count { get; set; }

        public override void Decode(ByteStream stream)
        {
            this.Type = stream.ReadVInt();
            this.Data = ByteStreamHelper.ReadDataReference(stream);
            this.Count = stream.ReadVInt();
        }

        public override void Encode(ByteStream stream)
        {
            stream.WriteVInt(this.Type);
            ByteStreamHelper.WriteDataReference(stream, this.Data);
            stream.WriteVInt(this.Count);
        }

        public override void ApplyAvatarChange(LogicClientAvatar avatar)
        {
            avatar.SetCommodityCount(this.Type, this.Data, this.Count);
        }

        public override void ApplyAvatarChange(AllianceMemberEntry memberEntry)
        {
        }

        public override AvatarChangeType GetAvatarChangeType()
        {
            return AvatarChangeType.COMMODITY_COUNT;
        }
    }
}