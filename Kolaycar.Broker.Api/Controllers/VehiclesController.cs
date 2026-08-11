using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IAgencyService _agencyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationService _configurationService;
        private readonly ILabelService _labelService;
        private readonly IParameterService _parameterService;
        public VehiclesController(
            IVehicleService vehicleService,
            IAgencyService agencyService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IConfigurationService configurationService,
            ILabelService labelService,
            IParameterService parameterService
           )
        {
            _vehicleService = vehicleService;
            _agencyService = agencyService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _configurationService = configurationService;
            _labelService = labelService;
            _parameterService = parameterService;
        }

        [HttpGet]
        [HttpGet("BulkSearch")]
        public async Task<ActionResult<HttpResult<object>>> Get(
            int vendorType,
            string apiKey,
            string apiPassword,
            string apiClientId,
            string languageCode,
            string currencyCode,
            int pickupLocationId,
            int returnLocationId,
            string pickupDate,
            string returnDate,
            string pickupTime,
            string returnTime,
            int driverAge,
            string userToken = null,
            string couponCode = null,
            string sessionCode = null,
            bool disableTimeout = false,
            string Guid = "",
            string verbose = null
            )
        {
            var getVehiclesRequest = new GetVehiclesRequest
            {
                VendorType = (VendorTypes)(object)(vendorType),
                ApiKey = apiKey.ToStringNullSafe(),
                ApiPassword = apiPassword.ToStringNullSafe(),
                ApiClientId = apiClientId.ToStringNullSafe(),
                LanguageCode = languageCode.ToStringNullSafe().ToUpper(),
                CurrencyCode = currencyCode.ToStringNullSafe().ToUpper(),
                PickupLocationId = pickupLocationId,
                ReturnLocationId = returnLocationId,
                PickupDate = pickupDate,
                ReturnDate = returnDate,
                PickupTime = pickupTime,
                ReturnTime = returnTime,
                UserToken = userToken,
                CouponCode = couponCode,
                DisableTimeout = disableTimeout.ToBoolNullSafe(),
                Guid = Guid,
                Verbose = verbose
            };

            if (_configuration["AppSettings:AvailibityRequestLogging"].ToBoolNullSafe())
            {
                Serilog.Log.Error("{@AvailibityRequest}", JsonConvert.SerializeObject(getVehiclesRequest));
            }

            var maxAllowedAdvanceReservationDays = await _configurationService.GetConfigurationByDegisken("MaxAllowedAdvanceReservationDays");
            var label = await _labelService.GetLabelByCodeAndLanguageId("MaxAllowedAdvanceReservationDays", (int)getVehiclesRequest.LanguageCode.ToEnum<LanguageTypes>() + 1);

            var checkGetVehiclesRequestResult = ReservationHelper.CheckGetVehiclesRequestRequireProps(getVehiclesRequest, maxAllowedAdvanceReservationDays, label);

            if (!string.IsNullOrEmpty(checkGetVehiclesRequestResult))
                return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: checkGetVehiclesRequestResult);

            var sessionId = string.IsNullOrEmpty(sessionCode)
                                    ? _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? ""
                                    : sessionCode;

            var isBulkSearch = Request.Path.Value?.TrimEnd('/').EndsWith("/BulkSearch", System.StringComparison.OrdinalIgnoreCase) == true;
            var serviceResponse = isBulkSearch || string.IsNullOrEmpty(getVehiclesRequest.ApiKey)
                                ? await _vehicleService.GetVehiclesWihtBulkRequest(getVehiclesRequest, _agencyService.GetCurrentAgencyId(), sessionId, disableTimeOut: disableTimeout)
                                : await _vehicleService.GetVehicles(getVehiclesRequest, _agencyService.GetCurrentAgencyId(), sessionId, disableTimeOut: disableTimeout);

            object CreateResponseData(object vehicles) => string.IsNullOrWhiteSpace(verbose)
                ? vehicles
                : new VerboseVehiclesData { Vehicles = vehicles, VendorLogs = serviceResponse.VendorLogs };

            if (serviceResponse.ServiceCode == ResultCodes.Timeout.ToString())
                return HttpResult<object>.Result(
                    data: CreateResponseData(null),
                    httpResultType: HttpStatusCode.OK,
                    success: false,
                    message: serviceResponse.Message,
                    resultCode: ResultCodes.Timeout);

            if (serviceResponse.Success == false)
                return HttpResult<object>.Result(
                    data: CreateResponseData(null),
                    httpResultType: HttpStatusCode.OK,
                    success: false,
                    message: serviceResponse.Message,
                    resultCode: serviceResponse.IsTimeOut ? ResultCodes.Timeout : ResultCodes.VehicleNotAvailable);

            var resultVehicles = _agencyService.ChechAgencyRestrictedList<Vehicle, RestrictedVehicle>(serviceResponse.Data as List<Vehicle>);

            return HttpResult<object>.Result(
                 data: CreateResponseData(resultVehicles),
                 httpResultType: HttpStatusCode.OK,
                 success: serviceResponse.Success,
                 message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }



        [HttpPost]
        [Route("vendor")]
        public async Task<ActionResult<HttpResult<object>>> GetByVendor([FromBody] PostVendorVehicleDto postVendorVehicleDto)
        {
            var getVehiclesRequest = new GetVehiclesRequest
            {
                VendorType = (VendorTypes)(object)(postVendorVehicleDto.VendorType),
                ApiKey = postVendorVehicleDto.EncryptedKeys
                    ? EncryptionHelper.Decrypt(postVendorVehicleDto.ApiKey.ToStringNullSafe())
                    : postVendorVehicleDto.ApiKey.ToStringNullSafe(),
                ApiPassword = postVendorVehicleDto.EncryptedKeys
                    ? EncryptionHelper.Decrypt(postVendorVehicleDto.ApiPassword.ToStringNullSafe())
                    : postVendorVehicleDto.ApiPassword.ToStringNullSafe(),
                ApiClientId = postVendorVehicleDto.ApiClientId.ToStringNullSafe(),
                LanguageCode = postVendorVehicleDto.LanguageCode.ToStringNullSafe().ToUpper(),
                CurrencyCode = postVendorVehicleDto.CurrencyCode.ToStringNullSafe().ToUpper(),
                PickupLocationId = postVendorVehicleDto.PickupLocationId,
                ReturnLocationId = postVendorVehicleDto.ReturnLocationId,
                PickupDate = postVendorVehicleDto.PickupDate,
                ReturnDate = postVendorVehicleDto.ReturnDate,
                PickupTime = postVendorVehicleDto.PickupTime,
                ReturnTime = postVendorVehicleDto.ReturnTime,
                UserToken = postVendorVehicleDto.UserToken,
                CouponCode = postVendorVehicleDto.CouponCode,
                ApiLocationCode = string.IsNullOrEmpty(postVendorVehicleDto.ApiLocationCode) ? "" : postVendorVehicleDto.ApiLocationCode
            };

            var maxAllowedAdvanceReservationDays = await _configurationService.GetConfigurationByDegisken("MaxAllowedAdvanceReservationDays");
            var label = await _labelService.GetLabelByCodeAndLanguageId("MaxAllowedAdvanceReservationDays", (int)getVehiclesRequest.LanguageCode.ToEnum<LanguageTypes>());

            var checkGetVehiclesRequestResult = ReservationHelper.CheckGetVehiclesRequestRequireProps(getVehiclesRequest, maxAllowedAdvanceReservationDays, label);

            if (!string.IsNullOrEmpty(checkGetVehiclesRequestResult))
                return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: checkGetVehiclesRequestResult);


            var sessionId = _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? "";

            var serviceResponse = string.IsNullOrEmpty(getVehiclesRequest.ApiKey)
                                ? await _vehicleService.GetVehiclesWihtBulkRequest(getVehiclesRequest, _agencyService.GetCurrentAgencyId(), sessionId)
                                : await _vehicleService.GetVehicles(getVehiclesRequest, _agencyService.GetCurrentAgencyId(), sessionId);

            if (serviceResponse.ServiceCode == ResultCodes.Timeout.ToString())
                return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.OK,
                    success: false,
                    message: serviceResponse.Message,
                    resultCode: ResultCodes.Timeout);

            if (serviceResponse.Success == false)
                return HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.OK,
                    success: false,
                    message: serviceResponse.Message,
                    resultCode: serviceResponse.IsTimeOut ? ResultCodes.Timeout : ResultCodes.VehicleNotAvailable);

            var resultVehicles = _agencyService.ChechAgencyRestrictedList<Vehicle, RestrictedVehicle>(serviceResponse.Data as List<Vehicle>);

            return HttpResult<object>.Result(
                 data: resultVehicles,
                 httpResultType: HttpStatusCode.OK,
                 success: serviceResponse.Success,
                 message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }

        [HttpGet("list")]
        public async Task<ActionResult<HttpResult<List<VehicleListItem>>>> Get(string languageCode, bool getOnlyMatched = false)
        {
            var languageType = !string.IsNullOrEmpty(languageCode) ? languageCode.ToUpper().ToEnum<LanguageTypes>() : LanguageTypes.TR;
            var vehicles = await _vehicleService.GetLocalVehiclesList(languageType, getOnlyMatched);

            return HttpResult<List<VehicleListItem>>.Result(
                data: vehicles,
                httpResultType: HttpStatusCode.OK,
                success: true,
                message: string.Empty);
        }
    }
}
