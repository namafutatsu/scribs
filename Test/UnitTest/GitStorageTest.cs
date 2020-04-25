using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using Scribs.Core.Services;
using Scribs.Core.Storages;
using Scribs.Core.Entities;

namespace Scribs.UnitTest {

    public class MoqSystemConfiguration : IDisposable {
        private ConfigurableServer server;
        public ConfigurableServer Server {
            get {
                if (server == null)
                    server = new ConfigurableServer(sc => ServiceDescriptors.ForEach(o => sc.Replace(o)));
                return server;
            }
        }
        public Mock<SystemService> Moq { get; }
        public IServiceProvider Services => Server.Services;
        public SystemService System => Services.GetService<SystemService>();
        public List<ServiceDescriptor> ServiceDescriptors { get; } = new List<ServiceDescriptor>();

        public MoqSystemConfiguration(params Action<Mock<SystemService>>[] setups) {
            var gitStorageSettings = new Mock<GitStorageSettings>();
            gitStorageSettings.Setup(m => m.Local).Returns(true);
            gitStorageSettings.Setup(m => m.Root).Returns(Path.Combine("root", "git"));
            ServiceDescriptors.Add(new ServiceDescriptor(typeof(GitStorageSettings), gitStorageSettings.Object));
            Moq = new Mock<SystemService>();
            Moq.Setup(m => m.WriteLeaf(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
            Moq.Setup(m => m.DeleteNode(It.IsAny<string>(), It.IsAny<bool>())).Callback(() => { });
            Moq.Setup(m => m.DeleteLeaf(It.IsAny<string>())).Callback(() => { });
            Moq.Setup(m => m.NodeExists(It.IsAny<string>())).Returns(true);
            Moq.Setup(m => m.GetName(It.IsAny<string>())).CallBase();
            ServiceDescriptors.Add(new ServiceDescriptor(typeof(SystemService), Moq.Object));
            if (setups != null)
                foreach (var setup in setups)
                    setup(Moq);
        }

        public void Dispose() {
            Server.Dispose();
        }
    }

    public class GitStorageTest {

        [Fact]
        public void EmptyProject() {
            string path = "path";
            Func<string, string> getPath = o => Path.Join(path, o);
            Action<Mock<SystemService>> setup = m => {
                m.Setup(m => m.GetLeaves(It.IsAny<string>())).Returns(new string[] {
                    getPath("file1"),
                    getPath("file2")
                });
                m.Setup(m => m.GetNodes(It.IsAny<string>())).Returns(new string[] {
                    getPath(".git"),
                    getPath("directory1"),
                    getPath("directory2")
                });
            };
            using (var configuration = new MoqSystemConfiguration(setup)) {
                configuration.Services.GetService<GitStorage>().EmptyProject("path");
                configuration.Moq.Verify(m => m.DeleteNode(It.IsAny<string>(), It.Is<bool>(o => o == true)), Times.Exactly(2));
                configuration.Moq.Verify(m => m.DeleteNode(It.Is<string>(o => o == getPath("directory1")), It.Is<bool>(o => o == true)), Times.Once);
                configuration.Moq.Verify(m => m.DeleteNode(It.Is<string>(o => o == getPath(".git")), It.Is<bool>(o => o == true)), Times.Never);
                configuration.Moq.Verify(m => m.DeleteLeaf(It.IsAny<string>()), Times.Exactly(2));
                configuration.Moq.Verify(m => m.DeleteLeaf(It.Is<string>(o => o == getPath("file1"))), Times.Once);
            }
        }

        private Action<Mock<SystemService>> SetupMetadataReading(out string id, out string text, out string repo) {
            id = Core.Utils.CreateId();
            text = "Lorem ipsum";
            repo = "http://git";
            var moq = new Mock<LeafReader>(null);
            var sequence = moq.SetupSequence(m => m.ReadLine());
            for (int i = 0; i < 2; i++)
                sequence = sequence.Returns("---")
                .Returns("id: " + id)
                .Returns("repo: " + repo)
                .Returns("index.leaves: true")
                .Returns("---");
            moq.Setup(m => m.ReadToEnd()).Returns(text);
            return m => {
                m.Setup(m => m.ReadLeaf(It.IsAny<string>())).Returns(moq.Object);
            };
        }

        [Fact]
        public void ReadProjectMetadata() {
            var project = new Document("name", null);
            var setup = SetupMetadataReading(out string id, out string text, out string repo);
            using (var configuration = new MoqSystemConfiguration(setup)) {
                configuration.Services.GetService<GitStorage>().ReadMetadata(project, null);
                Assert.Equal(id, project.Id);
                Assert.Equal(repo, project.Repo);
                Assert.True(project.IndexLeaves);
                Assert.False(project.IndexNodes);
            }
        }

        [Fact]
        public void ReadAndSetProjectMetadata() {
            var project = new Document("name", null);
            project.IndexLeaves = false;
            var setup = SetupMetadataReading(out string id, out string text, out string repo);
            using (var configuration = new MoqSystemConfiguration(setup)) {
                var storage = configuration.Services.GetService<GitStorage>();
                storage.ReadMetadata(project, null);
                Assert.Equal(id, project.Id);
                Assert.Equal(repo, project.Repo);
                Assert.True(project.IndexLeaves);
                Assert.False(project.IndexNodes);
                storage.SetMetadata(project);
                Assert.True(project.IndexLeaves);
                Assert.False(project.IndexNodes);
            }
        }

        [Fact]
        public void ReadProjectText() {
            var project = new Document("name", null);
            var setup = SetupMetadataReading(out string id, out string text, out string repo);
            using (var configuration = new MoqSystemConfiguration(setup)) {
                configuration.Services.GetService<GitStorage>().ReadDocument(project, null);
                Assert.Equal(text, project.Content);
            }
        }

        [Fact]
        public void ReadAndSetDocumentMetadata() {
            var project = new Document("name", null);
            project.IndexLeaves = false;
            var document = project.CreateDocument("name");
            var setup = SetupMetadataReading(out string id, out string text, out string repo);
            using (var configuration = new MoqSystemConfiguration(setup)) {
                var storage = configuration.Services.GetService<GitStorage>();
                storage.ReadMetadata(document, null);
                Assert.Equal(id, document.Id);
                Assert.True(document.IndexLeaves);
                Assert.False(document.IndexNodes);
                storage.SetMetadata(document);
                Assert.True(document.IndexLeaves);
                Assert.False(document.IndexNodes);
            }
        }

        [Fact]
        public void WriteNoIndexLeaf() {
            string userName = "Kevin";
            var user = new User(userName);
            string path = Path.Join("root", "git", "users", userName);
            var project = new Document("project", user);
            var leaf = new Document("leaf", user);
            leaf.Index = 3;
            leaf.Repo = "http://git";
            leaf.Content = "Lorem ipsum";
            using (var configuration = new MoqSystemConfiguration(null)) {
                var storage = configuration.Services.GetService<GitStorage>();
                storage.WriteDocument(leaf, storage.GetDocumentPath(project, leaf, Path.Combine(path, project.Name)), true);
                configuration.Moq.Verify(m => m.WriteLeaf(
                    It.Is<string>(o => o == Path.Combine(path, project.Name, leaf.Name + ".md")),
                    It.Is<string>(o => o == $"---\r\nid: {leaf.Id}\r\nrepo: {leaf.Repo}\r\n---\r\n{leaf.Content}")),
                    Times.Exactly(1));
            }
        }

        [Fact]
        public void WriteLeaf() {
            string userName = "Kevin";
            var user = new User(userName);
            string path = Path.Join("root", "git", "users", userName);
            var project = new Document("project", user);
            project.IndexLeaves = true;
            var leaf = new Document("leaf", user);
            leaf.Index = 3;
            leaf.Repo = "http://git";
            leaf.Content = "Lorem ipsum";
            using (var configuration = new MoqSystemConfiguration(null)) {
                var storage = configuration.Services.GetService<GitStorage>();
                storage.WriteDocument(leaf, storage.GetDocumentPath(project, leaf, Path.Combine(path, project.Name)), true);
                configuration.Moq.Verify(m => m.WriteLeaf(
                    It.Is<string>(o => o == Path.Combine(path, project.Name, "03." + leaf.Name + ".md")),
                    It.Is<string>(o => o == $"---\r\nid: {leaf.Id}\r\nrepo: {leaf.Repo}\r\n---\r\n{leaf.Content}")),
                    Times.Exactly(1));
            }
        }

        [Fact]
        public void WriteNoMetadaLeaf() {
            string userName = "Kevin";
            var user = new User(userName);
            string path = Path.Join("root", "git", "users", userName);
            var project = new Document("project", user);
            project.IndexLeaves = true;
            var leaf = new Document("leaf", user);
            leaf.Index = 3;
            leaf.Content = "Lorem ipsum";
            using (var configuration = new MoqSystemConfiguration(null)) {
                var storage = configuration.Services.GetService<GitStorage>();
                storage.WriteDocument(leaf, storage.GetDocumentPath(project, leaf, Path.Combine(path, project.Name)), true);
                configuration.Moq.Verify(m => m.WriteLeaf(
                    It.Is<string>(o => o == Path.Combine(path, project.Name, "03." + leaf.Name + ".md")),
                    It.Is<string>(o => o == $"---\r\nid: {leaf.Id}\r\n---\r\n{leaf.Content}")),
                    Times.Exactly(1));
            }
        }

        [Fact]
        public void WriteNode() {
            string userName = "Kevin";
            var user = new User(userName);
            string path = Path.Join("root", "git", "users", userName);
            var project = new Document("project", user);
            var node = new Document("node", user);
            node.Children = new ObservableCollection<Document>();
            node.Index = 3;
            node.Repo = "http://git";
            node.IndexLeaves = true;
            node.IndexNodes = false;
            node.Content = "Lorem ipsum";
            using (var configuration = new MoqSystemConfiguration(null)) {
                var storage = configuration.Services.GetService<GitStorage>();
                string directoryDocumentPath = Path.Combine(path, project.Name, node.Name, GitStorage.DirectoryDocumentName);
                storage.WriteDocument(node, directoryDocumentPath, true);
                //configuration.Moq.Verify(m => m.CreateNode(It.Is<string>(o => o == Path.Combine(path, project.Name, node.Name))), Times.Exactly(1));
                configuration.Moq.Verify(m => m.WriteLeaf(
                    It.Is<string>(o => o == directoryDocumentPath),
                    It.Is<string>(o => o == $"---\r\nid: {node.Id}\r\nrepo: {node.Repo}\r\nindex.leaves: True\r\n---\r\n{node.Content}")),
                    Times.Once);
            }
        }

        [Fact]
        public void WriteNoMetadataNode() {
            string userName = "Kevin";
            var user = new User(userName);
            string path = Path.Join("root", "git", "users", userName);
            var project = new Document("project", user);
            var node = new Document("node", user);
            node.Children = new ObservableCollection<Document>();
            node.Index = 3;
            node.Content = "Lorem ipsum";
            using (var configuration = new MoqSystemConfiguration(null)) {
                var storage = configuration.Services.GetService<GitStorage>();
                string directoryDocumentPath = Path.Combine(path, project.Name, node.Name, GitStorage.DirectoryDocumentName);
                storage.WriteDocument(node, directoryDocumentPath, true);
                //configuration.Moq.Verify(m => m.CreateNode(It.Is<string>(o => o == Path.Combine(path, project.Name, node.Name))), Times.Exactly(1));
                configuration.Moq.Verify(m => m.WriteLeaf(
                    It.Is<string>(o => o == Path.Combine(path, project.Name, node.Name, ".dir.md")),
                    It.Is<string>(o => o == $"---\r\nid: {node.Id}\r\n---\r\n{node.Content}")),
                    Times.Exactly(1));
            }
        }

        [Fact]
        public void WriteNoMetadataNoTextNode() {
            string userName = "Kevin";
            var user = new User(userName);
            string path = Path.Join("root", "git", "users", userName);
            var project = new Document("project", user);
            var node = new Document("node", user);
            node.Children = new ObservableCollection<Document>();
            node.Index = 3;
            using (var configuration = new MoqSystemConfiguration(null)) {
                var storage = configuration.Services.GetService<GitStorage>();
                string directoryDocumentPath = Path.Combine(path, project.Name, node.Name, GitStorage.DirectoryDocumentName);
                storage.WriteDocument(node, directoryDocumentPath, true);
                //configuration.Moq.Verify(m => m.CreateNode(It.Is<string>(o => o == Path.Combine(path, project.Name, node.Name))), Times.Exactly(1));
                configuration.Moq.Verify(m => m.WriteLeaf(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            }
        }

        [Fact]
        public void GetMetadataSimpleProject() {
            var document = new Document("name", null);
            var metadatas = Document.Metadatas.Where(o => o.Get(document) != null);
            Assert.Single(metadatas);
            Assert.Equal("id", metadatas.Single().Id);
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.False(configuration.Services.GetService<GitStorage>().NeedsDirectoryDocument(document));
            }
        }

        [Fact]
        public void GetMetadataSimpleProjectWithText() {
            var document = new Document("name", null);
            document.Content = "Text";
            var metadatas = Document.Metadatas.Where(o => o.Get(document) != null);
            Assert.Single(metadatas);
            Assert.Equal("id", metadatas.Single().Id);
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.True(configuration.Services.GetService<GitStorage>().NeedsDirectoryDocument(document));
            }
        }

        [Fact]
        public void GetMetadataIndexNodesProject() {
            var project = new Document("name", null);
            project.IndexNodes = true;
            var metadatas = Document.Metadatas.Where(o => o.Get(project) != null);
            Assert.Equal(2, metadatas.Count());
            var keys = metadatas.Select(o => o.Id);
            Assert.Contains("id", keys);
            Assert.Contains("index.nodes", keys);
            Assert.Equal(true.ToString(), metadatas.Single(o => o.Id == "index.nodes").Get(project));
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.True(configuration.Services.GetService<GitStorage>().NeedsDirectoryDocument(project));
            }
        }

        [Fact]
        public void GetMetadataIndexLeavesProject() {
            var project = new Document("name", null);
            project.IndexLeaves = true;
            var metadatas = Document.Metadatas.Where(o => o.Get(project) != null);
            Assert.Equal(2, metadatas.Count());
            var keys = metadatas.Select(o => o.Id);
            Assert.Contains("id", keys);
            Assert.Contains("index.leaves", keys);
            Assert.Equal(true.ToString(), metadatas.Single(o => o.Id == "index.leaves").Get(project));
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.True(configuration.Services.GetService<GitStorage>().NeedsDirectoryDocument(project));
            }
        }

        [Fact]
        public void GetMetadataIndexLeavesDocument() {
            var project = new Document("name", null);
            project.IndexLeaves = true; // No automatic inheritance
            var document = project.CreateDocument("name");
            var metadatas = Document.Metadatas.Where(o => o.Get(document) != null);
            Assert.Single(metadatas);
            Assert.Equal("id", metadatas.Single().Id);
            Assert.Null(Document.Metadatas.Single(o => o.Id == "index.leaves").Get(document));
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.False(configuration.Services.GetService<GitStorage>().NeedsDirectoryDocument(document));
            }
        }

        [Fact]
        public void GetMetadataIndexNodesDocument() {
            var project = new Document("name", null);
            project.IndexNodes = true; // No automatic inheritance
            var document = project.CreateDocument("name");
            var metadatas = Document.Metadatas.Where(o => o.Get(document) != null);
            Assert.Single(metadatas);
            Assert.Equal("id", metadatas.Single().Id);
            Assert.Null(Document.Metadatas.Single(o => o.Id == "index.nodes").Get(document));
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.False(configuration.Services.GetService<GitStorage>().NeedsDirectoryDocument(document));
            }
        }

        [Fact]
        public void GetIndexPrefix() {
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.Equal("03.", configuration.Services.GetService<GitStorage>().GetIndexPrefix(new Document("name", null) { Index = 3 }));
            }
        }

        [Fact]
        public void GetIndexedLeafDocumentPath() {
            string path = "path";
            var parent = new Document("parent", null);
            parent.IndexNodes = false;
            parent.IndexLeaves = true;
            var document = new Document("document", null, parent) { Index = 3 };
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.Equal(Path.Join(path, "03.document.md"), configuration.Services.GetService<GitStorage>().GetDocumentPath(parent, document, path));
            }
        }

        [Fact]
        public void GetIndexedNodeDocumentPath() {
            string path = "path";
            var parent = new Document("parent", null);
            parent.IndexNodes = true;
            parent.IndexLeaves = false;
            var document = new Document("document", null, parent) { Index = 3, Children = new ObservableCollection<Document>() };
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.Equal(Path.Join(path, "03.document"), configuration.Services.GetService<GitStorage>().GetDocumentPath(parent, document, path));
            }
        }

        [Fact]
        public void GetNotIndexedLeafDocumentPath() {
            string path = "path";
            var parent = new Document("parent", null);
            parent.IndexNodes = true;
            parent.IndexLeaves = false;
            var document = new Document("document", null, parent) { Index = 3 };
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.Equal(Path.Join(path, "document.md"), configuration.Services.GetService<GitStorage>().GetDocumentPath(parent, document, path));
            }
        }

        [Fact]
        public void GetNotIndexedNodeDocumentPath() {
            string path = "path";
            var parent = new Document("parent", null);
            parent.IndexNodes = false;
            parent.IndexLeaves = true;
            var document = new Document("document", null, parent) { Index = 3, Children = new ObservableCollection<Document>() };
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                Assert.Equal(Path.Join(path, "document"), configuration.Services.GetService<GitStorage>().GetDocumentPath(parent, document, path));
            }
        }

        [Fact]
        public void GetProjectName() {
            string name = "01.Project.md";
            string path = Path.Join("Storage", "Git", "Users", name);
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                var storage = configuration.Services.GetService<GitStorage>();
                Assert.Equal(name, storage.GetDocumentName(path, false, false, out int index));
                Assert.Equal(0, index);
            }
        }

        [Fact]
        public void GetDirectoryNameNoIndex() {
            string name = "03.Document.md";
            string path = Path.Join("A", "B", name);
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                var storage = configuration.Services.GetService<GitStorage>();
                Assert.Equal(name, storage.GetDocumentName(path, false, false, out int index));
                Assert.Equal(0, index);
            }
        }

        [Fact]
        public void GetDirectoryNameWithIndex() {
            string name = "03.Document.md";
            string path = Path.Join("A", "B", name);
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                var storage = configuration.Services.GetService<GitStorage>();
                Assert.Equal("Document.md", storage.GetDocumentName(path, false, true, out int index));
                Assert.Equal(3, index);
            }
        }

        [Fact]
        public void GetLeafNameNoIndex() {
            string name = "03.Document.md";
            string path = Path.Join("A", "B", name);
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                var storage = configuration.Services.GetService<GitStorage>();
                Assert.Equal("03.Document", storage.GetDocumentName(path, true, false, out int index));
                Assert.Equal(0, index);
            }
        }

        [Fact]
        public void GetLeafNameWithIndex() {
            string name = "03.Document.md";
            string path = Path.Join("A", "B", name);
            using (var configuration = new MoqSystemConfiguration(o => { })) {
                var storage = configuration.Services.GetService<GitStorage>();
                Assert.Equal("Document", storage.GetDocumentName(path, true, true, out int index));
                Assert.Equal(3, index);
            }
        }

        [Fact]
        public void GetLeaves() {
            string path = Path.Join("A", "B");
            Func<string, string> getPath = o => Path.Join(path, o);
            Action<Mock<SystemService>> setup = m => {
                m.Setup(m => m.GetLeaves(It.Is<string>(s => s == path))).Returns(new string[] {
                    getPath("file1.md"),
                    getPath("file2.md"),
                    getPath("invalidfile1"),
                    getPath("invalidfile2.mdr"),
                    getPath(".invalidfile3.md"),
                    getPath(".dir.md")
                });
            };
            using (var configuration = new MoqSystemConfiguration(setup)) {
                var leaves = configuration.Services.GetService<GitStorage>().GetLeaves(path);
                Assert.Collection(leaves, l => Assert.Equal(getPath("file1.md"), l), l => Assert.Equal(getPath("file2.md"), l));
            }
        }

        [Fact]
        public void GeNodes() {
            string path = Path.Join("A", "B");
            Func<string, string> getPath = o => Path.Join(path, o);
            Action<Mock<SystemService>> setup = m => {
                m.Setup(m => m.GetNodes(It.Is<string>(s => s == path), "*", SearchOption.TopDirectoryOnly)).Returns(new string[] {
                    getPath(".git"),
                    getPath("directory1"),
                    getPath("directory2")
                });
            };
            using (var configuration = new MoqSystemConfiguration(setup)) {
                var nodes = configuration.Services.GetService<GitStorage>().GetNodes(path);
                Assert.Collection(nodes, l => Assert.Equal(getPath("directory1"), l), l => Assert.Equal(getPath("directory2"), l));
            }
        }

        private void RotateRight<T>(IList<T> sequence, int count) {
            T temp = sequence[count - 1];
            sequence.RemoveAt(count - 1);
            sequence.Insert(0, temp);
        }

        private IEnumerable<IList<T>> Permutate<T>(IList<T> sequence, int count) {
            if (count == 1) yield return sequence;
            else {
                for (int i = 0; i < count; i++) {
                    foreach (var perm in Permutate(sequence, count - 1))
                        yield return perm;
                    RotateRight(sequence, count);
                }
            }
        }

        private IEnumerable<IList<T>> Permutate<T>(IList<T> sequence) => Permutate<T>(sequence, sequence.Count);

        [Fact]
        public void GetOrder() {
            var documents = new List<Document> {
                new Document("A", null) { Id = "1", Index = 0, Children = new ObservableCollection<Document>() },
                new Document("B", null) { Id = "2", Index = 0, Children = new ObservableCollection<Document>() },
                new Document("A", null) { Id = "3", Index = 0 },
                new Document("B", null) { Id = "4", Index = 0 },
                new Document("C", null) { Id = "5", Index = 1, Children = new ObservableCollection<Document>() },
                new Document("C", null) { Id = "6", Index = 1 },
                new Document("C", null) { Id = "7", Index = 2, Children = new ObservableCollection<Document>() },
                new Document("D", null) { Id = "8", Index = 2, Children = new ObservableCollection<Document>() },
                new Document("C", null) { Id = "9", Index = 3, Children = new ObservableCollection<Document>() }
            };
            using (var configuration = new MoqSystemConfiguration(null)) {
                var storage = configuration.Services.GetService<GitStorage>();
                foreach (var order in Permutate(documents)) {
                    int i = 1;
                    Assert.Collection(storage.OrderDocuments(documents),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id)),
                        l => Assert.Equal(i++, int.Parse(l.Id))
                    );
                }
            }
        }

        [Fact]
        public void LoadFile() {
            var user = new User("Kevin");
            var project = new Document("name", null);
            project.IndexLeaves = true;
            var setupReading = SetupMetadataReading(out string id, out string text, out string repo);
            Action<Mock<SystemService>> setupNaming = m => m.Setup(o => o.GetName(It.IsAny<string>())).Returns("03.leaf.md");
            using (var configuration = new MoqSystemConfiguration(setupReading, setupNaming)) {
                var leaf = configuration.Services.GetService<GitStorage>().LoadFile(user, project, null, true);
                Assert.Equal("leaf", leaf.Name);
                Assert.Equal(3, leaf.Index);
                Assert.Equal(id, leaf.Id);
                Assert.Equal(text, leaf.Content);
                Assert.True(leaf.IsLeaf);
            }
        }

        [Fact]
        public void LoadDirectory() {
            var user = new User("Kevin");
            var project = new Document("name", null);
            string path = Path.Join("path", project.Name, "directory");
            Func<string, string> getPath = o => Path.Combine(path, o);
            project.IndexNodes = true;
            var setupReading = SetupMetadataReading(out string id, out string text, out string repo);
            Action<Mock<SystemService>> setupSystem = m => {
                m.Setup(o => o.GetName(It.IsAny<string>())).Returns("03.directory");
                m.Setup(m => m.GetLeaves(It.Is<string>(s => s == path), It.Is<string>(o => o == GitStorage.DirectoryDocumentName)))
                    .Returns(new string[] { Path.Combine(path, GitStorage.DirectoryDocumentName) });
            };
            using (var configuration = new MoqSystemConfiguration(setupReading, setupSystem)) {
                var gitStorage = new Mock<GitStorage>(new GitStorageSettings { Local = false }, null, null, configuration.Moq.Object);
                gitStorage.Setup(o => o.GetChildren(It.IsAny<Document>(), It.Is<User>(o => o == user), path, true)).Returns(new ObservableCollection<Document> {
                    new Document("node1", user),
                    new Document("node2", user),
                    new Document("leaf1", user),
                    new Document("leaf2", user)
                });
                configuration.ServiceDescriptors.Add(new ServiceDescriptor(typeof(GitStorage), gitStorage.Object));
                var directory = configuration.Services.GetService<GitStorage>().LoadDirectory(user, project, path, true);
                configuration.Moq.Verify(m => m.ReadLeaf(It.Is<string>(o => o == Path.Combine(path, ".dir.md"))), Times.Exactly(2)); // Metadata + Text
                Assert.Equal("directory", directory.Name);
                Assert.Equal(3, directory.Index);
                Assert.Equal(id, directory.Id);
                Assert.Equal(text, directory.Content);
                Assert.False(directory.IsLeaf);
                Assert.NotNull(directory.Children);
                Assert.Equal(4, directory.Children.Count);
            }
        }
    }
}