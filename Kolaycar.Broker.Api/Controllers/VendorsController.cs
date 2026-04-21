using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Controllers
{
    //[BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _vendorService;
        private readonly ILocationService _locationService;
        private readonly IExtraService _extraService;
        private readonly IVehicleService _vehicleService;
        private readonly IAgencyService _agencyService;


        public VendorsController(IVendorService vendorService, ILocationService locationService, IExtraService extraService, IVehicleService vehicleService, IAgencyService agencyService)
        {
            _vendorService = vendorService;
            _locationService = locationService;
            _extraService = extraService;
            _vehicleService = vehicleService;
            _agencyService = agencyService;
        }

        [HttpGet]
        public async Task<ActionResult<CommonModels.HttpResult<IEnumerable<CommonModels.Vendor>>>> Get()
        {
            var agencyId = Convert.ToInt32(User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value);

            return CommonModels.HttpResult<IEnumerable<CommonModels.Vendor>>.Result(
                data: await _vendorService.GetVendors(agencyId),
                httpResultType: HttpStatusCode.OK,
                success: true);
        }

        [HttpGet("mapped")]
        public async Task<ActionResult<CommonModels.HttpResult<IEnumerable<CommonModels.Vendor>>>> GetVendorsByLocationId(int locationId)
        {
            var agencyId = Convert.ToInt32(User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value);

            return CommonModels.HttpResult<IEnumerable<CommonModels.Vendor>>.Result(
                data: await _vendorService.GetVendorsByLocationId(agencyId, locationId),
                httpResultType: HttpStatusCode.OK,
                success: true);
        }

        [HttpGet("list")]//aktif vendorlar
        public async Task<ActionResult<CommonModels.HttpResult<List<CommonModels.Vendor>>>> GetVendorsList()
        {
            return CommonModels.HttpResult<List<CommonModels.Vendor>>.Result(
                data: await _vendorService.GetActiveVendors(_agencyService.GetCurrentAgencyId()),
                httpResultType: HttpStatusCode.OK,
                success: true);
        }
        [HttpGet("all")]
        public async Task<ActionResult<CommonModels.HttpResult<List<CommonModels.Vendor>>>> GetAllVendors()
        {
            return CommonModels.HttpResult<List<CommonModels.Vendor>>.Result(
                data: await _vendorService.GetAllVendors(),
                httpResultType: HttpStatusCode.OK,
                success: true
                );
        }

        [HttpGet("locations")]
        public async Task<ActionResult<CommonModels.HttpResult<IEnumerable<CommonModels.Location>>>> Get(int languageId, int vendorId, string locationName)
        {
            var response = await _locationService.GetLocationsByVendorId(languageId, vendorId, locationName);
            return CommonModels.HttpResult<IEnumerable<CommonModels.Location>>.Result(
                data: response.Data as List<CommonModels.Location>,
                httpResultType: HttpStatusCode.OK,
                success: response.Success);
        }

        [HttpGet("locationDetails")]
        public async Task<ActionResult<CommonModels.HttpResult<CommonModels.Location>>> GetLocationDetails(int vendorId, int languageId, string locationCode)
        {
            var response = await _locationService.GetLocationDetailsFromVendorApi(languageId, vendorId, locationCode);
            return CommonModels.HttpResult<CommonModels.Location>.Result(
                data: response.Data as CommonModels.Location,
                httpResultType: HttpStatusCode.OK,
                success: response.Success
            );
        }

        [HttpGet("extras")]
        public async Task<ActionResult<CommonModels.HttpResult<List<CommonModels.Extra>>>> Get(int vendorId, string currencyCode, string languageCode, int rentalDuration, int pickupLocationId = 0)
        {
            if (vendorId == 0)
                return CommonModels.HttpResult<List<CommonModels.Extra>>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: "Lütfen geçerli bir vendorId değeri gönderin!");

            var agencyId = Convert.ToInt32(User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value);

            var serviceResponse = await _extraService.GetExtraListToVendorAPI(
                vendorId,
                agencyId,
                (CommonModels.CurrencyTypes)Enum.Parse(typeof(CommonModels.CurrencyTypes), currencyCode.ToUpper()),
                (CommonModels.LanguageTypes)Enum.Parse(typeof(CommonModels.LanguageTypes), languageCode.ToUpper()),
                rentalDuration);

            return CommonModels.HttpResult<List<CommonModels.Extra>>.Result(
                data: serviceResponse.Data as List<CommonModels.Extra>,
                httpResultType: HttpStatusCode.OK,
                success: serviceResponse.Success,
                message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }

        [HttpGet("vehicles")]
        public async Task<ActionResult<CommonModels.HttpResult<List<CommonModels.Vehicle>>>> Get(int vendorId)
        {

            if (vendorId == 0)
                return CommonModels.HttpResult<List<CommonModels.Vehicle>>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: "Lütfen geçerli bir vendorId değeri gönderin!");

            var serviceResponse = await _vehicleService.GetVehicleClassListFromVendorAPI(
                vendorId);

            return CommonModels.HttpResult<List<CommonModels.Vehicle>>.Result(
                data: serviceResponse.Data as List<CommonModels.Vehicle>,
                httpResultType: HttpStatusCode.OK,
                success: serviceResponse.Success,
                message: serviceResponse.ServiceMessage ?? serviceResponse.Message);
        }

        [HttpGet("settings")]
        public async Task<ActionResult<CommonModels.HttpResult<CommonModels.Settings>>> GetSettings(int vendorId)
        {
            if (vendorId == 0)
            {
                return HttpResult<CommonModels.Settings>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: "Lütfen geçerli bir vendorId değeri gönderin!");
            }

            var serviceResponse = await _vendorService.GetSettingsFromVendorApi(vendorId);
            if (serviceResponse != null && serviceResponse.Success)
            {
                return HttpResult<CommonModels.Settings>.Result(
                    data: serviceResponse.Data as CommonModels.Settings,
                    httpResultType: HttpStatusCode.OK,
                    success: true,
                    message: "");
            }


            return HttpResult<CommonModels.Settings>.Result(
                data: null,
                httpResultType: HttpStatusCode.OK,
                success: true,
                message: "");
        }

        [HttpPost("vendorvendors")]
        public async Task<ActionResult<CommonModels.HttpResult<List<CommonModels.VendorVendor>>>> GetVendorVendors(List<CommonModels.Vendor> vendors)
        {
            if (vendors != null && vendors.Count > 0)
            {
                var result = await _vendorService.GetVendorVendors(vendors: vendors);

                if (result != null && result.Count > 0)
                {
                    return HttpResult<List<CommonModels.VendorVendor>>.Result(
                        data: result,
                        httpResultType: HttpStatusCode.OK,
                        success: true,
                        message: ""
                        );
                }
            }

            return HttpResult<List<CommonModels.VendorVendor>>.Result(
                data: null,
                httpResultType: HttpStatusCode.BadRequest,
                success: false,
                message: "Liste bulunamadı"
                );
        }
        [HttpPost("Yolcu360Locations")]
        public async Task<ActionResult<HttpResult<List<Location>>>> GetLocationsFromYolcu360(Yolcu360LocationRequest yolcu360LocationRequest)
        {
            var response = await _locationService.GetLocationsByVendorId(yolcu360LocationRequest.LanguageId, yolcu360LocationRequest.VendorId, yolcu360LocationRequest.LocationName);

            return HttpResult<List<Location>>.Result(
            data: response.Data as List<Location>,
            httpResultType: HttpStatusCode.OK,
            success: true,
            message: ""
            );

            //return HttpResult<List<CommonModels.Location>>.Result(
            //data: new List<Location> { new Location { LocationName = "Test Sabiha", LocationId = 1, LocationCode = "15.65-65.98", Address = "Test", IsAirport = true , CityName = "İstanbul", MailAddress = "test@gmail.com", CityId = 1, CountryId = 2, PhoneNumber = "5647896532" } },
            //httpResultType: HttpStatusCode.OK,
            //success: true,
            //message: ""
            //);
        }
    }
}
