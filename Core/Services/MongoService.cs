using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;

namespace Scribs.Core.Services {

    public class Factories {
        MongoService mongo;
        ClockService clock;

        public Factories(MongoService mongo, ClockService clock) {
            this.mongo = mongo;
            this.clock = clock;
        }

        public Factory<E> Get<E>() where E: Entity {
            return new Factory<E>(mongo, clock);
        }
    }

    public class Factory<E> where E: Entity {
        private IMongoCollection<E> collection;
        private ClockService clock;

        public Factory(MongoService mongo, ClockService clock) {
            if (mongo != null) {
                collection = mongo.GetCollection<E>(typeof(E).Name);
            }
            this.clock = clock;
        }

        public virtual Task CreateAsync(E entity) {
            entity.CTime = entity.MTime = clock.GetNow();
            return collection.InsertOneAsync(entity);
        }

        public virtual async Task<E> GetAsync(Expression<Func<E, bool>> get) {
            using (var cursor = await collection.FindAsync(get, new FindOptions<E, E> { Limit = 1 })) {
                var entity = await cursor.SingleOrDefaultAsync();
                if (entity != null && entity.DTime.HasValue)
                    return null;
                return entity;
            }
        }

        public virtual Task<E> GetAsync(string id) => GetAsync(o => o.Id == id);

        public virtual Task<E> GetByNameAsync(string name) => GetAsync(o => o.Name == name);

        public virtual async Task<IList<E>> GetAsync(IEnumerable<string> ids) {
            var definition = new FilterDefinitionBuilder<E>();
            var filter = definition.In(x => x.Id, ids);
            using (var cursor = await collection.FindAsync(filter)) {
                return cursor.ToList();
            }
        }

        public Task UpdateAsync(string id, E entity) {
            entity.MTime = clock.GetNow();
            return collection.ReplaceOneAsync(o => o.Id == id, entity);
        }

        public Task UpdateAsync(E entity) {
            entity.MTime = clock.GetNow();
            return UpdateAsync(entity.Id, entity);
        }

        public Task RemoveAsync(E entity) {
            entity.DTime = clock.GetNow();
            return UpdateAsync(entity.Id, entity);
        }

        public async Task RemoveAsync(string id) {
            var entity = await GetAsync(id);
            if (entity != null) {
                entity.DTime = clock.GetNow();
                await UpdateAsync(entity.Id, entity);
            }
        }
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
