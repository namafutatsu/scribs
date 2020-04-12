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

        private void StorageLoad<S>() where S: ILocalStorage {
            Assert.True(Document.Equals(fixture.Project,
                fixture.Services.GetService<S>().Load(fixture.UserName, fixture.Project.Name)));
        }

        [Fact]
        public void GitStorageLoad() => StorageLoad<GitStorage>();

        [Fact]
        public void JsonStorageLoad() => StorageLoad<JsonStorage>();
    }
}