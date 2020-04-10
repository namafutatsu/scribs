using System.Collections.Generic;
using MongoDB.Driver;

namespace Scribs.Core.Services {

    public class Factory<E> where E: Entity {
        private IMongoCollection<E> collection;

        public Factory(MongoService mongoService) {
            if (mongoService != null)
                collection = mongoService.GetCollection<E>(typeof(E).Name);
        }

        public virtual E Create(E entity) {
            collection.InsertOne(entity);
            return entity;
        }

        public virtual E Get(string id) => collection.Find<E>(o => o.Key == id).FirstOrDefault();

        public virtual E GetByName(string name) => collection.Find<E>(o => o.Name == name).FirstOrDefault();

        public List<E> Get() => collection.Find(o => true).ToList();

        public void Update(string id, E entity) => collection.ReplaceOne(o => o.Key == id, entity);

        public void Update(E entity) => Update(entity.Key, entity);

        public void Remove(E entity) => collection.DeleteOne(o => o.Key == entity.Key);

        public void Remove(string id) => collection.DeleteOne(o => o.Key == id);
    }

    public class MongoService {
        IMongoDatabase database;

        public MongoService(IMongoSettings settings) {
            var client = new MongoClient(settings.ConnectionString);
            database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<E> GetCollection<E>(string collection) {
            return database.GetCollection<E>(collection);
        }
    }

    public class MongoSettings : IMongoSettings {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IMongoSettings {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
