using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Scribs.Core.Entities;

namespace Scribs.E2ETest {

    [Collection("E2E")]
    public class GitStorageTest : IClassFixture<Fixture> {
        Fixture fixture;

        public GitStorageTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public void Storage() {
        }
    }
}