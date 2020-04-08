using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Scribs.Core {

    public class Entity {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        [JsonProperty("Key")]
        public string Key { get; protected set; }

        [BsonElement("Name")]
        [JsonProperty("Name")]
        public string Name { get; protected set; }
    }
}
