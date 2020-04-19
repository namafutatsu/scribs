using System.Collections.Generic;
using System.Dynamic;
using MongoDB.Driver;

namespace Scribs.Core.Services {

    public class Factories {
        MongoService mongoService;

        public Factories(MongoService mongoService) {
            this.mongoService = mongoService;
        }

        public Factory<E> Get<E>() where E: Entity {
            return new Factory<E>(mongoService);
        }
    }

    public class Factory<E> where E: Entity {
        private IMongoCollection<E> collection;

        public Factory(MongoService mongoService) {
            if (mongoService != null)
                collection = mongoService.GetCollection<E>(typeof(E).Name);
        }

        public virtual void Create(E entity) {
            collection.InsertOne(entity);
        }

        public virtual E Get(string id) => collection.Find<E>(o => o.Id == id).FirstOrDefault();

        public virtual E GetByName(string name) => collection.Find<E>(o => o.Name == name).FirstOrDefault();

        public List<E> Get() => collection.Find(o => true).ToList();

        public void Update(string id, E entity) => collection.ReplaceOne(o => o.Id == id, entity);

        public void Update(E entity) => Update(entity.Id, entity);

        public void Remove(E entity) => collection.DeleteOne(o => o.Id == entity.Id);

        public void Remove(string id) => collection.DeleteOne(o => o.Id == id);
    }

    public class MongoService {
        MongoClient client;
        IMongoDatabase database;

        public MongoService(IMongoSettings settings) {
            if (settings?.ConnectionString != null) {
                client = new MongoClient(settings.ConnectionString);
                //try {
                    database = client.GetDatabase(settings.DatabaseName);
                //} catch {
                //    database = client.crea
                //}
            }
        }

        public IMongoCollection<E> GetCollection<E>(string collection) {
            return database.GetCollection<E>(collection);
        }

        public void DropDatabase() => client.DropDatabase(database.DatabaseNamespace.DatabaseName);
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
