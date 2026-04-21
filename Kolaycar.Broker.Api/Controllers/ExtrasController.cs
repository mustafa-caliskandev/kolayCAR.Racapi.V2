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
    public class ExtrasController : ControllerBase
    {
        private readonly IAgencyService _agencyService;
        private readonly IExtraService _extraService;
        private readonly IResTokenService _resTokenService;
        private UserRoles _userRole;

        public ExtrasController(IExtraService extraService, IAgencyService agencyService, IResTokenService resTokenService)
        {
            _extraService = extraService;
            _agencyService = agencyService;
            _resTokenService = resTokenService;
            _userRole = _agencyService.GetCurrentUserRole();
        }

        [HttpGet]
        public async Task<ActionResult<HttpResult<object>>> Get(
            string reservationToken,
            string languageCode)
        {
            if (string.IsNullOrWhiteSpace(reservationToken))
                return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: "Check your reservationToken!");

            var serviceResponse = await _extraService.GetExtras(new GetExtrasRequest
            {
                ReservationToken = reservationToken,
                LanguageCode = languageCode.ToStringNullSafe().ToUpper()
            });



            if (!serviceResponse.Success)
            {
                return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.OK,
                    success: serviceResponse.Success,
                    message: serviceResponse.ServiceMessage ?? serviceResponse.Message);

            }

            var restrictedServiceResponse = new GetExtrasResponseRestricted();
            var extrasVehicle = serviceResponse.Data as GetExtrasResponse;

            if (_userRole == UserRoles.External)
            {
                if (extrasVehicle != null)
                {
                    restrictedServiceResponse.Vehicle =
                        _agencyService.ChechAgencyRestricted<RestrictedVehicle>(extrasVehicle.Vehicle);
                    restrictedServiceResponse.Extras =
                        _agencyService.ChechAgencyRestrictedList<Extra, RestrictedExtra>(extrasVehicle.Extras);
                }
            }

            var getExtrasResponse = _userRole == UserRoles.External ? restrictedServiceResponse : (object)extrasVehicle;

            return HttpResult<object>.Result(
                data: getExtrasResponse,
                httpResultType: HttpStatusCode.OK,
                success: serviceResponse.Success,
                message: serviceResponse.ServiceMessage ?? serviceResponse.Message);

        }
        [HttpGet]
        [Route("GetPremiumPacket")]
        public async Task<ActionResult<HttpResult<object>>> GetPremiumPacket(string reservationToken)
        {
            if (string.IsNullOrWhiteSpace(reservationToken))
                return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: "Check your reservationToken!");

            var token = await _resTokenService.GetReservationTokenByUniqueId(reservationToken);

            if (token == null)
            {
                return HttpResult<object>.Result(
                     data: null,
                     httpResultType: HttpStatusCode.OK,
                     success: false,
                     message: "Check your reservationToken!");
            }

            var premiumExtra = await _extraService.GetPremiumPacketByReservationToken(token);

            return HttpResult<object>.Result(
                data: premiumExtra,
                httpResultType: HttpStatusCode.OK,
                success: true,
                message: "");

        }


        [HttpGet("list")]
        public async Task<HttpResult<object>> Get(int vendorId, string currencyCode, string languageCode, int rentalDuration)
        {
            var serviceResponse = await _extraService.GetMappedExtras(
                vendorId,
                currencyCode != null ? currencyCode.TrimNullSafe().ToUpper().ToEnum<CurrencyTypes>() : CurrencyTypes.TRY,
                languageCode != null ? languageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>() : LanguageTypes.TR,
                rentalDuration,
                false);

            var resultExtras = _agencyService.ChechAgencyRestrictedList<Extra, ExtraListItem>(serviceResponse.Data as List<Extra>);

            return HttpResult<object>.Result(
                data: resultExtras,
                httpResultType: HttpStatusCode.OK,
                success: serviceResponse.Success,
                message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }

        //[HttpGet("specialproductsbylocationvendor")]
        //public async Task<HttpResult<List<Models.SpecialProductPrice>>> Get(int vendorId = 0, int productId = 0)
        //{
        //    if (vendorId != 0)
        //    {
        //        var serviceResponse = await _extraService.GetSpecialProductsByLocationVendor(vendorId, productId);

        //        if (serviceResponse.Success)
        //        {
        //            return HttpResult<List<Models.SpecialProductPrice>>.Result(
        //                data: serviceResponse.Data as List<Models.SpecialProductPrice>,
        //                httpResultType: HttpStatusCode.OK,
        //                success: true
        //                );
        //        }
        //    }
        //    return HttpResult<List<Models.SpecialProductPrice>>.Result(
        //            data: null,
        //            httpResultType: HttpStatusCode.OK,
        //            success: false,
        //            message: "vendorId parameter can not be null or equels 0");
        //}
    }
}
