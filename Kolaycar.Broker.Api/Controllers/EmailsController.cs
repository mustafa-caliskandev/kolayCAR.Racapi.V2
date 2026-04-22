using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace KolayCAR.Broker.API.Controllers
{
    //[BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]

        public async Task<IActionResult> Post(
            string fromTitle,
            string toMailAddress,
            string subject,
            string body,
            string replyTo)
        {
            var success = await _emailService.PostEmail(
                "ControllerMail",
                fromTitle,
                toMailAddress,
                subject,
                body,
                replyTo);

            return Ok(HttpResult<string>.Result(
                data: null,
                httpResultType: HttpStatusCode.OK,
                success: success,
                message: success ? "Email sent successfully!" : "Email could not be sent!"));
        }

        [AllowAnonymous]
        [HttpPost("longContent")]
        public async Task<IActionResult> Post([FromBody] PostEmailRequest postEmailRequest, bool isEncodedBody = false)
        {
            var success = await _emailService.PostEmail(
                postEmailRequest.TypeName,
                postEmailRequest.FromTitle,
                postEmailRequest.ToMailAddress,
                postEmailRequest.Subject,
                !isEncodedBody ? postEmailRequest.Body : HttpUtility.UrlDecode(postEmailRequest.Body),
                postEmailRequest.ReplyTo, postEmailRequest.IsCancelMail);
            return Ok(HttpResult<string>.Result(
                data: null,
                httpResultType: HttpStatusCode.OK,
                success: success,
                message: success ? "Email sent successfully!" : "Email could not be sent!"));
        }
    }
}

