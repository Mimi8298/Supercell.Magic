namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceRequestAllianceUnitsMessage : ServerAccountMessage
    {
        public LogicLong MemberId { get; set; }
        public string Message { get; set; }
        public int CastleUpgradeLevel { get; set; }
        public int CastleUsedCapacity { get; set; }
        public int CastleTotalCapacity { get; set; }
        public int CastleSpellUsedCapacity { get; set; }
        public int CastleSpellTotalCapacity { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.MemberId);
            stream.WriteString(this.Message);
            stream.WriteVInt(this.CastleUpgradeLevel);
            stream.WriteVInt(this.CastleUsedCapacity);
            stream.WriteVInt(this.CastleTotalCapacity);
            stream.WriteVInt(this.CastleSpellUsedCapacity);
            stream.WriteVInt(this.CastleSpellTotalCapacity);
        }

        public override void Decode(ByteStream stream)
        {
            this.MemberId = stream.ReadLong();
            this.Message = stream.ReadString(900000);
            this.CastleUpgradeLevel = stream.ReadVInt();
            this.CastleUsedCapacity = stream.ReadVInt();
            this.CastleTotalCapacity = stream.ReadVInt();
            this.CastleSpellUsedCapacity = stream.ReadVInt();
            this.CastleSpellTotalCapacity = stream.ReadVInt();
        }
        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_REQUEST_ALLIANCE_UNITS;
        }
    }
}