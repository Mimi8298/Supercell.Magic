namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Logic.Avatar;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class AllianceJoinRequestMessage : ServerRequestMessage
    {
        public LogicLong AllianceId { get; set; }
        public LogicClientAvatar Avatar { get; set; }

        public bool Created { get; set; }
        public bool Invited { get; set; }
        
        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AllianceId);
            stream.WriteBoolean(this.Created);
            stream.WriteBoolean(this.Invited);
            
            this.Avatar.Encode(stream);
        }

        public override void Decode(ByteStream stream)
        {
            this.AllianceId = stream.ReadLong();
            this.Created = stream.ReadBoolean();
            this.Invited = stream.ReadBoolean();

            this.Avatar = new LogicClientAvatar();
            this.Avatar.Decode(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.ALLIANCE_JOIN_REQUEST;
        }
    }
}