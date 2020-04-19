using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scribs.Core.Entities {

    [DataContract]
    public class User: Entity {
        [BsonElement("Mail")]
        [DataMember]
        public string Mail { get; set; }
        [BsonElement("Password")]
        public string Password { get; set; }
        public string Path => System.IO.Path.Join("users", Name);

        public User(string name) {
            Id = Utils.CreateGuid();
            Name = name;
        }
    }
}
