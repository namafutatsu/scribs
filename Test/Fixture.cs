using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.Test {
    public class Fixture: IDisposable {
        public ConfigurableServer Server { get; }

        public Fixture() {
            var userFactory = new Mock<Factory<User>>(null);
            userFactory.Setup(m => m.GetByName(It.Is<string>(o => o == "gdrtf"))).Returns(new User("gdrtf") { Mail = "gdrtf@mail.com" });
            var serviceDescriptor = new ServiceDescriptor(typeof(Factory<User>), userFactory.Object);
            Server = new ConfigurableServer(sc => sc.Replace(serviceDescriptor));
        }

        public void Dispose() {
            Server.Dispose();
        }
    }
}
