namespace Supercell.Magic.Servers.Core.Network.Message.Core
{
    using Supercell.Magic.Servers.Core.Database.Document;
    using Supercell.Magic.Titan.DataStream;

    public class ScoringSyncMessage : ServerCoreMessage
    {
        public SeasonDocument CurrentSeasonDocument { get; set; }
        public SeasonDocument LastSeasonDocument { get; set; }

        public override void Encode(ByteStream stream)
        {
            CouchbaseDocument.Encode(stream, this.CurrentSeasonDocument);

            if (this.LastSeasonDocument != null)
            {
                stream.WriteBoolean(true);
                CouchbaseDocument.Encode(stream, this.LastSeasonDocument);
            }
            else
            {
                stream.WriteBoolean(false);
            }
        }

        public override void Decode(ByteStream stream)
        {
            this.CurrentSeasonDocument = CouchbaseDocument.Decode<SeasonDocument>(stream);

            if (stream.ReadBoolean())
            {
                this.LastSeasonDocument = CouchbaseDocument.Decode<SeasonDocument>(stream);
            }
        }

        public override ServerMessageType GetMessageType()
        {
            return ServerMessageType.SCORING_SYNC;
        }
    }
}