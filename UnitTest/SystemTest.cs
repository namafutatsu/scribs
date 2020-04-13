using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;
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