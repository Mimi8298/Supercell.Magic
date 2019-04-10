namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameJoinAllianceRequestMessage : ServerRequestMessage
    {
        public LogicLong AccountId { get; set; }
        public LogicLong AllianceId { get; set; }
        public LogicLong AvatarStreamId { get; set; }

        public bool Created { get; set; }
        public bool Invited { get; set; }
        
        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AccountId);
            stream.WriteLong(this.AllianceId);
            stream.WriteBoolean(this.Created);
            stream.WriteBoolean(this.Invited);
            stream.WriteBoolean(this.AvatarStreamId != null);

            if (this.AvatarStreamId != null)
            {
                stream.WriteLong(this.AvatarStreamId);
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.AccountId = stream.ReadLong();
            this.AllianceId = stream.ReadLong();
            this.Created = stream.ReadBoolean();
            this.Invited = stream.ReadBoolean();

            if (stream.ReadBoolean())
            {
                this.AvatarStreamId = stream.ReadLong();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_JOIN_ALLIANCE_REQUEST;
        }
    }
}