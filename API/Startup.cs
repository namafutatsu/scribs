using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AutoMapper;
using Scribs.API.Models;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.API {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.Configure(Configuration)
                //.AddAutoMapper(typeof(Startup))
                .AddServices()
                .AddSingleton(ScribsMapper.GetMapper())
                .AddControllers();
            ConfigureAdditionalServices(services);
        }

        protected virtual void ConfigureAdditionalServices(IServiceCollection services) {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }

        public static class ScribsMapper {
            public static IMapper GetMapper() {
                var mapperConfig = new MapperConfiguration(
                        configuration => {
                            configuration.CreateMap<User, UserRegistrationModel>().ReverseMap();
                            configuration.IgnoreUnmapped();
                        });

                mapperConfig.AssertConfigurationIsValid();

                return mapperConfig.CreateMapper();
            }
        }

    }
    public static class MapperExtensions {

        private static void IgnoreUnmappedProperties(TypeMap map, IMappingExpression expr) {
            foreach (string propName in map.GetUnmappedPropertyNames()) {
                var srcPropInfo = map.SourceType.GetProperty(propName);

                var destPropInfo = map.DestinationType.GetProperty(propName);

                if (destPropInfo != null)
                    expr.ForMember(propName, opt => opt.Ignore());
            }
        }

        public static void IgnoreUnmapped(this IProfileExpression profile) {
            profile.ForAllMaps(IgnoreUnmappedProperties);
        }

        public static void IgnoreUnmapped(this IProfileExpression profile, Func<TypeMap, bool> filter) {
            profile.ForAllMaps((map, expr) =>
            {
                if (filter(map)) {
                    IgnoreUnmappedProperties(map, expr);
                }
            });
        }

        public static void IgnoreUnmapped(this IProfileExpression profile, Type src, Type dest) {
            profile.IgnoreUnmapped((TypeMap map) => map.SourceType == src && map.DestinationType == dest);
        }

        public static void IgnoreUnmapped<TSrc, TDest>(this IProfileExpression profile) {
            profile.IgnoreUnmapped(typeof(TSrc), typeof(TDest));
        }
    }
}
