namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class CreateAllianceResponseMessage : ServerResponseMessage
    {
        public LogicLong AllianceId { get; set; }
        public Reason ErrorReason { get; set; }

        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                stream.WriteLong(this.AllianceId);
            }
            else
            {
                stream.WriteVInt((int) this.ErrorReason);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.AllianceId = stream.ReadLong();
            }
            else
            {
                this.ErrorReason = (Reason)stream.ReadVInt();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.CREATE_ALLIANCE_RESPONSE;
        }

        public enum Reason
        {
            GENERIC,
            INVALID_NAME,
            INVALID_DESCRIPTION,
            NAME_TOO_SHORT,
            NAME_TOO_LONG
        }
    }
}