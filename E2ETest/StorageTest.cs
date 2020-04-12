using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Scribs.Core.Entities;

namespace Scribs.E2ETest {
    public class StorageTest : IClassFixture<Fixture> {
        Fixture fixture;

        public StorageTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public async Task Storage() {
        }
    }
}