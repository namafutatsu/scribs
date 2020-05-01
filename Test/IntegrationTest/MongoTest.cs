using System.Threading.Tasks;
using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.IntegrationTest {

    [Collection("Integration")]
    public class MongoTest {
        Fixture fixture;

        public MongoTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public async Task DbGet() {
            var user = await fixture.Services.GetFactory<User>().GetByNameAsync(fixture.UserName);
            Assert.Equal(fixture.UserMail, user.Mail);
        }
    }
}