using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AgenciesController : ControllerBase
    {
        private readonly IAgencyService _agencyService;

        public AgenciesController(IAgencyService agencyService)
        {
            _agencyService = agencyService;
        }

        [HttpGet]
        public async Task<ActionResult<HttpResult<object>>> Get(
            string reservationToken,
            string languageCode)
        {
            var agencyIdToken = Convert.ToInt32(User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value);
            var serviceResponse = await _agencyService.GetAgencies(new GetAgenciesRequest
            {
                LanguageCode = languageCode.ToUpper(),
                ReservationToken = reservationToken,
                AgencyId = agencyIdToken
            });

            var resultAgency = _agencyService.ChechAgencyRestricted<RestrictedAgency>(serviceResponse.Data);

            return HttpResult<object>.Result(
                data: resultAgency,
                httpResultType: HttpStatusCode.OK,
                success: serviceResponse.Success,
                message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }
    }
}
