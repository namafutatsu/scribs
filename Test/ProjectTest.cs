using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Scribs.Core;

namespace Scribs.Test {
    public class ProjectTest: IDisposable {
        ConfigurableServer server;

        public ProjectTest() {
            server = new ConfigurableServer();
        }

        public void Dispose() {
            server.Dispose();
        }

        [Fact]
        public async Task Test1Async() {
            using (var client = server.CreateClient()) {
                var value = await client.GetStringAsync("api/value");
                Assert.Equal("Hello world", value);
            }
        }
    }
}
