using KaffeMaskineProjekt.DomainModels;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KaffeMaskineProjekt.ApiService.Services
{
    public sealed class TokenService (IConfiguration config)
    {
        public string Create(User user)
        {
            string secretKeyVal = config["JwT:Secret"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyVal));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                    new Claim(JwtRegisteredClaimNames.Name, user.Name), 
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(config.GetValue<int>("JwT:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Issuer = config["JwT:Issuer"],
                Audience = config["JwT:Audience"]
            };

            // Add role claims if the user has any roles
            if (user.Roles != null && user.Roles.Any())
            {
                var claims = user.Roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
                tokenDescriptor.Subject.AddClaims(claims);
            }

            var tokenHandler = new JsonWebTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return token;
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}
