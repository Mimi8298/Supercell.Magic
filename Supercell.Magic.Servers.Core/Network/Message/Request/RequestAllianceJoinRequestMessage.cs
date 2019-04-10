namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Logic.Avatar;

    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class RequestAllianceJoinRequestMessage : ServerRequestMessage
    {
        public LogicLong AllianceId { get; set; }
        public LogicClientAvatar Avatar { get; set; }

        public string Message { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteString(this.Message);
            stream.WriteLong(this.AllianceId);
            this.Avatar.Encode(stream);
        }

        public override void Decode(ByteStream stream)
        {
            this.Message = stream.ReadString(900000);
            this.AllianceId = stream.ReadLong();
            this.Avatar = new LogicClientAvatar();
            this.Avatar.Decode(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.REQUEST_ALLIANCE_JOIN_REQUEST;
        }
    }
}