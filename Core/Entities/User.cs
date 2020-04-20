using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scribs.Core.Entities {

    [DataContract]
    public class User: Entity {
        [BsonElement]
        public string FirstName { get; set; }
        [BsonElement]
        public string LastName { get; set; }
        [BsonElement]
        public string Mail { get; set; }
        [BsonElement]
        public string Password { get; set; }
        [BsonElement]
        public string ConfirmPassword { get; set; }
        [BsonElement]
        public string Secret { get; set; }
        public string Path => System.IO.Path.Join("users", Name);

        public User(string name) {
            Id = Utils.CreateGuid();
            Name = name;
        }
    }
}
