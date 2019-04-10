namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceUnitDonateResponseMessage : ServerAccountMessage
    {
        public LogicLong MemberId { get; set; }
        public LogicLong StreamId { get; set; }
        public LogicCombatItemData Data { get; set; }

        public int UpgradeLevel { get; set; }
        public bool QuickDonate { get; set; }
        public bool Success { get; set; }

        public string MemberName { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.MemberId);
            stream.WriteLong(this.StreamId);

            ByteStreamHelper.WriteDataReference(stream, this.Data);

            stream.WriteVInt(this.UpgradeLevel);
            stream.WriteBoolean(this.QuickDonate);
            stream.WriteBoolean(this.Success);
            stream.WriteString(this.MemberName);
        }

        public override void Decode(ByteStream stream)
        {
            this.MemberId = stream.ReadLong();
            this.StreamId = stream.ReadLong();
            this.Data = (LogicCombatItemData) ByteStreamHelper.ReadDataReference(stream);
            this.UpgradeLevel = stream.ReadVInt();
            this.QuickDonate = stream.ReadBoolean();
            this.Success = stream.ReadBoolean();
            this.MemberName = stream.ReadString(900000);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_UNIT_DONATE_RESPONSE;
        }
    }
}