using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using Scribs.Core;
using Scribs.Core.Entities;
using Scribs.Core.Services;
using Scribs.Core.Storages;

namespace Scribs.E2ETest {

    [CollectionDefinition("E2E")]
    public class DatabaseCollection : ICollectionFixture<Fixture> {
    }

    public class Fixture: IDisposable {
        public string UserName => "Kenny";
        public string UserMail => $"{UserName}@scribs.io";
        public string ProjectName => "Test";
        public ConfigurableServer Server;
        public IServiceProvider Services => Server.Services;
        public User User { get; }
        public Document Project { get; } // Post by API

        public Fixture() {
            Server = CreateServer();
            User = CreateUser(UserName);
            Project = new Document(ProjectName, User);
            ClearData();
            SaveEntity(User);
            FillProject(Project);
            SaveProject<GitStorage>(Project);
            SaveProject<JsonStorage>(Project);
            CommitRepo(Project);
        }

        private ConfigurableServer CreateServer() {
            string path = Path.Combine(Path.GetTempPath(), "Scribs", Utils.CreateGuid());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var gitStorageSettings = new Mock<GitStorageSettings>();
            gitStorageSettings.Setup(m => m.Local).Returns(true);
            gitStorageSettings.Setup(m => m.Root).Returns(Path.Combine(path, "git"));
            var gitStorageSettingsDescriptor = new ServiceDescriptor(typeof(GitStorageSettings), gitStorageSettings.Object);
            var jsonStorageSettings = new Mock<JsonStorageSettings>();
            jsonStorageSettings.Setup(m => m.Local).Returns(true);
            jsonStorageSettings.Setup(m => m.Root).Returns(Path.Combine(path, "json"));
            var jsonStorageSettingsDescriptor = new ServiceDescriptor(typeof(JsonStorageSettings), jsonStorageSettings.Object);
            return new ConfigurableServer(sc => sc.Replace(gitStorageSettingsDescriptor).Replace(jsonStorageSettingsDescriptor));
        }

        private User CreateUser(string userName) => new User(userName) {
            Mail = UserMail,
            Password = "azerty",
        };

        private void SaveEntity<E>(E entity) where E : Entity {
            var factory = Services.GetFactory<E>();
            factory.Create(entity);
        }

        private void DeleteEntity<E>(string name) where E : Entity {
            var factory = Services.GetFactory<E>();
            E entity = factory.GetByName(name);
            if (entity != null) {
                factory.Remove(entity);
            }
        }

        private void CreateGitHubRepo(Document project) {
            Services.GetService<GitHubService>().Create(Project);
        }

        public void DeleteGitHubRepo(Document project) {
            if (project.User.Name != UserName)
                return;
            var service = Services.GetService<GitHubService>();
            if (service.Exists(project))
                service.Delete(Project);
        }

        private void CommitRepo(Document project) {
            string path = Path.Combine(Services.GetService<GitStorage>().Root, project.Path);
            Services.GetService<RepositoryService>().Commit(path, "Message");
        }

        private void FillProject(Document project) {
            project.Text = "Lorem ipsum dolor sit amet";
            project.IndexLeaves = false;
            project.CreateDocument("01", "Donec at lobortis libero, at fringilla tellus. Vestibulum eget tortor orci. Donec fermentum risus neque, a volutpat risus elementum rutrum. Cras dapibus est ligula. Suspendisse vitae tincidunt eros. Nulla sollicitudin nisl sed nunc dignissim cursus. Pellentesque nibh lacus, ultrices ut felis a, pharetra lobortis orci. Ut in euismod tortor. Aenean sagittis, odio laoreet semper tempus, ante libero tempor sem, eget faucibus augue nunc eget lorem. Phasellus vulputate sodales mi volutpat euismod. Nulla cursus tellus nunc, ac pretium sem faucibus semper. Sed tristique lectus purus, nec congue elit pretium et. Nulla vestibulum nec lectus a scelerisque.");
            project.CreateDocument("02", "Vestibulum viverra nunc dictum sem sollicitudin, vel suscipit elit lacinia. Nullam ex purus, luctus eu odio nec, rhoncus gravida arcu. Donec eu rutrum odio. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Etiam viverra nibh ac felis pulvinar dictum. Maecenas ex velit, euismod ac blandit eget, viverra et nulla. Mauris tempor ante nec diam luctus tempor. Ut vitae felis vel orci dapibus commodo placerat ornare turpis. Aenean bibendum sollicitudin felis, sit amet imperdiet nunc porta ac. Quisque risus est, commodo tempor interdum et, pretium in velit. Integer id sollicitudin augue, vitae maximus eros. Etiam eu leo venenatis, rutrum sapien a, mollis libero. Integer eget tellus imperdiet dolor tincidunt iaculis ut ut neque. Curabitur sollicitudin dolor nec lectus tristique convallis. Suspendisse tincidunt, urna non rutrum venenatis, odio mauris lacinia tellus, suscipit facilisis felis orci at enim.");
            project.CreateDocument("03", "Pellentesque bibendum at nibh quis lobortis.\n\nVivamus laoreet diam nec magna faucibus pretium. Phasellus lacinia nibh est, sit amet molestie purus lacinia et. Aliquam erat volutpat. Sed dapibus erat sapien, facilisis pellentesque orci auctor quis. Nunc ut quam erat. Mauris scelerisque est sit amet hendrerit pellentesque. Aenean nibh nibh, aliquet lacinia fermentum et, vulputate sit amet justo. Cras sed odio a purus ornare aliquam. Nulla vulputate interdum quam, at placerat elit semper vitae. Morbi consectetur mollis elementum.");
            var notes = Project.CreateDocument("notes", "Consectetur adipiscing elit");
            notes.IndexNodes = true;
            notes.IndexLeaves = false;
            notes.CreateDocument("notes01", "Nulla et molestie dolor. Mauris scelerisque lacus quis mi congue pharetra. Sed porttitor auctor lorem id elementum. Sed ut nulla nulla. Maecenas semper, leo et luctus commodo, arcu sem tempor ex, ac dapibus risus ligula id lorem. Donec dictum ultrices arcu sit amet luctus. Aenean ut eros eu nisi vehicula tristique vitae in diam. Sed vitae nunc non est tristique tempus ut nec nisl. Phasellus mollis diam turpis, id blandit augue vestibulum nec.");
            notes.CreateDocument("notes02", "Mauris at arcu nibh. Aliquam turpis augue, porta ac fermentum et, rutrum in elit. Vestibulum mauris arcu, ullamcorper ut pulvinar quis, consectetur ut justo. Donec orci purus, pulvinar et sollicitudin quis, fermentum vel lacus. Fusce pretium nibh sed gravida molestie. Aenean hendrerit sagittis mattis. Mauris condimentum pellentesque mi. Nullam a finibus orci. Cras malesuada, sem vulputate gravida pretium, sapien enim pulvinar arcu, vel venenatis orci ante vel tellus.");
            notes.CreateDocument("notes03", "Vivamus eu tempus diam. Cras aliquet volutpat egestas. Curabitur euismod dictum velit non varius. Fusce et congue leo. In hac habitasse platea dictumst. Fusce ac ipsum diam. Nullam nec vulputate justo. Aliquam ex metus, tincidunt ac lobortis facilisis, commodo et leo.\n\nDonec eget dolor et purus blandit vestibulum eu sit amet purus.");
            notes.CreateDocument("notes04", "Duis porta elit justo, ac euismod sem rutrum nec. In efficitur fermentum velit, nec laoreet tellus rutrum nec. Suspendisse in mauris vehicula sapien posuere dictum ut ut justo. Fusce at arcu nisl. Mauris ut mi a sem maximus pretium. Etiam bibendum eu tortor eget feugiat. Nullam lorem augue, ornare vitae erat at, vulputate hendrerit eros. Quisque blandit eros metus, vel malesuada libero luctus id. Vestibulum in lorem lorem. Ut in leo commodo, facilisis tortor sit amet, cursus arcu. Pellentesque dapibus ex sed consectetur pulvinar. Proin vel nulla ac turpis hendrerit tempor. Quisque purus mauris, rhoncus in elementum et, accumsan ac sem. Maecenas pretium, augue vel efficitur tincidunt, erat odio tempus dolor, ut faucibus urna velit sed odio. Quisque sagittis sed dui sed luctus. Proin consectetur a augue a consequat.");
            var places = notes.CreateDocument("places", null, 1);
            places.CreateDocument("places01", "In efficitur fermentum velit.");
            var times = notes.CreateDocument("times", null, 2);
            times.CreateDocument("times01", "Quisque bibendum bibendum diam");
            var chars = notes.CreateDocument("chars", null, 3);
            chars.IndexLeaves = true;
            chars.CreateDocument("alice", "Praesent sit amet enim urna. Nunc facilisis laoreet urna a efficitur. Nulla et egestas nisi. Integer vitae commodo justo. Nulla semper felis at elit maximus aliquam. Donec ornare nisl ut malesuada viverra. Duis eget interdum lacus. Donec ante diam, pellentesque eu diam vel, interdum convallis dolor. Integer eget nisl vel neque sollicitudin fermentum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam quis urna nisl. Vestibulum vestibulum, nunc blandit tempor condimentum, orci eros ultricies felis, vitae aliquam tortor dolor tincidunt est. Cras posuere, mauris ac pulvinar tempor, enim metus pulvinar velit, in rutrum enim ante sed tellus.", 1);
            chars.CreateDocument("bob", "Nullam felis sem, interdum a ultrices ut, volutpat a lorem. Quisque bibendum bibendum diam, in auctor nulla aliquam vitae. Aliquam at tincidunt purus, sed mattis diam. Pellentesque tincidunt tortor sed nisi pulvinar pharetra. Curabitur rutrum, turpis sed vehicula ultrices, lorem lorem dictum mauris, et eleifend diam ligula quis ipsum. Quisque erat erat, laoreet eget arcu a, pharetra elementum leo. Nam cursus purus sem, in fermentum arcu vulputate sit amet. Phasellus id hendrerit elit. Maecenas eget lacus quis libero egestas luctus sed pretium justo. Integer sed mauris tortor. Cras dictum felis a elit aliquet, eu fringilla tortor aliquet.", 2);
            chars.CreateDocument("franck", "Morbi et ultrices enim. Sed id libero a mauris blandit tristique sed ac ipsum. Phasellus non velit ante. Sed sed metus vitae elit luctus sagittis at quis enim. Mauris vel dui ut arcu aliquet auctor sit amet sed massa. Donec a ultricies enim, id mollis lectus. Fusce ornare, tortor sit amet rutrum faucibus, urna nisl finibus est, sed consequat sem arcu vitae velit. Morbi maximus arcu diam, vitae pharetra diam mattis laoreet. Quisque interdum risus ipsum, sit amet rhoncus lorem porta at. Cras nec placerat lorem. Proin vestibulum varius leo vel dictum. Mauris nec nunc ac turpis volutpat fringilla vel sit amet sem. Donec iaculis massa ut congue pellentesque. Sed egestas placerat nisl at tempus.", 3);
            chars.CreateDocument("dave", "Nunc mi justo, lobortis vel neque nec, tincidunt blandit leo. Fusce hendrerit ante sed fermentum convallis. Nam ut elit ligula. Duis non porta dui, sed convallis tellus. Ut efficitur vel nunc quis vestibulum. Aenean nec enim vel odio commodo tempor. Nam et quam et magna fermentum tempus. Mauris sagittis massa sed odio malesuada, in consequat orci elementum. Curabitur sit amet turpis vel arcu auctor pharetra. In ut libero nibh. Duis id eros egestas, dapibus mauris quis, fringilla ligula.", 4);
        }

        private void SaveProject<S>(Document project) where S: ILocalStorage {
            Services.GetService<S>().Save(project);
        }

        public void ClearData() {
            DeleteGitHubRepo(Project);
            DeleteEntity<User>(UserName);
            foreach (var storage in new List<ILocalStorage> {
                Services.GetService<GitStorage>(),
                Services.GetService<JsonStorage>()
            }) {
                string path = Path.Combine(storage.Root, User.Path);
                if (Directory.Exists(path))
                    try {
                        Directory.Delete(path, true);
                    } catch { }
            }
        }

        public void Dispose() {
            //ClearData();
            Server.Dispose();
        }
    }
}
