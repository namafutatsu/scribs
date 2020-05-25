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
                .Configure<UserRepositorySettings>(configuration.GetSection(nameof(UserRepositorySettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<UserRepositorySettings>>().Value)
                // Admin repository
                .Configure<AdminRepositorySettings>(configuration.GetSection(nameof(AdminRepositorySettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<AdminRepositorySettings>>().Value)
                // Git storage
                .Configure<GitStorageSettings>(configuration.GetSection(nameof(GitStorageSettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<GitStorageSettings>>().Value)
                // Json storage
                .Configure<JsonStorageSettings>(configuration.GetSection(nameof(JsonStorageSettings)))
                .AddSingleton(s => s.GetRequiredService<IOptions<JsonStorageSettings>>().Value);
        }

        public static IServiceCollection AddServices(this IServiceCollection serviceCollection) {
            return serviceCollection
                .AddSingleton(MapperUtils.GetMapper())
                .AddSingleton<SystemService>()
                .AddSingleton<ClockService>()
                .AddSingleton<MongoService>()
                .AddSingleton<Factories>()
                .AddSingleton<GitHubService>()
                .AddSingleton<UserRepositoryService>()
                .AddSingleton<AdminRepositoryService>()
                .AddSingleton<GitStorage>()
                .AddSingleton<JsonStorage>()
                .AddSingleton<MongoStorage>()
                .AddSingleton<PandocService>()
                // todo virer ?
                .AddSingleton<Factory<User>>()
                .AddSingleton<Factory<Document>>()
                .AddSingleton<Factory<Text>>()
                .AddSingleton<AuthService>();
        }
    }
}
