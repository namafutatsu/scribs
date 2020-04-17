using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scribs.API;

namespace Scribs.IntegrationTest {

    public class ConfigurableStartup : Startup {
        private readonly Action<IServiceCollection> configureAction;

        public ConfigurableStartup(IConfiguration configuration, Action<IServiceCollection> configureAction)
            : base(configuration) => this.configureAction = configureAction;

        protected override void ConfigureAdditionalServices(IServiceCollection services) {
            configureAction(services);
        }
    }

    public class ConfigurableServer : TestServer {
        public ConfigurableServer(Action<IServiceCollection> configureAction = null) : base(CreateBuilder(configureAction)) {
        }

        private static IWebHostBuilder CreateBuilder(Action<IServiceCollection> configureAction) {
            if (configureAction == null) {
                configureAction = (sc) => { };
            }
            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc.AddSingleton(configureAction))
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetAssembly(typeof(ConfigurableStartup)).Location))
                    .AddJsonFile("appsettings.json")
                    .Build()
                )
                .UseStartup<ConfigurableStartup>()
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            return builder;
        }
    }
}
