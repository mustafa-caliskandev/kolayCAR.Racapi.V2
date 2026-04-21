using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Logo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin, UserRoles.Accountancy)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class LogoController : ControllerBase
    {
        private readonly ILogoService _logoService;
        private readonly ILogoServiceV2 _logoServiceV2;
        private readonly IAgencyService _agencyService;

        public LogoController(
            ILogoService logoService,
            ILogoServiceV2 logoServiceV2,
            IAgencyService agencyService)
        {
            _logoService = logoService;
            _logoServiceV2 = logoServiceV2;
            _agencyService = agencyService;
        }

        [HttpPost("GetPaymentList")]
        public async Task<IActionResult> GetPaymentList([FromBody] GetPaymentListRequest request)
        {
            var agencyId = _agencyService.GetCurrentAgencyId();
            //var agencyCode = User.Claims.Where(x => x.Type == ClaimTypes.UserData).FirstOrDefault().Value;
            //var jwtToken = _agencyService.GetCurrentBearerToken();

            var result = await _logoService.GetPaymentList(request);

            return Ok(result);
        }

        [HttpPost("GetInvoiceList")]
        public async Task<IActionResult> GetInvoiceList([FromBody] GetInvoiceListRequest request)
        {
            var agencyId = _agencyService.GetCurrentAgencyId();
            //var agencyCode = User.Claims.Where(x => x.Type == ClaimTypes.UserData).FirstOrDefault().Value;
            //var jwtToken = _agencyService.GetCurrentBearerToken();

            var result = request.Version.Equals("1.0")
                        ? await _logoService.GetInvoiceList(request)
                        : await _logoServiceV2.GetInvoices(request);
            return Ok(result);

            //if (request.Version.Equals("1.0"))
            //{
            //    var resultV1 = await _logoService.GetInvoiceList(request);
            //    return Ok(resultV1);
            //}

            //var resultV2 = await _logoServiceV2.GetInvoices(request);
            //return Ok(resultV2);
        }

        [HttpPost("GetCanceledInvoiceList")]
        public async Task<IActionResult> GetCanceledInvoiceList([FromBody] GetInvoiceListRequest request)
        {
            var agencyId = _agencyService.GetCurrentAgencyId();

            request.OnlyCanceledReservations = true;

            var result = await _logoServiceV2.GetInvoices(request);

            return Ok(result);
        }

        [HttpGet("Vendors")]
        public async Task<IActionResult> GetVendors()
        {
            var result = await _logoService.GetVendors();

            return Ok(result);
        }

    }
}
