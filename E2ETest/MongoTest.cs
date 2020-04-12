using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.E2ETest {

    [Collection("E2E")]
    public class MongoTest : IClassFixture<Fixture> {
        Fixture fixture;

        public MongoTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public void DbGet() {
            var user = fixture.Services.GetFactory<User>().GetByName(fixture.UserName);
            Assert.Equal(fixture.UserMail, user.Mail);
        }
    }
}