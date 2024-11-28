using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IeltsTestWeb.Utils
{
    public class JwtUtil
    {
        private readonly IConfiguration configuration;
        public JwtUtil(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string GenerateAccessToken(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("type","access_token")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        public string GenerateRefreshToken()
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("type","refresh_token")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public static bool IsJwtValid(string token, string type)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                if (!jwtHandler.CanReadToken(token))
                    return false;

                var jwtToken = jwtHandler.ReadJwtToken(token);
                var expClaim = jwtToken.Payload.Exp;
                var typeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == type);

                if (!expClaim.HasValue || typeClaim == null)
                    return false;

                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expClaim.Value).UtcDateTime;
                var tokenType = typeClaim.Value;

                return expirationTime > DateTime.UtcNow && tokenType.Equals(type);
            }
            catch (Exception)
            {
                return false; 
            }
        }
    }
}
