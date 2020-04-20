using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using AutoMapper;
using Scribs.API.Models;
using Scribs.Core.Entities;
using Scribs.Core.Services;
using System.Threading.Tasks;

namespace Scribs.API {
    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.Authority = "*";
                    options.Audience = "*"; 
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ClockSkew = TimeSpan.FromMinutes(5),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtManager.SECRET)),
                        RequireSignedTokens = true,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ValidateAudience = false,
                        //ValidAudience = "api://default",
                        ValidateIssuer = false,
                        //ValidIssuer = "https://{yourOktaDomain}/oauth2/default",
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                    // todo :
                    options.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration();
                });
            services.Configure(Configuration)
                .AddServices()
                .AddSingleton(ScribsMapper.GetMapper())
                .AddControllers();
            IdentityModelEventSource.ShowPII = true;
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
            app.UseAuthentication();
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
                            configuration.CreateMap<Document, ProjectModel>().ReverseMap();
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

    public static class ClaimsPrincipalExtensions {

        public static async Task<User> Identify(this ClaimsPrincipal principal, Factory<User> userFactory) {
            var user = await userFactory.GetAsync(principal.Identity.Name);
            if (user == null)
                throw new Exception("User not found");
            return user;
        }

        public static Task<User> Identify(this ClaimsPrincipal principal, Factories factories) {
            return principal.Identify(factories.Get<User>());
        }
    }

    public static class JwtManager {

        public const string SECRET = "e5aaac48-caf0-4d45-bf0b-cf4f0b2ace9b";

        public static string GenerateToken(string id) {
            var secret = SECRET;
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            var issuer = "*";
            var audience = "*";
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {new Claim(ClaimTypes.NameIdentifier, id)}),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
