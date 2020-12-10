using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Scribs.Core.Entities;
using Scribs.Core.Storages;
using Scribs.Core;
using Scribs.Core.Services;
using System.Linq;
using System.IO;

namespace Scribs.IntegrationTest {

    [Collection("Integration")]
    public class RepositoryTest {
        Fixture fixture;

        public RepositoryTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public void CheckPullDiff() {
            var project = fixture.Services.GetService<JsonStorage>().Load(fixture.UserName, fixture.Project.Name);
            project.Name = "CheckPullDiff";
            fixture.Services.GetService<MongoStorage>().Save(project);
            project = fixture.Services.GetService<MongoStorage>().Load(fixture.UserName, project.Name);
            var gitStorage = fixture.Services.GetService<GitStorage>();
            fixture.DeleteGitHubRepo(project);
            gitStorage.Save(project);
            string path = Path.Join(gitStorage.Root, project.Path);
            fixture.Services.GetService<UserRepositoryService>().Commit(path, "init");
            project.Content += "modif";
            gitStorage.GetDiff(project);
        }
    }
}