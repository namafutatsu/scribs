using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.UnitTest {
    public class UserTest {
        public string userName = "Kenny";

        [Fact]
        public async Task GetUserMail() {
            string mail = "user@mail.com";
            var userFactory = new Mock<Factory<User>>(null);
            userFactory.Setup(m => m.GetByName(It.Is<string>(o => o == userName))).Returns(new User(userName) { Mail = mail });
            var serviceDescriptor = new ServiceDescriptor(typeof(Factory<User>), userFactory.Object);
            using (var server = new ConfigurableServer(sc => sc.Replace(serviceDescriptor))) {
                using (var client = server.CreateClient()) {
                    var response = await client.GetAsync($"api/user/getbyname/{userName}");
                    var json = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(json);
                    Assert.Equal(mail, user.Mail);
                }
            }
        }
    }
}