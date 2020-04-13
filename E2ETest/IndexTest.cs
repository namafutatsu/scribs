using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Scribs.Core.Storages;
using Scribs.Core;

namespace Scribs.E2ETest {

    [Collection("E2E")]
    public class IndexTest : IClassFixture<Fixture> {
        Fixture fixture;

        public IndexTest(Fixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public void LoadGitIndexInFileName() {
            var project = fixture.Services.GetService<GitStorage>().Load(fixture.User.Name, fixture.Project.Name);
            var dave = fixture.Project
                .Children.FirstOrDefault(o => o.Name == "notes")?
                .Children.FirstOrDefault(o => o.Name == "chars")?
                .Children.SingleOrDefault(o => o.Index == 4);
            Assert.NotNull(dave);
            Assert.Equal("dave", dave.Name);
        }

        [Fact]
        public void LoadGitIndexInUnordedFile() {
            var project = fixture.Services.GetService<GitStorage>().Load(fixture.User.Name, fixture.Project.Name);
            var notes03 = fixture.Project
                .Children.FirstOrDefault(o => o.Name == "notes")?
                .Children.FirstOrDefault(o => o.Name == "notes03");
            Assert.NotNull(notes03);
            Assert.Equal(0, notes03.Index);
        }

        [Fact]
        public void LoadGitIndexInUnnamedFile() {
            var gitStorage = fixture.Services.GetService<GitStorage>();
            var project = gitStorage.Load(fixture.User.Name, fixture.Project.Name);
            var file03 = project.Children.FirstOrDefault(o => o.Name == "03");
            Assert.Equal(0, file03.Index); // Order alphabetically
            Assert.Equal("03", file03.Name);
        }

        [Fact]
        public void SaveGitIndexInFileName() {
            var gitStorage = fixture.Services.GetService<GitStorage>();
            var project = gitStorage.Load(fixture.User.Name, fixture.Project.Name);
            project.Name = "SaveGitIndexInFileName";
            project.Disconnect = true;
            gitStorage.Save(project);
            Assert.True(File.Exists(Path.Combine(gitStorage.Root, project.Path, "notes", "03.chars", "04.dave.md")));
        }

        [Fact]
        public void SaveAndLoadJsonIndexInFileName() {
            var storage = fixture.Services.GetService<JsonStorage>();
            var project = storage.Load(fixture.User.Name, fixture.Project.Name);
            var dave = fixture.Project
                .Children.FirstOrDefault(o => o.Name == "notes")?
                .Children.FirstOrDefault(o => o.Name == "chars")?
                .Children.SingleOrDefault(o => o.Index == 4);
            Assert.Equal("dave", dave.Name);
        }

        [Fact]
        public void SaveAndLoadJsonIndexInUnordedFile() {
            var storage = fixture.Services.GetService<JsonStorage>();
            var project = storage.Load(fixture.User.Name, fixture.Project.Name);
            var notes03 = fixture.Project
                .Children.FirstOrDefault(o => o.Name == "notes")?
                .Children.FirstOrDefault(o => o.Name == "notes03");
            Assert.NotNull(notes03);
            Assert.Equal(0, notes03.Index);
        }

        [Fact]
        public void SaveAndLoadJsonIndexInUnnamedFile() {
            var storage = fixture.Services.GetService<JsonStorage>();
            var project = storage.Load(fixture.User.Name, fixture.Project.Name);
            var file03 = project.Children.FirstOrDefault(o => o.Name == "03");
            Assert.Equal(0, file03.Index);
            Assert.Equal("03", file03.Name);
        }

        private void LoadIndexParams<S>() where S: ILocalStorage {
            var storage = fixture.Services.GetService<S>();
            var project = storage.Load(fixture.User.Name, fixture.Project.Name);
            Assert.False(project.IndexNodes);
            Assert.False(project.IndexLeaves);
            var notes = project.Children.FirstOrDefault(o => o.Name == "notes");
            Assert.True(notes.IndexNodes);
            Assert.False(notes.IndexLeaves);
            var chars = notes.Children.FirstOrDefault(o => o.Name == "chars");
            Assert.True(chars.IndexNodes); // Inheritance from notes
            Assert.True(chars.IndexLeaves);
        }

        [Fact]
        public void LoadIndexParamsGitStorage() => LoadIndexParams<GitStorage>();

        [Fact]
        public void LoadIndexParamsJsonStorage() => LoadIndexParams<JsonStorage>();
    }
}