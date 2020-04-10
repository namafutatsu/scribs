using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Scribs.Core.Entities;

namespace Scribs.Test {
    public class UserTest : IClassFixture<Fixture> {
        Fixture fixture;

        public UserTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetUser() {
            using (var client = fixture.Server.CreateClient()) {
                var response = await client.GetAsync("api/user");
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(json);
                Assert.Equal("gdrtf@mail.com", user.Mail);
            }
        }
    }
}