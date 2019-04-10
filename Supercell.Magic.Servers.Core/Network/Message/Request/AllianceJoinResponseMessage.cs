namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceJoinResponseMessage : ServerResponseMessage
    {
        public LogicLong AllianceId { get; set; }
        public string AllianceName { get; set; }
        public int AllianceLevel { get; set; }
        public int AllianceBadgeId { get; set; }
        public bool Created { get; set; }

        public Reason ErrorReason { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                stream.WriteLong(this.AllianceId);
                stream.WriteString(this.AllianceName);
                stream.WriteVInt(this.AllianceLevel);
                stream.WriteVInt(this.AllianceBadgeId);
                stream.WriteBoolean(this.Created);
            }
            else
            {
                stream.WriteVInt((int)this.ErrorReason);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.AllianceId = stream.ReadLong();
                this.AllianceName = stream.ReadString(900000);
                this.AllianceLevel = stream.ReadVInt();
                this.AllianceBadgeId = stream.ReadVInt();
                this.Created = stream.ReadBoolean();
            }
            else
            {
                this.ErrorReason = (Reason)stream.ReadVInt();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_JOIN_RESPONSE;
        }

        public enum Reason
        {
            GENERIC,
            FULL,
            CLOSED,
            SCORE,
            BANNED,
        }
    }
}