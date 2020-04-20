using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Storages;
using Scribs.Core;

namespace Scribs.IntegrationTest {

    [Collection("Integration")]
    public class StorageTest {
        Fixture fixture;

        public StorageTest(Fixture fixture) {
            this.fixture = fixture;
        }

        private void StorageLoad<S>() where S : IStorage {
            var project = fixture.Services.GetService<S>().Load(fixture.UserName, fixture.Project.Name);
            Assert.True(Document.Equals(fixture.Project, project));
            Assert.Equal(fixture.User.Name, project.UserName);
        }

        private void StorageSaveThenLoad<S>() where S : IStorage {
            var project = fixture.Services.GetService<JsonStorage>().Load(fixture.UserName, fixture.Project.Name);
            project.Disconnect = true;
            project.Name = "StorageSaveThenLoad" + typeof(S).ToString();
            var storage = fixture.Services.GetService<S>();
            storage.Save(project);
            Assert.True(Document.Equals(project, storage.Load(fixture.UserName, project.Name)));
        }

        private void CrossStorage<S, D>() where S : IStorage where D : IStorage {
            var sourceStorage = fixture.Services.GetService<S>();
            var destinationStorage = fixture.Services.GetService<D>();
            var sourceProject = sourceStorage.Load(fixture.User.Name, fixture.Project.Name);
            Assert.False(sourceProject.NoMetadata);
            sourceProject.Name = "CrossStorage" + typeof(S).ToString() + typeof(D).ToString();
            sourceProject.Disconnect = true;
            destinationStorage.Save(sourceProject);
            var destinationProject = destinationStorage.Load(fixture.User.Name, sourceProject.Name);
            Assert.False(destinationProject.NoMetadata);
            Assert.True(destinationProject.Equals(sourceProject));
            destinationProject.Name = fixture.Project.Name;
            Assert.True(destinationProject.Equals(fixture.Project));
        }

        [Fact]
        public void GitStorageLoad() => StorageLoad<GitStorage>();

        [Fact]
        public void JsonStorageLoad() => StorageLoad<JsonStorage>();

        [Fact]
        public void GitStorageSaveThenLoad() => StorageSaveThenLoad<GitStorage>();

        [Fact]
        public void JsonStorageSaveThenLoad() => StorageSaveThenLoad<JsonStorage>();

        [Fact]
        public void CrossStorageJsonToGit() => CrossStorage<JsonStorage, GitStorage>();

        [Fact]
        public void MongoStorageLoad() => StorageLoad<MongoStorage>();

        [Fact]
        public void CrossStorageMongoToGit() => CrossStorage<MongoStorage, GitStorage>();
    }
}