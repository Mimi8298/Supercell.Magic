namespace Supercell.Magic.Servers.Core.Network.Message.Account
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceCreateMailMessage : ServerAccountMessage
    {
        public LogicLong MemberId { get; set; }
        public string Message { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.MemberId);
            stream.WriteString(this.Message);
        }

        public override void Decode(ByteStream stream)
        {
            this.MemberId = stream.ReadLong();
            this.Message = stream.ReadString(900000);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_CREATE_MAIL;
        }
    }
}