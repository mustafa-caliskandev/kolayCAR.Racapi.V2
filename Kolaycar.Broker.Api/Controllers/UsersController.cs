using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UsersController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] UserAuthenticate userAuthenticate)
        {
            try
            {
                var user = await _userService.Authenticate(userAuthenticate.Username, userAuthenticate.Password, userAuthenticate.SecretKey);

                if (user == null)
                    return Ok(HttpResult<User>.Result(null, HttpStatusCode.OK, false, "Kullanıcı adı veya şifre yanlış!"));

                return Ok(HttpResult<User>.Result(user, HttpStatusCode.OK, true, "Giriş başarılı!"));
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@FailLogin}", ex.Message);
                return Ok(HttpResult<User>.Result(null, HttpStatusCode.OK, false, "Kullanıcı adı veya şifre yanlış!"));
            }
        }

        [AllowAnonymous]
        [HttpPost("exchange-token")]
        public async Task<IActionResult> ExchangeToken()
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress;
            if (clientIp?.IsIPv4MappedToIPv6 == true)
                clientIp = clientIp.MapToIPv4();

            var allowedIps = _configuration.GetSection("ExchangeToken:AllowedIps").Get<string[]>();
            if (clientIp == null || allowedIps == null ||
                !allowedIps.Any(value => IPAddress.TryParse(value, out var allowedIp) &&
                    (allowedIp.IsIPv4MappedToIPv6 ? allowedIp.MapToIPv4() : allowedIp).Equals(clientIp)))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    HttpResult<User>.Result(null, HttpStatusCode.Forbidden, false, "Bu IP adresinden erişime izin verilmiyor!"));
            }

            var externalToken = Request.Headers["X-External-Token"].FirstOrDefault();
            var user = await _userService.AuthenticateExternalToken(externalToken);

            if (user == null)
                return Unauthorized(HttpResult<User>.Result(null, HttpStatusCode.Unauthorized, false, "Token geçersiz veya acente bulunamadı!"));

            return Ok(HttpResult<User>.Result(user, HttpStatusCode.OK, true, "Giriş başarılı!"));
        }
    }
}
