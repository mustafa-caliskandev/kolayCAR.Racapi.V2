using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password, string secretKey);
        Task<User> AuthenticateExternalToken(string externalToken);
    }
    public class UserService : IUserService
    {
        private readonly IAgencyService _agencyService;
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _configuration;

        public UserService(IOptions<AppSettings> appSettings, IAgencyService agencyService, IConfiguration configuration)
        {
            _appSettings = appSettings.Value;
            _agencyService = agencyService;
            _configuration = configuration;
        }

        public async Task<User> Authenticate(string username, string password, string secretKey)
        {
            try
            {
                if (EncryptionHelper.Decrypt(username) == "")
                    return null;

                var agency = await _agencyService.GetAgencyByUsernamePassword(username, password);

                if (agency == null)
                {
                    Serilog.Log.Error("{@NullAgency}", $"username : {username}, password : {password}, secretkey : {secretKey.ToStringNullSafe()}");
                    return null;
                }

                if (!string.IsNullOrEmpty(secretKey) && password == EncryptionHelper.Decrypt(secretKey) && agency.UserRole == UserRoles.External)
                    agency.UserRole = UserRoles.Agency;

                else if (string.IsNullOrEmpty(secretKey))
                {
                    if (agency.UserRole != UserRoles.Admin)
                    {
                        agency.UserRole = agency.UserRole != UserRoles.Accountancy ? UserRoles.External : UserRoles.Accountancy;
                    }
                }

                return CreateUserWithToken(agency);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@authenticateHeaderError}", $"username : {username}, password : {password}, secretkey : {secretKey.ToStringNullSafe()}, - {ex.Message}");
                return null;
            }

        }

        public async Task<User> AuthenticateExternalToken(string externalToken)
        {
            if (string.IsNullOrWhiteSpace(externalToken))
                return null;

            var externalSecret = _configuration["ExchangeSecretKey"];
            if (string.IsNullOrWhiteSpace(externalSecret))
                externalSecret = _configuration["ExternalJwt:SecretKey"];

            if (string.IsNullOrWhiteSpace(externalSecret))
            {
                Serilog.Log.Error("ExchangeSecretKey is not configured.");
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(externalSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
                };

                var principal = tokenHandler.ValidateToken(externalToken.Trim(), validationParameters, out var validatedToken);
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !string.Equals(jwtToken.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                    return null;

                var agencyCode = principal.Claims
                    .FirstOrDefault(x => string.Equals(x.Type, "agencyCode", StringComparison.OrdinalIgnoreCase))?
                    .Value?
                    .Trim();

                var agency = await _agencyService.GetAgencyByCode(agencyCode);
                return agency == null ? null : CreateUserWithToken(agency);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("{@ExternalJwtAuthenticationError}", ex.Message);
                return null;
            }
        }

        private User CreateUserWithToken(Agency agency)
        {
            var user = new User
            {
                Id = agency.AgencyId,
                FirstName = agency.OwnerName,
                LastName = agency.OwnerSurname,
                Username = agency.Email,
                Role = agency.UserRole
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.UserData, agency.AgencyCode.ToStringNullSafe())
                }),
                Expires = DateTime.UtcNow.AddDays(_appSettings.TokenExpireDay),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            user.Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            return user;
        }
    }
}
