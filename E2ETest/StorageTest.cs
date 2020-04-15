using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Storages;
using Scribs.Core;

namespace Scribs.E2ETest {

    [Collection("E2E")]
    public class StorageTest : IClassFixture<Fixture> {
        Fixture fixture;

        public StorageTest(Fixture fixture) {
            this.fixture = fixture;
        }

        private void StorageLoad<S>() where S : ILocalStorage {
            Assert.True(Document.Equals(fixture.Project,
                fixture.Services.GetService<S>().Load(fixture.UserName, fixture.Project.Name)));
        }

        private void StorageSaveThenLoad<S>() where S : ILocalStorage {
            var project = fixture.Services.GetService<JsonStorage>().Load(fixture.UserName, fixture.Project.Name);
            project.Disconnect = true;
            project.Name = "StorageSaveThenLoad" + typeof(S).ToString();
            var storage = fixture.Services.GetService<S>();
            storage.Save(project);
            Assert.True(Document.Equals(project, storage.Load(fixture.UserName, project.Name)));
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
        public void CrossStorage() {
            var jsonStorage = fixture.Services.GetService<JsonStorage>();
            var gitStorage = fixture.Services.GetService<GitStorage>();
            var jsonProject = jsonStorage.Load(fixture.User.Name, fixture.Project.Name);
            Assert.False(jsonProject.NoMetadata);
            jsonProject.Name = "CrossStorage";
            jsonProject.Disconnect = true;
            gitStorage.Save(jsonProject);
            var gitProject = gitStorage.Load(fixture.User.Name, jsonProject.Name);
            Assert.False(gitProject.NoMetadata);
            Assert.True(gitProject.Equals(jsonProject));
            gitProject.Name = fixture.Project.Name;
            Assert.True(gitProject.Equals(fixture.Project));
        }
    }
}