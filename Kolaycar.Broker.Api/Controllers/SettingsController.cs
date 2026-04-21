using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingService _settingService;

        public SettingsController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        public async Task<ActionResult<HttpResult<Settings>>> Get(
            string reservationToken,
            string languageCode,
            string currencyCode)
        {
            var serviceResponse = await _settingService.GetSettings(new GetSettingsRequest
            {
                LanguageCode = languageCode.ToUpper(),
                ReservationToken = reservationToken,
                CurrencyCode = currencyCode.ToUpper()
            });

            return HttpResult<Settings>.Result(
                data: serviceResponse.Data as Settings,
                httpResultType: HttpStatusCode.OK,
                success: serviceResponse.Success,
                message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }
    }
}
