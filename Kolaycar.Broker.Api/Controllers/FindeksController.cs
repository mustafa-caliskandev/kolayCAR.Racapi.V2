using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.FindeksRequestBase;

namespace KolayCAR.Broker.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FindeksController : ControllerBase
    {
        private readonly IFindeksService _findeksService;
        public FindeksController(IFindeksService findeksService)
        {
            _findeksService = findeksService;
        }

        [HttpPost]
        [Route("IsActiveFindeksReportExists")]
        public async Task<ServiceResponseBase> IsActiveFindeksReportExists([FromBody] GetIsActiveFindeksReportRequest getActiveFindeksReportRequest)
        {
            var result = await _findeksService.GetIsActiveFindeksReportExists(getActiveFindeksReportRequest);

            if (result is null)
                return new ServiceResponseBase { Data = null, Success = false, Message = "Bir hata oluştu!" };

            return result;
        }
        [HttpPost]
        [Route("FindeksPhoneIdListRequest")]
        public async Task<ServiceResponseBase> FindeksPhoneIdListRequest([FromBody] GetPhoneIdListRequest getPhoneIdListRequest)
        {
            var result = await _findeksService.GetFindeksPhoneIdList(getPhoneIdListRequest);

            if (result is null)
                return new ServiceResponseBase { Data = null, Success = false, Message = "Bir hata oluştu!" };

            return result;
        }
        [HttpPost]
        [Route("FindeksReportRequest")]
        public async Task<ServiceResponseBase> FindeksReportRequest([FromBody] GetFindeksReportRequest getFindeksReportRequest)
        {
            var result = await _findeksService.GetFindeksReport(getFindeksReportRequest);
            if (result is null)
                return new ServiceResponseBase { Data = null, Success = false, Message = "Bir hata oluştu!" };

            return result;
        }
        [HttpPost]
        [Route("FindeksPinRenew")]
        public async Task<ServiceResponseBase> FindeksPinRenew([FromBody] CreatePinRenewRequest createPinRenewRequest)
        {
            var result = await _findeksService.FindeksPinRenew(createPinRenewRequest);

            if (result is null)
                return new ServiceResponseBase { Data = null, Success = false, Message = "Bir hata oluştu!" };

            return result;
        }
        [HttpPost]
        [Route("FindeksPinConfirm")]
        public async Task<ServiceResponseBase> FindeksPinConfirm([FromBody] ConfirmPinRequest confirmPinRequest)
        {
            var result = await _findeksService.FindeksPinConfirm(confirmPinRequest);

            if (result is null)
                return new ServiceResponseBase { Data = null, Success = false, Message = "Bir hata oluştu!" };

            return result;
        }
        [HttpPost]
        [Route("IsVehicleSuitableForCustomer")]
        public async Task<ServiceResponseBase> IsVehicleSuitableForCustomer([FromBody] IsVehicleSuitableForCustomer isVehicleSuitableForCustomer)
        {
            var result = await _findeksService.IsVehicleSuitableForCustomer(isVehicleSuitableForCustomer);

            if (result is null)
                return new ServiceResponseBase { Data = null, Success = false, Message = "Bir hata oluştu!" };

            return result;
        }
    }
}
