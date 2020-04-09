using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Scribs.Core.Services;

namespace Scribs.Core.Entities {
    public class User: Entity {
        public string Path => System.IO.Path.Join("users", Name);
        [BsonElement("Mail")] public string Mail { get; set; }
        [BsonElement("Password")] public string Password { get; set; }

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

    //public class UserFactory : Factory<User> {
    //    public UserFactory(MongoService mongoService) : base(mongoService) {
    //    }

    //    public override User Create(User user) {
    //        return base.Create(user);
    //    }
    //}
}
