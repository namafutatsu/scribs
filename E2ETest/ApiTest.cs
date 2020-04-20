using Newtonsoft.Json;
using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Models;
using System.Net.Http;
using System.Net;

namespace Scribs.IntegrationTest {

    [Collection("Integration")]
    public class ApiTest {
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

        [Fact]
        public async void RegisterUser() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserRegistrationModel {
                    FirstName = "First",
                    LastName = "Last",
                    Name = "RegisterUser",
                    Password = "azerty",
                    ConfirmPassword = "azerty",
                    Mail = "mail@scribs.io"
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/register", contentData);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async void RegisterDuplicateUser() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserRegistrationModel {
                    FirstName = "First",
                    LastName = "Last",
                    Name = "RegisterDuplicateUser",
                    Password = "azerty",
                    ConfirmPassword = "azerty",
                    Mail = "mail@scribs.io"
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/register", contentData);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                data = JsonConvert.SerializeObject(new UserRegistrationModel {
                    FirstName = "First",
                    LastName = "Last",
                    Name = "RegisterDuplicateUser",
                    Password = "azerty",
                    ConfirmPassword = "azerty",
                    Mail = "mail@scribs.io"
                });
                contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                response = await client.PostAsync($"api/user/register", contentData);
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }

        [Fact]
        public async void RegisterUserWrongMail() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserRegistrationModel {
                    FirstName = "First",
                    LastName = "Last",
                    Name = "RegisterUserWrongMail",
                    Password = "azerty",
                    ConfirmPassword = "azerty",
                    Mail = "mailsatcribs.io"
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/register", contentData);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async void RegisterUserNoName() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserRegistrationModel {
                    FirstName = "First",
                    LastName = "Last",
                    Password = "azerty",
                    ConfirmPassword = "azerty",
                    Mail = "mail@scribs.io"
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/register", contentData);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async void RegisterUserWrongPassword() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserRegistrationModel {
                    FirstName = "First",
                    LastName = "Last",
                    Password = "azerty",
                    ConfirmPassword = "azerty2",
                    Mail = "mail@scribs.io"
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/register", contentData);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }
    }
}