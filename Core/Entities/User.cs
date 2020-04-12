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
            Key = Utils.CreateGuid();
            Name = name;
        }

        public static User GetByName(string name) {
            // todo
            return new User(name);
        }

        public static User CreateUser(string name, string password, string mail) {
            var user = new User(name) {
                Password = password,
                Mail = mail
            };
            return user;
        }

        public static User DeleteUser(string name, string password, string mail) {
            var user = new User(name) {
                Password = password,
                Mail = mail
            };
            return user;
        }
    }
}
