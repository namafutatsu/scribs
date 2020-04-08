using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Scribs.Core.Entities;
using Scribs.Core.Services;
using Scribs.Core.Storages;

namespace Scribs.Core {
    public static class Utils {
        public static string CreateGuid() => ObjectId.GenerateNewId().ToString();//Guid.NewGuid().ToString();
        public static string CleanName(string name) => name;
    }

    public static class ServiceExtensions {

        public static Factory<E> GetFactory<E>(this IServiceProvider serviceProvider) where E : Entity {
            return serviceProvider.GetService<Factory<E>>();
        }

        public static IServiceCollection Configure(this IServiceCollection serviceCollection, IConfiguration configuration) {
            return serviceCollection
                // Mongo
                .Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)))
                .AddSingleton<IMongoSettings>(s => s.GetRequiredService<IOptions<MongoSettings>>().Value)
                .AddSingleton<MongoService>()
                // Factories
                .AddSingleton<Factory<User>>()
                // GitHub
                .Configure<GitHubSettings>(configuration.GetSection(nameof(GitHubSettings)))
                .AddSingleton<IGitHubSettings>(s => s.GetRequiredService<IOptions<GitHubSettings>>().Value)
                .AddSingleton<GitHubService>()
                // Repository
                .Configure<RepositorySettings>(configuration.GetSection(nameof(RepositorySettings)))
                .AddSingleton<IRepositorySettings>(s => s.GetRequiredService<IOptions<RepositorySettings>>().Value)
                .AddSingleton<RepositoryService>()
                // Git storage
                .Configure<GitStorageSettings>(configuration.GetSection(nameof(GitStorageSettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<GitStorageSettings>>().Value)
                .AddSingleton<GitStorage>()
                // Json storage
                .Configure<JsonStorageSettings>(configuration.GetSection(nameof(JsonStorageSettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<JsonStorageSettings>>().Value)
                .AddSingleton<JsonStorage>();
        }
    }
}
