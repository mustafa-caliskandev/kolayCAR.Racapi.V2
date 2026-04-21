using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KolayCAR.Broker.Infrastructure.Extensions
{
    public static class AirtuerkExtensions
    {
        public static string DecodeTokenForAirSystemLoginWithBase64(string token, string secretKey)
        {
            try
            {
                //string secret = "";
                //var hmac = new System.Security.Cryptography.HMACSHA512(Convert.FromBase64String(secret));
                //var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
                var hmac = new HMACSHA512(Convert.FromBase64String(secretKey));
                //var key = Encoding.ASCII.GetBytes(secretKey);
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var validations = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                var claims = handler.ValidateToken(token, validations, out var tokenSecure);
                var agencyObj = claims.FindFirst("currentUser").Value;

                return agencyObj;
            }
            catch (SecurityTokenExpiredException ex)
            {
                Serilog.Log
                    .ForContext("token", token)
                    .ForContext("systemname", secretKey)
                    .Error("{@AirSystemLoginError}", ex.Message);
            }
            catch (Exception ex)
            {
                Serilog.Log
                    .ForContext("token", token)
                    .ForContext("systemname", secretKey)
                    .Error("{@AirSystemLoginError}", ex.Message);
            }
            return string.Empty;
        }
        public static string DecodeTokenForAirSystemLogin(string token, string secretKey)
        {
            try
            {
                //string secret = "";
                //var hmac = new System.Security.Cryptography.HMACSHA512(Convert.FromBase64String(secret));
                var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
                //var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
                //var key = Encoding.ASCII.GetBytes(secretKey);
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var validations = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = mySecurityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                var claims = handler.ValidateToken(token, validations, out var tokenSecure);
                var agencyObj = claims.FindFirst("currentUser").Value;

                return agencyObj;
            }
            catch (SecurityTokenExpiredException ex)
            {
                Serilog.Log
                    .ForContext("token", token)
                    .ForContext("systemname", secretKey)
                    .Error("{@AirSystemLoginError}", ex.Message);
            }
            catch (Exception ex)
            {
                Serilog.Log
                    .ForContext("token", token)
                    .ForContext("systemname", secretKey)
                    .Error("{@AirSystemLoginError}", ex.Message);
            }
            return string.Empty;
        }
        public static string GenerateToken()
        {
            //User user = new User
            //{
            //    Id = agency.AgencyId,
            //    FirstName = agency.OwnerName,
            //    LastName = agency.OwnerSurname,
            //    Username = agency.Email,
            //    Role = agency.UserRole
            //};
            var secret = "OTA5Mzk5ZjNjMzQyNjNhZTRkODM4ZDI3MjQ0OGYzYjY3N2ZjMTc0ZGQxNzAwYzhhY2ViOTUyMzQwNWUzNGUwNA==";
            var tokenHandler = new JwtSecurityTokenHandler();
            var hmac = new System.Security.Cryptography.HMACSHA512(Convert.FromBase64String(secret));
            var key = Encoding.ASCII.GetBytes(secret);

            var payload = new JwtPayload
            {
                { "agencyNumber ", "255120 "},
                { "userName", "255120"},
                { "pnrOwner", "" }
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("agencyNumber", "255120"),
                    new Claim("userName", "255120"),

                    //new Claim(ClaimTypes.UserData, "255120")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(hmac.Key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var result = tokenHandler.WriteToken(token);

            return result;
        }
    }
}
