using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly ISummaryService _summaryService;
        private readonly IAgencyService _agencyService;
        private readonly IExtraService _extraService;
        private readonly IResTokenService _resTokenService;
        private UserRoles _userRole;

        public SummaryController(ISummaryService summaryService, IAgencyService agencyService, IExtraService extraService, IResTokenService resTokenService)
        {
            _summaryService = summaryService;
            _agencyService = agencyService;
            _extraService = extraService;
            _userRole = _agencyService.GetCurrentUserRole();
            _resTokenService = resTokenService;
        }

        [HttpGet]
        public async Task<ActionResult<HttpResult<object>>> Get(
            string extraList,
            string reservationToken,
            int? memberId,
            string couponCode,
            string languageCode,
            float paidAmount,
            bool highAmountDiscountActive
            )
        {
            if (string.IsNullOrWhiteSpace(reservationToken))
                return HttpResult<object>.Result(
                data: null,
                httpResultType: HttpStatusCode.BadRequest,
                success: false,
                message: "Check the reservationToken parameter!");

            var reservationTokenObj = await _resTokenService.GetReservationTokenByUniqueId(reservationToken);

            var getSummaryRequest = new GetSummaryRequest
            {
                ExtraList = extraList,
                ReservationToken = reservationToken,
                MemberId = memberId,
                CouponCode = couponCode,
                LanguageCode = languageCode.ToStringNullSafe().ToUpper(),
                PaidAmount = paidAmount,
                HighAmountDiscountActive = highAmountDiscountActive
            };

            var getExtrasRequest = new GetExtrasRequest
            {
                ReservationToken = reservationToken.TrimNullSafe(),
                LanguageCode = getSummaryRequest.LanguageCode,
                CurrencyCode = reservationTokenObj.CurrencyType.ToString()
            };

            var extraServiceResponse = !string.IsNullOrEmpty(getSummaryRequest.ExtraList) ?
                        await _extraService.GetExtras(getExtrasRequest) :
                        new ServiceResponseBase
                        {
                            Success = true,
                            Data = new GetExtrasResponse
                            {
                                Extras = new List<Extra>(),
                            }
                        };

            var getExtrasResponse = extraServiceResponse.Data as GetExtrasResponse;

            var serviceResponse = await _summaryService.GetSummary(getSummaryRequest, getExtrasResponse);

            if (serviceResponse.Success)
            {
                GetSummaryResponseRestricted restrictedServiceResponse = new GetSummaryResponseRestricted();
                if (_userRole == UserRoles.External)
                {
                    var summaryVehicle = serviceResponse.Data as GetSummaryResponse;
                    restrictedServiceResponse.Vehicle = _agencyService.ChechAgencyRestricted<RestrictedVehicle>(summaryVehicle.Vehicle);
                    restrictedServiceResponse.Extras = _agencyService.ChechAgencyRestrictedList<ReservationExtra, RestrictedReservationExtra>(summaryVehicle.Extras as List<ReservationExtra>);
                }

                object getSummaryResponse = _userRole == UserRoles.External ? restrictedServiceResponse : serviceResponse.Data;

                return HttpResult<object>.Result(
                    data: getSummaryResponse,
                    httpResultType: HttpStatusCode.OK,
                    success: serviceResponse.Success,
                    message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
            }

            return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.OK,
                    success: serviceResponse.Success,
                    message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }
    }
}
