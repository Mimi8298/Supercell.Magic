namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceLeavedMessage : ServerAccountMessage
    {
        public LogicLong AllianceId { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AllianceId);
        }

        public override void Decode(ByteStream stream)
        {
            this.AllianceId = stream.ReadLong();
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_LEAVED;
        }
    }
}