using System;
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
using Scribs.Core;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Scribs.IntegrationTest {

    [Collection("Integration")]
    public class ApiTest {
        Fixture fixture;

        public ApiTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ApiGet() {
            using (var client = fixture.Server.CreateClient()) {
                var response = client.GetAsync($"api/user/getbyname/{fixture.UserName}").Result;
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(json);
                Assert.Equal(fixture.UserMail, user.Mail);
            }
        }

        [Fact]
        public async Task RegisterUser() {
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
        public async Task RegisterDuplicateUser() {
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
        public async Task RegisterUserWrongMail() {
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
        public async Task RegisterUserNoName() {
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
        public async Task RegisterUserWrongPassword() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserRegistrationModel {
                    Name = "Name",
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
        public async Task SignIn() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserSignInModel {
                    Name = fixture.UserName,
                    Password = fixture.Password
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/login", contentData);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                string json = await response.Content.ReadAsStringAsync();
                string token = JsonConvert.DeserializeObject<UserSignInModel>(json).Token;
                Assert.NotNull(token);
            }
        }

        [Fact]
        public async Task SignInNoName() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserSignInModel {
                    Password = fixture.Password
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/login", contentData);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task SignInWrongName() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserSignInModel {
                    Name = fixture.UserName + "_",
                    Password = fixture.Password
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/login", contentData);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task SignInNoPassword() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserSignInModel {
                    Name = fixture.UserName
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/login", contentData);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task SignInWrongPassword() {
            using (var client = fixture.Server.CreateClient()) {
                string data = JsonConvert.SerializeObject(new UserSignInModel {
                    Name = fixture.UserName,
                    Password = fixture.Password + "_"
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/user/login", contentData);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetProject() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string data = JsonConvert.SerializeObject(new DocumentModel {
                    Name = fixture.ProjectName
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/project/get", contentData);
                var workspace = JsonConvert.DeserializeObject<WorkspaceModel>(await response.Content.ReadAsStringAsync());
                var project = workspace.Project;
                Assert.Equal(fixture.Project.Id, project.Id);
                Assert.Equal(fixture.Project.Children.Count, project.Children.Count);
                Assert.Contains(project.Id, workspace.Texts);
                Assert.Equal(fixture.Project.Content, workspace.Texts[project.Id]);
            }
        }

        [Fact]
        public async Task GetProjects() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"api/project/getlist");
                var projects = JsonConvert.DeserializeObject<IEnumerable<DocumentModel>>(await response.Content.ReadAsStringAsync());
                Assert.Single(projects.Where(o => o.Id == fixture.Project.Id));
            }
        }

        [Fact]
        public async Task GetProjectUnauthorized() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(Utils.CreateId());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string data = JsonConvert.SerializeObject(new DocumentModel {
                    Name = fixture.ProjectName
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => client.PostAsync($"api/project/get", contentData));
            }
        }

        [Fact]
        public async Task PostProject() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var service = fixture.Services.GetService<Factory<Document>>();
                var oldProject = await service.GetByNameAsync(fixture.Project.Name);
                oldProject.Name = "PostProject";
                var model = fixture.Services.GetService<IMapper>().Map<DocumentModel>(oldProject);
                model.Id = null;
                string data = JsonConvert.SerializeObject(model);
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/project/post", contentData);
                string id = await response.Content.ReadAsStringAsync();
                var newProject = await service.GetAsync(id);
                Assert.Equal(oldProject.Name, newProject.Name);
                Assert.Null(newProject.Content);
            }
        }

        [Fact]
        public async Task PostProjectUnauthorized() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(Utils.CreateId());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var service = fixture.Services.GetService<Factory<Document>>();
                var oldProject = await service.GetByNameAsync(fixture.Project.Name);
                oldProject.Name = "PostProject";
                var model = fixture.Services.GetService<IMapper>().Map<DocumentModel>(oldProject);
                model.Id = null;
                string data = JsonConvert.SerializeObject(model);
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => client.PostAsync($"api/project/post", contentData));
            }
        }

        [Fact]
        public async Task UpdateProject() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var service = fixture.Services.GetService<Factory<Document>>();
                var oldProject = await service.GetByNameAsync(fixture.Project.Name);
                oldProject.Name = "UpdateProject.Old";
                var mapper = fixture.Services.GetService<IMapper>();
                var model = fixture.Services.GetService<IMapper>().Map<DocumentModel>(oldProject);
                model.Id = null;
                string data = JsonConvert.SerializeObject(model);
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/project/post", contentData);
                string id = await response.Content.ReadAsStringAsync();
                var newProject = await service.GetAsync(id);
                newProject.Name = "UpdateProject.New";
                data = JsonConvert.SerializeObject(mapper.Map<DocumentModel>(newProject));
                contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                response = await client.PostAsync($"api/project/post", contentData);
                Assert.Equal(newProject.Name, (await service.GetAsync(id)).Name);
            }
        }

        [Fact]
        public async Task UpdateProjectUnauthorized() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(Utils.CreateId());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var service = fixture.Services.GetService<Factory<Document>>();
                var oldProject = await service.GetByNameAsync(fixture.Project.Name);
                oldProject.Name = "UpdateProject.Old";
                var mapper = fixture.Services.GetService<IMapper>();
                var model = fixture.Services.GetService<IMapper>().Map<DocumentModel>(oldProject);
                model.Id = null;
                string data = JsonConvert.SerializeObject(model);
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => client.PostAsync($"api/project/post", contentData));
            }
        }

        [Fact]
        public async Task GetText() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string data = JsonConvert.SerializeObject(new TextModel {
                    Id = fixture.Project.Id,
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/text/get", contentData);
                string content = await response.Content.ReadAsStringAsync();
                Assert.Equal(fixture.Project.Content, content);
            }
        }

        [Fact]
        public async Task GetTextUnauthorized() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(Utils.CreateId());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string data = JsonConvert.SerializeObject(new TextModel {
                    Id = fixture.Project.Id,
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => client.PostAsync($"api/text/get", contentData));
            }
        }

        [Fact]
        public async Task PostText() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string content = Guid.NewGuid().ToString();
                string id = Utils.CreateId();
                string data = JsonConvert.SerializeObject(new TextModel {
                    Id = id,
                    ProjectId = fixture.Project.Id,
                    Content = content
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/text/post", contentData);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var text = await fixture.Services.GetService<Factory<Text>>().GetAsync(id);
                Assert.NotNull(text);
                Assert.Equal(fixture.Project.Id, text.ProjectId);
                Assert.Equal(fixture.User.Id, text.UserId);
                Assert.Equal(content, text.Content);
            }
        }

        [Fact]
        public async Task PostTextUnauthorized() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(Utils.CreateId());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string content = Guid.NewGuid().ToString();
                string id = Utils.CreateId();
                string data = JsonConvert.SerializeObject(new TextModel {
                    Id = id,
                    ProjectId = fixture.Project.Id,
                    Content = content
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => client.PostAsync($"api/text/post", contentData));
            }
        }

        [Fact]
        public async Task UpdateText() {
            using (var client = fixture.Server.CreateClient()) {
                string token = AuthService.GenerateToken(fixture.User.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var saved = new Text {
                    Id = Utils.CreateId(),
                    UserId = fixture.User.Id,
                    ProjectId = fixture.Project.Id,
                    Name = Guid.NewGuid().ToString(),
                    Content = Guid.NewGuid().ToString()
                };
                string newContent = Guid.NewGuid().ToString();
                string data = JsonConvert.SerializeObject(new TextModel {
                    Id = saved.Id,
                    Content = newContent
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/text/post", contentData);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var text = await fixture.Services.GetService<Factory<Text>>().GetAsync(saved.Id);
                Assert.NotNull(text);
                Assert.Equal(fixture.User.Id, text.UserId);
                Assert.Equal(newContent, text.Content);
            }
        }

        [Fact]
        public async Task GetTextFromOtherUser() {
            using (var client = fixture.Server.CreateClient()) {
                var hacker = new User("GetTextFromOtherUser");
                await fixture.Services.GetFactory<User>().CreateAsync(hacker);
                string token = AuthService.GenerateToken(hacker.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string data = JsonConvert.SerializeObject(new TextModel {
                    Id = fixture.Project.Id,
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/text/get", contentData);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task UpdateTextFromOtherUser() {
            using (var client = fixture.Server.CreateClient()) {
                var hacker = new User("UpdateTextFromOtherUser");
                await fixture.Services.GetFactory<User>().CreateAsync(hacker);
                string token = AuthService.GenerateToken(hacker.Id);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var saved = new Text {
                    Id = Utils.CreateId(),
                    UserId = fixture.User.Id,
                    ProjectId = fixture.Project.Id,
                    Name = Guid.NewGuid().ToString(),
                    Content = Guid.NewGuid().ToString()
                };
                await fixture.Services.GetFactory<Text>().CreateAsync(saved);
                string newContent = Guid.NewGuid().ToString();
                string data = JsonConvert.SerializeObject(new TextModel {
                    Id = saved.Id,
                    Content = newContent
                });
                var contentData = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/text/post", contentData);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }
    }
}