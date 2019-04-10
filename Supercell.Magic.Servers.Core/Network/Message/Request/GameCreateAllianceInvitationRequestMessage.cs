namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Logic.Message.Avatar.Stream;
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Math;

    public class GameCreateAllianceInvitationRequestMessage : ServerRequestMessage
    {
        public LogicLong AccountId { get; set; }
        public AllianceInvitationAvatarStreamEntry Entry { get; set; }

        public override void Encode(ByteStream stream)
        {
            stream.WriteLong(this.AccountId);
            this.Entry.Encode(stream);
        }

        public override void Decode(ByteStream stream)
        {
            this.AccountId = stream.ReadLong();
            this.Entry = new AllianceInvitationAvatarStreamEntry();
            this.Entry.Decode(stream);
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_CREATE_ALLIANCE_INVITATION_REQUEST;
        }
    }
}