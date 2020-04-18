using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scribs.Core.Entities;
using Scribs.Core.Storages;

namespace Scribs.Core.Services {
    public static class ServiceExtensions {

        public static Factory<E> GetFactory<E>(this IServiceProvider serviceProvider) where E : Entity {
            return serviceProvider.GetService<Factory<E>>();
        }

        public static IServiceCollection Configure(this IServiceCollection serviceCollection, IConfiguration configuration) {
            return serviceCollection
                // Mongo
                .Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)))
                .AddSingleton<IMongoSettings>(s => s.GetRequiredService<IOptions<MongoSettings>>().Value)
                // GitHub
                .Configure<GitHubSettings>(configuration.GetSection(nameof(GitHubSettings)))
                .AddSingleton<IGitHubSettings>(s => s.GetRequiredService<IOptions<GitHubSettings>>().Value)
                // Repository
                .Configure<RepositorySettings>(configuration.GetSection(nameof(RepositorySettings)))
                .AddSingleton<IRepositorySettings>(s => s.GetRequiredService<IOptions<RepositorySettings>>().Value)
                // Git storage
                .Configure<GitStorageSettings>(configuration.GetSection(nameof(GitStorageSettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<GitStorageSettings>>().Value)
                // Json storage
                .Configure<JsonStorageSettings>(configuration.GetSection(nameof(JsonStorageSettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<JsonStorageSettings>>().Value);
        }

        public static IServiceCollection AddServices(this IServiceCollection serviceCollection) {
            return serviceCollection
                .AddSingleton<SystemService>()
                .AddSingleton<MongoService>()
                .AddSingleton<Factories>()
                .AddSingleton<GitHubService>()
                .AddSingleton<RepositoryService>()
                .AddSingleton<GitStorage>()
                .AddSingleton<JsonStorage>()
                .AddSingleton<MongoStorage>()
                // todo virer ?
                .AddSingleton<Factory<User>>()
                .AddSingleton<Factory<Document>>();
        }
    }
}
