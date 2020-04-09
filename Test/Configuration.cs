using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scribs.API;

namespace Scribs.Test {

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
                .ConfigureServices(sc => sc.AddSingleton<Action<IServiceCollection>>(configureAction))
                .UseStartup<ConfigurableStartup>();
            return builder;
        }
    }
}
