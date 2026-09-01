using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password, string secretKey);
    }
    public class UserService : IUserService
    {
        private readonly IAgencyService _agencyService;
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings, IAgencyService agencyService)
        {
            _appSettings = appSettings.Value;
            _agencyService = agencyService;
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

                User user = new User
                {
                    Id = agency.AgencyId,
                    FirstName = agency.OwnerName,
                    LastName = agency.OwnerSurname,
                    Username = agency.Email,
                    Role = agency.UserRole
                };

                // Serilog.Log.Error("{@SuccessLogin}", user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.UserData, agency.AgencyCode.ToStringNullSafe())
                    }),
                    Expires = DateTime.UtcNow.AddDays(_appSettings.TokenExpireDay),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);

                return user;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@authenticateHeaderError}", $"username : {username}, password : {password}, secretkey : {secretKey.ToStringNullSafe()}, - {ex.Message}");
                return null;
            }

        }
    }
}
