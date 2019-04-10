namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Titan.DataStream;

    public class GameCreateAllianceInvitationResponseMessage : ServerResponseMessage
    {
        public Reason ErrorReason { get; set; }
        
        public override void Encode(ByteStream stream)
        {
            if (!this.Success)
            {
                stream.WriteVInt((int) this.ErrorReason);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (!this.Success)
            {
                this.ErrorReason = (Reason) stream.ReadVInt();
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_CREATE_ALLIANCE_INVITATION_RESPONSE;
        }

        public enum Reason
        {
            GENERIC,
            NO_CASTLE,
            ALREADY_IN_ALLIANCE,
            ALREADY_HAS_AN_INVITE,
            HAS_TOO_MANY_INVITES,
        }
    }
}