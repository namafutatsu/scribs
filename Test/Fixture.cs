using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Scribs.Core;
using Scribs.Core.Entities;
using Scribs.Core.Services;
using Scribs.Core.Storages;

namespace Scribs.Test {
    public class Fixture: IDisposable {
        public string userName = "Kenny";
        public string projectName = "Test";
        public ConfigurableServer Server => new ConfigurableServer();
        public IServiceProvider Services => Server.Services;
        public User User { get; }
        public Document Project { get; }

        public Fixture() {
            User = CreateUser(userName);
            Project = new Document(projectName, User);
            ClearData();
            //SaveEntity(User);
            //Services.GetService<GitHubService>().Create(Project);
        }

        private User CreateUser(string userName) => new User(userName) {
            Mail = $"{userName}@scribs.io",
            Password = "azerty",
        };

        private void SaveEntity<E>(E entity) where E : Entity {
            var factory = Services.GetFactory<E>();
            factory.Create(entity);
        }

        private void DeleteEntity<E>(string name) where E : Entity {
            var factory = Services.GetFactory<E>();
            E entity = factory.GetByName(name);
            if (entity != null) {
                factory.Remove(entity);
            }
        }

        public void ClearData() {
            Services.GetService<GitHubService>().Delete(Project);
            DeleteEntity<User>(userName);
            foreach (var storage in new List<ILocalStorage> {
                Services.GetService<GitStorage>(),
                Services.GetService<JsonStorage>()
            }) {
                string path = Path.Combine(storage.Root, User.Path);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
        }

        //public void CloneTestRepo() {
        //    var repositoryService = Server.Services.GetService<RepositoryService>();
        //    var gitStorage = Server.Services.GetService<GitStorage>();
        //    string path = Path.Combine(gitStorage.Root, User.Path, repoName);
        //    repositoryService.Clone($"scribs_{userName}_{repoName}", path);
        //}

        public void Dispose() {
            //ClearData();
            Server.Dispose();
        }
    }
}
