using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scribs.Core.Entities {

    [DataContract]
    public class User: Entity {
        [BsonElement, DataMember]
        public string FirstName { get; set; }
        [BsonElement, DataMember]
        public string LastName { get; set; }
        [BsonElement, DataMember]
        public string Mail { get; set; }
        [BsonElement, DataMember]
        public string Password { get; set; }
        [BsonElement, DataMember]
        public string Secret { get; set; }
        public string Path => System.IO.Path.Join("users", Name);

        public User(string name) {
            Id = Utils.CreateId();
            Name = name;
        }
    }
}
