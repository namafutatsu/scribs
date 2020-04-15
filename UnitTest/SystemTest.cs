using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Scribs.Core.Services;
using Scribs.Core.Storages;
using Scribs.Core.Entities;

namespace Scribs.UnitTest {

    public class MoqSystemConfiguration : IDisposable {
        public ConfigurableServer Server { get; }
        public Mock<SystemService> Moq { get; }
        public IServiceProvider Services => Server.Services;
        public SystemService System => Services.GetService<SystemService>();

        public MoqSystemConfiguration(Action<Mock<SystemService>> setup) {
            Moq = new Mock<SystemService>();
            Moq.Setup(m => m.DeleteNode(It.IsAny<string>(), It.IsAny<bool>())).Callback(() => { });
            Moq.Setup(m => m.DeleteLeaf(It.IsAny<string>())).Callback(() => { });
            setup(Moq);
            var serviceDescriptor = new ServiceDescriptor(typeof(SystemService), Moq.Object);
            Server = new ConfigurableServer(sc => sc.Replace(serviceDescriptor));
        }

        public void Dispose() {
            Server.Dispose();
        }
    }

    public class SystemTest {

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
            id = Core.Utils.CreateGuid();
            text = "Lorem ipsum";
            repo = "http://git";
            var moq = new Mock<LeafReader>(null);
            moq.SetupSequence(m => m.ReadLine())
                .Returns("---")
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
                Assert.Equal(id, project.Key);
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
                Assert.Equal(id, project.Key);
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
                Assert.Equal(text, project.Text);
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
                Assert.Equal(id, document.Key);
                Assert.True(document.IndexLeaves);
                Assert.False(document.IndexNodes);
                storage.SetMetadata(document);
                Assert.True(document.IndexLeaves);
                Assert.False(document.IndexNodes);
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
            document.Text = "Text";
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
    }
}