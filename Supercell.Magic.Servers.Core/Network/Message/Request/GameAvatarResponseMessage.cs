namespace Supercell.Magic.Servers.Core.Network.Message.Request
{
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Titan.DataStream;

    public class GameAvatarResponseMessage : ServerResponseMessage
    {
        public GameDocument Document { get; set; }
        
        public override void Encode(ByteStream stream)
        {
            if (this.Success)
            {
                CouchbaseDocument.Encode(stream, this.Document);
            }
        }

        public override void Decode(ByteStream stream)
        {
            if (this.Success)
            {
                this.Document = CouchbaseDocument.Decode<GameDocument>(stream);
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.GAME_AVATAR_RESPONSE;
        }
    }
}