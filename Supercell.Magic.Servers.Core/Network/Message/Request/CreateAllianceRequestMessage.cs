namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Logic.Data;
    using Supercell.Magic.Logic.Helper;
    using Supercell.Magic.Logic.Message.Alliance;
    using Supercell.Magic.Titan.DataStream;

    public class CreateAllianceRequestMessage : ServerRequestMessage
    {
        public string AllianceName { get; set; }
        public string AllianceDescription { get; set; }

        public AllianceType AllianceType { get; set; }
        public int AllianceBadgeId { get; set; }
        public int RequiredScore { get; set; }
        public int RequiredDuelScore { get; set; }
        public int WarFrequency { get; set; }

        public bool PublicWarLog { get; set; }
        public bool ArrangedWarEnabled { get; set; }

        public LogicData OriginData { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteString(this.AllianceName);
            stream.WriteString(this.AllianceDescription);
            stream.WriteVInt((int) this.AllianceType);
            stream.WriteVInt(this.AllianceBadgeId);
            stream.WriteVInt(this.RequiredScore);
            stream.WriteVInt(this.RequiredDuelScore);
            stream.WriteVInt(this.WarFrequency);
            stream.WriteBoolean(this.PublicWarLog);
            stream.WriteBoolean(this.ArrangedWarEnabled);
            ByteStreamHelper.WriteDataReference(stream, this.OriginData);
        }

        public override void Decode(ByteStream stream)
        {
            this.AllianceName = stream.ReadString(900000);
            this.AllianceDescription = stream.ReadString(900000);
            this.AllianceType = (AllianceType) stream.ReadVInt();
            this.AllianceBadgeId = stream.ReadVInt();
            this.RequiredScore = stream.ReadVInt();
            this.RequiredDuelScore = stream.ReadVInt();
            this.WarFrequency = stream.ReadVInt();
            this.PublicWarLog = stream.ReadBoolean();
            this.ArrangedWarEnabled = stream.ReadBoolean();
            this.OriginData = ByteStreamHelper.ReadDataReference(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CREATE_ALLIANCE_REQUEST;
        }
    }
}