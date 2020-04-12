using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Scribs.Core.Entities;

namespace Scribs.Core {

    [DataContract]
    //[KnownType(typeof(Document))]
    public abstract class Entity {
        [BsonId, BsonRepresentation(BsonType.ObjectId), BsonElement("_id")]
        [JsonProperty("Key")]
        [DataMember]
        public string Key { get; set; }

        [BsonElement("Name")]
        [JsonProperty("Name")]
        [DataMember]
        public string Name { get; protected set; }
    }
}
