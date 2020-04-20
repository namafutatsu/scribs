using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
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
            if (mongoService != null) {
                collection = mongoService.GetCollection<E>(typeof(E).Name);
            }
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

        public virtual Task CreateAsync(E entity) => collection.InsertOneAsync(entity);

        public virtual async Task<E> GetAsync(Expression<Func<E, bool>> get) {
            using (var cursor = await collection.FindAsync(get, new FindOptions<E, E> { Limit = 1 })) {
                return await cursor.SingleOrDefaultAsync();
            }
        }

        public virtual Task<E> GetAsync(string id) => GetAsync(o => o.Id == id);

        public virtual Task<E> GetByNameAsync(string name) => GetAsync(o => o.Name == name);

        public Task UpdateAsync(string id, E entity) => collection.ReplaceOneAsync(o => o.Id == id, entity);

        public Task UpdateAsync(E entity) => UpdateAsync(entity.Id, entity);

        public Task RemoveAsync(E entity) => collection.DeleteOneAsync(o => o.Id == entity.Id);

        public Task RemoveAsync(string id) => collection.DeleteOneAsync(o => o.Id == id);
    }

    public class MongoService {
        MongoClient client;
        IMongoDatabase database;

        public MongoService(IMongoSettings settings) {
            if (settings?.ConnectionString != null) {
                client = new MongoClient(settings.ConnectionString);
                database = client.GetDatabase(settings.DatabaseName);
            }
        }

        public static bool NeedsNameIndex(string name, out bool uniq) {
            uniq = false;
            switch (name) {
                case "User":
                    uniq = true;
                    return true;
                case "Document":
                    return true;
                default:
                    return false;
            }
        }

        public IMongoCollection<E> GetCollection<E>(string name) where E: Entity {
            var filter = new BsonDocument("name", name);
            bool nameIndex = NeedsNameIndex(name, out bool uniqNameIndex);
            if (nameIndex) {
                var options = new ListCollectionNamesOptions { Filter = filter };
                nameIndex = !database.ListCollectionNames(options).Any();
            }
            var collection = database.GetCollection<E>(name);
            if (nameIndex) {
                var keys = Builders<E>.IndexKeys.Ascending(_ => _.Name);
                var indexOptions = new CreateIndexOptions { Unique = true };
                var model = new CreateIndexModel<E>(keys, indexOptions);
                collection.Indexes.CreateOne(model);
            }
            return collection;
        }

        public void DropDatabase() => client.DropDatabase(database.DatabaseNamespace.DatabaseName);
    }


    public class Index {
        public string Field { get; }
        public bool unique { get; }

        public Index(string field, bool unique) {
            Field = field;
            this.unique = unique;
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
