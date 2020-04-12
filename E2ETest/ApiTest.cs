using Newtonsoft.Json;
using Xunit;
using Scribs.Core.Entities;

namespace Scribs.E2ETest {

    [Collection("E2E")]
    public class ApiTest : IClassFixture<Fixture> {
        Fixture fixture;

        public ApiTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public void ApiGet() {
            using (var client = fixture.Server.CreateClient()) {
                var response = client.GetAsync($"api/user/getbyname/{fixture.UserName}").Result;
                var json = response.Content.ReadAsStringAsync().Result;
                var user = JsonConvert.DeserializeObject<User>(json);
                Assert.Equal(fixture.UserMail, user.Mail);
            }
        }
    }
}