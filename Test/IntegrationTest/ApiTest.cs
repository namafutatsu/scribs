using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Models;
using Scribs.Core.Services;

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

        [Fact]
        public async void GetProject() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string data = JsonConvert.SerializeObject(new DocumentModel {
                    Name = fixture.ProjectName
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/project/get", contentData);
                var project = JsonConvert.DeserializeObject<DocumentModel>(await response.Content.ReadAsStringAsync());
                Assert.Equal(fixture.Project.Id, project.Id);
                Assert.Equal(fixture.Project.Content, project.Content);
                Assert.Equal(fixture.Project.Children.Count, project.Children.Count);
            }
        }

        [Fact]
        public async void PostProject() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var service = fixture.Services.GetService<Factory<Document>>();
                var oldProject = service.GetByName(fixture.Project.Name);
                oldProject.Name = "PostProject";
                var model = fixture.Services.GetService<IMapper>().Map<DocumentModel>(oldProject);
                model.Id = null;
                string data = JsonConvert.SerializeObject(model);
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/project/post", contentData);
                var project = JsonConvert.DeserializeObject<DocumentModel>(await response.Content.ReadAsStringAsync());
                var newProject = service.Get(project.Id);
                Assert.Equal(oldProject.Name, newProject.Name);
                Assert.Null(newProject.Content);
            }
        }

        [Fact]
        public async void UpdateProject() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var service = fixture.Services.GetService<Factory<Document>>();
                var oldProject = service.GetByName(fixture.Project.Name);
                oldProject.Name = "UpdateProject.Old";
                var mapper = fixture.Services.GetService<IMapper>();
                var model = fixture.Services.GetService<IMapper>().Map<DocumentModel>(oldProject);
                model.Id = null;
                string data = JsonConvert.SerializeObject(model);
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/project/post", contentData);
                var project = JsonConvert.DeserializeObject<DocumentModel>(await response.Content.ReadAsStringAsync());
                var newProject = service.Get(project.Id);
                newProject.Name = "UpdateProject.New";
                data = JsonConvert.SerializeObject(mapper.Map<DocumentModel>(newProject));
                contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                response = await client.PostAsync($"api/project/post", contentData);
                Assert.Equal(newProject.Name, service.Get(project.Id).Name);
            }
        }
    }
}