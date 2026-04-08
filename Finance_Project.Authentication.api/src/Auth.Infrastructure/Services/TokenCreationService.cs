using Auth.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Infrastructure.Services
{
    /// <summary>
    /// TokenCreationService
    /// </summary>
    public class TokenCreationService(IConfiguration configuration) : ITokenCreationService
    {
        /// <summary>Generates the token.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="username">The username.</param>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        public Tuple<string, DateTime> GenerateToken(string userId, string username, string role)
        {
            // Key-value pair that states a fact about the user.
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

            // Holds the instruction on how to sign the token.
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            DateTime expiration = DateTime.UtcNow.AddMinutes(double.Parse(configuration["Jwt:Expiration"]));

            var tokenDescriptor = new JwtSecurityToken
                (
                    issuer: configuration["Jwt:Issuer"], // Who made the token
                    audience: configuration["Jwt:Audience"], // Who is the token for
                    claims: claims,
                    expires: expiration,
                    signingCredentials: credentials
                );

            return new Tuple<string, DateTime> (new JwtSecurityTokenHandler().WriteToken(tokenDescriptor), expiration);
        }
    }
}