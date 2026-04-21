using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class SurveysController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public SurveysController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> Post([FromBody] PostEmailRequest postEmailRequest, int surveyId)
        {
            if (postEmailRequest != null &&
                !string.IsNullOrEmpty(postEmailRequest.FromTitle) &&
                !string.IsNullOrEmpty(postEmailRequest.ToMailAddress) &&
                !string.IsNullOrEmpty(postEmailRequest.Subject) &&
                !string.IsNullOrEmpty(postEmailRequest.Body))
            {
                postEmailRequest.Body = HttpUtility.UrlDecode(postEmailRequest.Body);
                postEmailRequest.Body = postEmailRequest.Body.Replace("{{EncryptedSurveyId}}", EncryptionHelper.Encrypt(surveyId.ToString()));

                Serilog.Log.Error("{@PostSurveyEmail}", postEmailRequest);

                var emailSuccess = await _emailService.PostEmail(
                    "SurveyMail",
                    postEmailRequest.FromTitle,
                    postEmailRequest.ToMailAddress,
                    postEmailRequest.Subject,
                    postEmailRequest.Body,
                    postEmailRequest.ReplyTo);

                return Ok(HttpResult<string>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.OK,
                    success: emailSuccess,
                    message: emailSuccess ? "Survey sent successfully!" : "Survey sent error!"));
            }

            return Ok(HttpResult<string>.Result(
                data: null,
                httpResultType: HttpStatusCode.OK,
                success: false,
                message: "Survey could not send!"));
        }
    }
}
