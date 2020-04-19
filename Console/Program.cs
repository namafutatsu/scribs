using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scribs.Core;
using Scribs.Core.Entities;
using Scribs.Core.Services;
using Scribs.Core.Storages;

namespace Scribs.Console {
    class Program {
        static void Main(string[] args) {
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();
            var services = new ServiceCollection().Configure(configuration).AddServices().AddServices().BuildServiceProvider();

            var user = new User("test");
            services.GetFactory<User>().Create(user);

            //var gdrtf = services.GetFactory<User>().GetByName("gdrtf");
            //gdrtf.Mail = "test@test.test";
            //services.GetFactory<User>().Update(gdrtf);
            //var project = services.GetService<GitStorage>().Load(gdrtf.Name, "test");
            //services.GetService<JsonStorage>().Save(project);
            //project = services.GetService<JsonStorage>().Load(gdrtf.Name, "test");
            //services.GetFactory<Document>().Create(project);


            //var deserializer = new DataContractJsonSerializer(typeof(Document));
            //var m = new MemoryStream(Encoding.UTF8.GetBytes(@"{""Key"":""fezstgert"",""Name"":""test2"",""Index"":""3""}"));
            //var doc = deserializer.ReadObject(m) as Document;
        }
    }
}
