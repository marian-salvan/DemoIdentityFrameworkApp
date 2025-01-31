using DemoBackend.Configuration;
using DemoBackend.Constants;
using DemoBackend.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoBackend.Helpers
{
    public static class TokenProviderHelper
    {
        public static (string, DateTime) GenerateAccessToken(AuthenticationSettings authenticationSettings, UserEntity user, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authenticationSettings.TokenSecret));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>
            {
                new(ApplicationClaims.Id, user.Id),
                new(ApplicationClaims.Email,!string.IsNullOrEmpty(user.Email) ? user.Email : string.Empty),
                new(ApplicationClaims.Age, user.Age.ToString()),
                new(ApplicationClaims.Role, role.ToUpper()),
                new(ClaimTypes.Role, role.ToUpper()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            //if you are under 18 you get access to premium content
            if (user.Age < 18)
            {
                claimsForToken.Add(new(ApplicationClaims.AccessToPremium, ApplicationClaims.AccessToPremium));
            }

            var expirationDate = DateTime.UtcNow.AddMinutes(60);

            var jwtSecurityToken = new JwtSecurityToken(
                          authenticationSettings.Issuer,
                          authenticationSettings.Audience,
                          claimsForToken,
                          DateTime.UtcNow,
                          expirationDate,
                          signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler()
               .WriteToken(jwtSecurityToken);

            return (tokenToReturn, expirationDate);
        }

        public static string GenerateRefreshToken(AuthenticationSettings authenticationSettings)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authenticationSettings.RefreshTokenSecret));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expirationDate = DateTime.UtcNow.AddHours(12);

            var jwtSecurityToken = new JwtSecurityToken(
                          authenticationSettings.Issuer,
                          authenticationSettings.Audience,
                          null,
                          DateTime.UtcNow,
                          expirationDate,
                          signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler()
               .WriteToken(jwtSecurityToken);

            return tokenToReturn;
        }

        public static bool ValidateRefreshToken(AuthenticationSettings authenticationSettings, string refreshToken)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authenticationSettings.RefreshTokenSecret));

            var validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = securityKey,
                ValidIssuer = authenticationSettings.Issuer,
                ValidAudience = authenticationSettings.Audience,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var result = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);

            return result != null;
        }
    }
}
