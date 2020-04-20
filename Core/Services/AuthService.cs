using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Scribs.Core.Entities;

namespace Scribs.Core.Services {
    public class AuthService {
        public const string SECRET = "e5aaac48-caf0-4d45-bf0b-cf4f0b2ace9b";

        Factory<User> factory;

        public AuthService(Factory<User> factory) {
            this.factory = factory;
        }

        public async Task<User> Identify(ClaimsPrincipal principal) {
            var user = await factory.GetAsync(principal.Identity.Name);
            if (user == null)
                throw new Exception("User not found");
            return user;
        }
        public static string GenerateToken(string id) {
            var secret = SECRET;
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            var issuer = "*";
            var audience = "*";
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, id) }),
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
