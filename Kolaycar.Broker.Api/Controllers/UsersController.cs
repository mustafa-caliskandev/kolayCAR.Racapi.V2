using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;
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
    }
}
