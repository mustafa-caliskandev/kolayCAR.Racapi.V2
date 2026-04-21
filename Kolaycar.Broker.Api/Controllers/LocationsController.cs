using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILanguageService _languageService;
        private readonly IAgencyService _agencyService;


        public LocationsController(
            ILocationService locationService,
            ILanguageService languageService,
            IAgencyService agencyService)
        {
            _locationService = locationService;
            _languageService = languageService;
            _agencyService = agencyService;
        }

        [HttpGet]
        public async Task<ActionResult<CommonModels.HttpResult<IEnumerable<CommonModels.Location>>>> Get(string languageId, string languageCode)
        {
            if (!string.IsNullOrEmpty(languageCode))
                languageId = ((int)languageCode.TrimNullSafe().ToUpper().ToEnum<CommonModels.LanguageTypes>() + 1).ToStringNullSafe();

            return CommonModels.HttpResult<IEnumerable<CommonModels.Location>>.Result(
                 data: await _locationService.GetLocations(languageId.ToIntNullSafe()),
                 httpResultType: HttpStatusCode.OK,
                 success: true);
        }

        [HttpGet]
        [Route("Cities")]
        public async Task<ActionResult<CommonModels.HttpResult<IEnumerable<City>>>> GetCities(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return CommonModels.HttpResult<IEnumerable<City>>.Result(
                   data: null,
                   httpResultType: HttpStatusCode.OK,
                   message: "Please send country code!",
                   success: true);
            }

            if (!await _locationService.ExistCountry(countryCode))
            {
                return CommonModels.HttpResult<IEnumerable<City>>.Result(
                         data: null,
                         httpResultType: HttpStatusCode.BadRequest,
                         message: "Invalid country code!",
                         success: false);
            }

            var data = await _locationService.GetCities(countryCode);

            return CommonModels.HttpResult<IEnumerable<City>>.Result(
                 data: data,
                 httpResultType: HttpStatusCode.OK,
                 success: true);
        }

        [HttpGet]
        [Route("Countries")]
        public async Task<ActionResult<CommonModels.HttpResult<IEnumerable<Countrylang>>>> GetCountries(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                return CommonModels.HttpResult<IEnumerable<Countrylang>>.Result(
                data: null,
                httpResultType: HttpStatusCode.OK,
                message: "Please send language code!",
                success: true);
            }

            string languageId = "";
            try
            {
                if (!string.IsNullOrEmpty(languageCode))
                    languageId = ((int)languageCode.TrimNullSafe().ToUpper().ToEnum<CommonModels.LanguageTypes>() + 1).ToStringNullSafe();
            }
            catch (Exception ex)
            {
                return CommonModels.HttpResult<IEnumerable<Countrylang>>.Result(
               data: null,
               httpResultType: HttpStatusCode.BadRequest,
               message: "Invalid language code!",
               success: false);
            }


            return CommonModels.HttpResult<IEnumerable<Countrylang>>.Result(
                 data: await _locationService.GetCountries(languageId.ToIntNullSafe()),
                 httpResultType: HttpStatusCode.OK,
                 success: true);
        }

        [HttpGet("GetAllLocations")]
        public async Task<ActionResult> GetAllLocations(string languageCode = "tr")
        {
            var language = await _languageService.Get(languageCode);
            var languageId = language != null ? language.Dilid : 1;
            var agencyId = _agencyService.GetCurrentAgencyId();

            var activeLocations = await _locationService.GetAllSearchLocations(agencyId);

            var result = activeLocations
                            .Where(l => l.LanguageId == languageId)
                            .OrderByDescending(l => l.IsAirport)
                            .ThenBy(l => l.LocationName)
                            .Select(l => new
                            {
                                cityName = l.CityName,
                                locationId = l.LocationId,
                                locationName = l.LocationName,
                                isPopular = l.IsPopular,
                                isPickup = l.IsPickup,
                                isAirport = l.IsAirport,
                            }).ToList();
            return Ok(new
            {
                data = result,
                success = true,
                resultCode = 200
            });
        }
        [HttpGet("MapYolcu360Locations")]
        public async Task<ActionResult> MapYolcu360Locations(int vendorId)
        {
            await _locationService.MapYolcu360Locations(vendorId);

            return Ok(new
            {
                data = "a",
                success = true,
                resultCode = 200
            });
        }

        [HttpGet("SetLocationCoordinate")]
        public async Task<ActionResult> SetLocationCoordinate()
        {
            await _locationService.SetLocationCoordinate();

            return Ok(new
            {
                data = "Coordinates updated successfully",
                success = true,
                resultCode = 200
            });
        }
    }
}
