using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Scribs.Core {

    [DataContract]
    //[KnownType(typeof(Document))]
    public abstract class Entity {
        [BsonId, BsonRepresentation(BsonType.ObjectId), BsonElement("_id"), DataMember, JsonProperty("Id")]
        public string Id { get; set; }

        [BsonElement("Name"), DataMember, JsonProperty]
        public string Name { get; set; }
    }
}
