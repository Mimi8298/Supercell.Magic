namespace Supercell.Magic.Servers.Core.Database.Document
{
    using Supercell.Magic.Titan.DataStream;
    using Supercell.Magic.Titan.Json;
    using Supercell.Magic.Titan.Math;

    public abstract class CouchbaseDocument
    {
        public const string JSON_ATTRIBUTE_ID_HIGH = "id_hi";
        public const string JSON_ATTRIBUTE_ID_LOW = "id_lo";

        public LogicLong Id { get; private set; }

        protected CouchbaseDocument()
        {
        }

        protected CouchbaseDocument(LogicLong id)
        {
            this.Id = id;
        }

        protected abstract void Save(LogicJSONObject jsonObject);
        protected abstract void Load(LogicJSONObject jsonObject);
        protected abstract void Encode(ByteStream stream);
        protected abstract void Decode(ByteStream stream);

        public static string Save(CouchbaseDocument document)
        {
            LogicJSONObject jsonObject = new LogicJSONObject();

            jsonObject.Put(CouchbaseDocument.JSON_ATTRIBUTE_ID_HIGH, new LogicJSONNumber(document.Id.GetHigherInt()));
            jsonObject.Put(CouchbaseDocument.JSON_ATTRIBUTE_ID_LOW, new LogicJSONNumber(document.Id.GetLowerInt()));
            document.Save(jsonObject);

            return LogicJSONParser.CreateJSONString(jsonObject, 256);
        }

        public static T Load<T>(string json) where T : CouchbaseDocument, new()
        {
            LogicJSONObject jsonObject = LogicJSONParser.ParseObject(json);

            T document = new T();

            document.Id = LogicLong.ToLong(jsonObject.GetJSONNumber(CouchbaseDocument.JSON_ATTRIBUTE_ID_HIGH).GetIntValue(),
                jsonObject.GetJSONNumber(CouchbaseDocument.JSON_ATTRIBUTE_ID_LOW).GetIntValue());
            document.Load(jsonObject);

            return document;
        }

        public static void Encode(ByteStream stream, CouchbaseDocument document)
        {
            stream.WriteLong(document.Id);
            document.Encode(stream);
        }

        public static T Decode<T>(ByteStream stream) where T : CouchbaseDocument, new()
        {
            T document = new T();

            document.Id = stream.ReadLong();
            document.Decode(stream);

            return document;
        }
    }
}