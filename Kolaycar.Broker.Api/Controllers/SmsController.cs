using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    //[BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _smsService;

        public SmsController(ISmsService smsService)
        {
            _smsService = smsService;
        }

        public async Task<IActionResult> Post([FromBody] PostSmsRequest postSmsRequest)
        {
            var result = await _smsService.PostSms(postSmsRequest);

            return Ok(HttpResult<string>.Result(
                data: null,
                httpResultType: HttpStatusCode.OK,
                success: result,
                message: result ? "Sms sent successfully!" : "An error has occurred!"));
        }
    }
}
