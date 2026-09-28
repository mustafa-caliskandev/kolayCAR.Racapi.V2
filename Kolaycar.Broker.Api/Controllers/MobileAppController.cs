using AutoMapper;
using kolayCAR.Broker.AWS.Services;
using KolayCAR.Broker.API.Creators;
using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.CommentDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.CurrenciesDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.GetDetailsDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;
using KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ReservationDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos;
using KolayCAR.Broker.API.Models.MobileAppModels;
using KolayCAR.Broker.API.Models.MobileAppModels.Enums;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.FindeksRequestBase;
using Comment = KolayCAR.Broker.Domain.Models.Comment;
using Extra = KolayCAR.Broker.Domain.Models.Extra;
using RentalCondition = KolayCAR.Broker.Domain.Models.RentalCondition;

namespace KolayCAR.Broker.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class MobileAppController : ControllerBase
    {
        #region Constructor & Definations

        private readonly AppSettings _appSettings;
        private readonly BrokerContext _context;
        private readonly Creator _creator;

        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IVehicleService _vehicleService;
        private readonly IAgencyService _agencyService;
        private readonly ICurrencyService _currencyService;
        private readonly ICouponService _couponService;
        private readonly IContentService _contentService;
        private readonly IConfigurationService _configurationService;
        private readonly IExtraService _extraService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IFindeksService _findeksService;
        private readonly ILanguageService _languageService;
        private readonly ILocationService _locationService;
        private readonly ILabelService _labelService;
        private readonly IMapper _mapper;
        private readonly IMobileService _mobileService;
        private readonly IParameterService _parameterService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentSettingService _paymentSettingService;
        private readonly IReservationDetailService _reservationDetailService;
        private readonly IReservationPaymentDetailService _reservationPaymentDetailService;
        private readonly IReservationService _reservationService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IReservationTokenService _reservationTokenService;
        private readonly UserRoles _userRole = UserRoles.MobileAPP;
        private readonly ISurveyService _surveyService;
        private readonly IVendorService _vendorService;
        private readonly IVendorOfficeService _vendorOfficeService;
        private readonly IResTokenService _resTokenService;
        private readonly IVendorContactInformationService _vendorContactInformationService;

        private readonly IAWSService _awsService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Tedarikçi çağrıları I/O-bound; bağlantı havuzu geldikten sonra sabit 30 slot gereksiz dar kalıyordu.
        private static readonly int _vendorConcurrency = Environment.ProcessorCount * 16;
        private static readonly SemaphoreSlim _vendorSemaphore = new(_vendorConcurrency, _vendorConcurrency);
        private static readonly TimeSpan _vendorSlotWaitTimeout = TimeSpan.FromSeconds(3);

        public MobileAppController(
            IOptions<AppSettings> appSettings,
            BrokerContext context,
            Creator creator,

            IConfiguration configuration,
            IMemoryCacheService memoryCacheService,
            IHttpContextAccessor httpContextAccessor,

            IVehicleService vehicleService,
            IAgencyService agencyService,
            ICurrencyService currencyService,
            ICouponService couponService,
            IContentService contentService,
            IConfigurationService configurationService,
            IExtraService extraService,
            IExchangeRateService exchangeRateService,
            IFindeksService findeksService,
            ILabelService labelService,
            ILanguageService languageService,
            ILocationService locationService,
            IMapper mapper,
            IMobileService mobileService,
            IPaymentService paymentService,
            IPaymentSettingService paymentSettingService,
            IParameterService parameterService,
            IReservationDetailService reservationDetailService,
            IReservationPaymentDetailService reservationPaymentDetailService,
            IReservationService reservationService,
            IReservationStepsService reservationStepsService,
            IReservationTokenService reserTokenService,
            ISurveyService surveyService,
            IVendorService vendorService,
            IVendorOfficeService vendorOfficeService,
            IResTokenService resTokenService,
            IVendorContactInformationService vendorContactInformationService,

            IAWSService awsService,
            IServiceScopeFactory serviceScopeFactory
            )
        {
            _appSettings = appSettings.Value;
            _context = context;
            _creator = creator;

            _configuration = configuration;
            _memoryCacheService = memoryCacheService;
            _httpContextAccessor = httpContextAccessor;

            _vehicleService = vehicleService;
            _agencyService = agencyService;
            _currencyService = currencyService;
            _exchangeRateService = exchangeRateService;
            _surveyService = surveyService;
            _mapper = mapper;
            _mobileService = mobileService;
            _extraService = extraService;
            _couponService = couponService;
            _contentService = contentService;
            _reservationDetailService = reservationDetailService;
            _reservationPaymentDetailService = reservationPaymentDetailService;
            _reservationStepsService = reservationStepsService;
            _reservationTokenService = reserTokenService;
            _configurationService = configurationService;
            _labelService = labelService;
            _locationService = locationService;
            _vendorService = vendorService;
            _paymentService = paymentService;
            _paymentSettingService = paymentSettingService;
            _reservationService = reservationService;
            _findeksService = findeksService;
            _languageService = languageService;
            _vendorOfficeService = vendorOfficeService;
            _parameterService = parameterService;
            _resTokenService = resTokenService;
            _vendorContactInformationService = vendorContactInformationService;

            _awsService = awsService;
            _serviceScopeFactory = serviceScopeFactory;
        }
        #endregion

        #region Get Vehicles Action
        [HttpPost]
        [Route("GetVehicles")]
        public async Task<IActionResult> GetVehicles(GetVehicleDto getVehicleDto)
        {
            try
            {
                var dateTimeString = $"{getVehicleDto.PickupDate} {getVehicleDto.PickupTime}";

                if (!DateTime.TryParse(dateTimeString, out var pickupDateTime) || pickupDateTime <= DateTime.Now)
                {
                    return BadRequest(new
                    {
                        data = "",
                        success = false,
                        resultCode = ResultCodes.VehicleNotAvailable,
                        message = "Invalid or past pickup date provided. Please enter a valid future date and time."
                    });
                }

                if (getVehicleDto.PickupLocationId <= 0 || getVehicleDto.ReturnLocationId <= 0)
                {
                    return BadRequest(new
                    {
                        data = "",
                        success = false,
                        resultCode = ResultCodes.Error,
                        message = "PickupLocationId and ReturnLocationId cannot be empty."
                    });
                }

                var languageId = await _memoryCacheService.GetLanguageId(getVehicleDto.LanguageCode);

                if (string.IsNullOrEmpty(getVehicleDto.SessionCode))
                {
                    getVehicleDto.SessionCode = _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? "";
                }

                var langId = languageId > 0 ? languageId : 1;
                var (locationId, vehicles) = await FetchVehiclesForLocationAsync(getVehicleDto, getVehicleDto.PickupLocationId, getVehicleDto.ReturnLocationId);

                if (vehicles == null || !vehicles.Any())
                {
                    return Ok(new
                    {
                        data = new VehicleListDto(),
                        success = false,
                        resultCode = ResultCodes.Error
                    });
                }

                var vehicleList = await MobileVehicles(langId, getVehicleDto.SessionCode, getVehicleDto, vehicles, locationId, _memoryCacheService);
                vehicleList.PickupLocationId = locationId;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var agencyService = scope.ServiceProvider.GetRequiredService<IAgencyService>();
                        await _awsService.PushListingData(vehicles, null, await agencyService.GetCurrentAgencyType());
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error("{@AWSPushListingData}", ex.Message);
                    }
                });

                return Ok(new
                {
                    data = vehicleList,
                    success = true,
                    resultCode = ResultCodes.Success
                });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GetVehiclesError}", $"{ex.Message}-{ex.StackTrace}-{ex.InnerException?.Message}");

                return Ok(new
                {
                    data = new VehicleListDto(),
                    success = false,
                    resultCode = ResultCodes.Error
                });
            }
        }

        [HttpPost]
        [Route("GetVehiclesList")]
        public async Task<IActionResult> GetVehiclesList(GetVehicleDto getVehicleDto)
        {
            try
            {
                var dateTimeString = $"{getVehicleDto.PickupDate} {getVehicleDto.PickupTime}";

                if (!DateTime.TryParse(dateTimeString, out var pickupDateTime) || pickupDateTime <= DateTime.Now)
                {
                    return BadRequest(new
                    {
                        data = "",
                        success = false,
                        resultCode = ResultCodes.VehicleNotAvailable,
                        message = "Invalid or past pickup date provided. Please enter a valid future date and time."
                    });
                }

                if (getVehicleDto.PickupLocationIds == null || !getVehicleDto.PickupLocationIds.Any())
                {
                    return BadRequest(new
                    {
                        data = "",
                        success = false,
                        resultCode = ResultCodes.Error,
                        message = "PickupLocationId list cannot be empty."
                    });
                }

                var languageId = await _memoryCacheService.GetLanguageId(getVehicleDto.LanguageCode);

                if (string.IsNullOrEmpty(getVehicleDto.SessionCode))
                {
                    getVehicleDto.SessionCode = _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? "";
                }

                // Phase 1: Fetch vehicles from all locations IN PARALLEL (vendor API calls)
                var firstLocationId = getVehicleDto.PickupLocationIds.First();
                var fetchTasks = getVehicleDto.PickupLocationIds.Select(locationId =>
                {
                    var returnLocationId = locationId == firstLocationId ? getVehicleDto.ReturnLocationId : locationId;
                    return FetchVehiclesForLocationAsync(getVehicleDto, locationId, returnLocationId);
                });

                var fetchResults = await Task.WhenAll(fetchTasks);

                // Phase 2: Transform vehicles SEQUENTIALLY (uses controller-scoped services with DbContext)
                var vehicleListResults = new List<VehicleListDto>();
                var langId = languageId > 0 ? languageId : 1;
                foreach (var (locationId, vehicles) in fetchResults)
                {
                    if (vehicles == null || !vehicles.Any())
                        continue;

                    var vehicleList = await MobileVehicles(langId, getVehicleDto.SessionCode, getVehicleDto, vehicles, locationId, _memoryCacheService);
                    vehicleList.PickupLocationId = locationId;
                    vehicleListResults.Add(vehicleList);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceScopeFactory.CreateScope();
                            var agencyService = scope.ServiceProvider.GetRequiredService<IAgencyService>();
                            await _awsService.PushListingData(vehicles, null, await agencyService.GetCurrentAgencyType());
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error("{@AWSPushListingData}", ex.Message);
                        }
                    });
                }

                if (!vehicleListResults.Any())
                {
                    return Ok(new
                    {
                        data = new List<VehicleListDto>(),
                        success = false,
                        resultCode = ResultCodes.Error
                    });
                }

                var result = new
                {
                    data = vehicleListResults,
                    success = true,
                    resultCode = ResultCodes.Success
                };

                return CompressedJson(result);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GetVehiclesListError}", $"{ex.Message}-{ex.StackTrace}-{ex.InnerException?.Message}");

                return Ok(new
                {
                    data = new List<VehicleListDto>(),
                    success = false,
                    resultCode = ResultCodes.Error
                });
            }
        }

        private async Task<(int locationId, List<Vehicle> vehicles)> FetchVehiclesForLocationAsync(GetVehicleDto getVehicleDto, int pickupLocationId, int returnLocationId)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var memoryCacheService = scope.ServiceProvider.GetRequiredService<IMemoryCacheService>();
                var vehicles = await GetVehiclesFromService(getVehicleDto, pickupLocationId, returnLocationId, memoryCacheService);
                return (pickupLocationId, vehicles);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@FetchVehiclesForLocationAsync}", $"Location {pickupLocationId} fetch failed: {ex.Message}");
                return (pickupLocationId, null);
            }
        }

        private IActionResult CompressedJson(object data)
        {
            var acceptEncoding = Request.Headers["Accept-Encoding"].ToString();
            var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (acceptEncoding.Contains("br", StringComparison.OrdinalIgnoreCase))
            {
                using var output = new MemoryStream();
                using (var br = new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true))
                {
                    br.Write(jsonBytes, 0, jsonBytes.Length);
                }
                Response.Headers["Content-Encoding"] = "br";
                Response.Headers["Vary"] = "Accept-Encoding";
                return File(output.ToArray(), "application/json");
            }
            else if (acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
            {
                using var output = new MemoryStream();
                using (var gz = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
                {
                    gz.Write(jsonBytes, 0, jsonBytes.Length);
                }
                Response.Headers["Content-Encoding"] = "gzip";
                Response.Headers["Vary"] = "Accept-Encoding";
                return File(output.ToArray(), "application/json");
            }

            return File(jsonBytes, "application/json");
        }

        private async Task<List<Vehicle>> GetVehiclesFromService(GetVehicleDto getVehicleDto, int pickupLocationId, int returnLocationId, IMemoryCacheService memoryCacheService)
        {
            var resultList = new List<Vehicle>();
            var vendors = await memoryCacheService.GetLocationVendors(pickupLocationId);

            var tasks = new List<Task<List<Vehicle>>>();
            foreach (var vendor in vendors)
            {
                var request = new GetVehiclesRequest
                {
                    VendorType = (VendorTypes)vendor.VendorType,
                    ApiKey = vendor.ApiKey,
                    ApiPassword = vendor.ApiPassword,
                    ApiClientId = vendor.ApiClientId,
                    ApiLocationCode = vendor.VendorLocationCode,
                    LanguageCode = getVehicleDto.LanguageCode,
                    CurrencyCode = getVehicleDto.CurrencyCode,
                    PickupLocationId = pickupLocationId,
                    ReturnLocationId = returnLocationId,
                    PickupDate = getVehicleDto.PickupDate,
                    ReturnDate = getVehicleDto.ReturnDate,
                    PickupTime = getVehicleDto.PickupTime,
                    ReturnTime = getVehicleDto.ReturnTime,
                    CouponCode = getVehicleDto.CouponCode,
                    SessionCode = getVehicleDto.SessionCode,
                    SecretKey = vendor.SecretKey,
                };
                tasks.Add(ExecuteWithTimeout(request));
            }

            var results = await Task.WhenAll(tasks);
            foreach (var vehicleList in results)
            {
                if (vehicleList != null)
                    resultList.AddRange(vehicleList);
            }
            return resultList;
        }

        private async Task<List<Vehicle>> ExecuteWithTimeout(GetVehiclesRequest request, int timeoutMs = 20000)
        {
            if (!await _vendorSemaphore.WaitAsync(_vendorSlotWaitTimeout))
            {
                Serilog.Log.Warning("{@VendorSlotUnavailable}", $"Eşzamanlılık slotu bulunamadı, tedarikçi atlandı: {request.VendorType}");
                return null;
            }

            try
            {
                using var cts = new CancellationTokenSource(timeoutMs);
                using var scope = _serviceScopeFactory.CreateScope();
                var vehicleService = scope.ServiceProvider.GetRequiredService<IVehicleService>();
                var data = await vehicleService.GetVehicles(request, _agencyService.GetCurrentAgencyId(), request.SessionCode).WaitAsync(cts.Token);
                return data.Data as List<Vehicle>;
            }
            catch (OperationCanceledException)
            {
                Serilog.Log.Warning("{@ExecuteWithTimeout}", $"Vendor fetch timed out after {timeoutMs}ms: {request.VendorType}");
                return null;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@ExecuteWithTimeout}", $"Vendor fetch failed: {ex.Message}");
                return null;
            }
            finally
            {
                _vendorSemaphore.Release();
            }
        }

        private async Task<VehicleListDto> MobileVehicles(int languageId, string sessionId, GetVehicleDto getVehicleDto, List<Vehicle> resultVehicles, int pickupLocationId, IMemoryCacheService memoryCacheService)
        {
            try
            {
                var podomain = _parameterService.GetParameterValue("PODOMAIN");
                var allFilters = await memoryCacheService.GetFilters();
                var filters = allFilters.Where(f => f.LanguageId == languageId).ToList();
                var allSortingOptions = await memoryCacheService.GetSortingOptions();
                var sortingOptions = allSortingOptions.Where(s => s.LanguageId == languageId).ToList();
                var allBadges = await memoryCacheService.GetBadges();
                var badges = allBadges.Where(b => b.LanguageId == languageId).ToList();
                var locationVendorContacts = await memoryCacheService.GetLocationVendorContacts(pickupLocationId);
                var deliveryTypes = await memoryCacheService.GetVendorLocationDeliveryTypes();
                var deliveryTasks = resultVehicles.Select(async v =>
                {
                    var typeId = deliveryTypes?.FirstOrDefault(x => x.VendorId == v.VendorId && x.LocationId == v.PickupLocationId)?.DeliveryTypeId ?? 0;
                    v.DeliveryType = typeId > 0 ? (DeliveryType)typeId : (DeliveryType)await GetDeliveryTypeId(v.IsOffice, v.IsAirport, languageId);
                });
                await Task.WhenAll(deliveryTasks);

                var vehicleList = new VehicleListDto();
                vehicleList.SearchId = sessionId;
                vehicleList.Filters = await CreateFilters(languageId, resultVehicles, filters, podomain);
                vehicleList.FastFilters = await CreateFastFilters(languageId, vehicleList.Filters, podomain);
                vehicleList.PopularFilters = await CreatePopularFilters(languageId, vehicleList.Filters);
                vehicleList.OrderOptions = CreateOrderOptions(allSortingOptions, podomain, languageId);
                vehicleList.VendorLocations = await CreateVendorLocations(languageId, pickupLocationId, resultVehicles, locationVendorContacts);
                var vehicles = await CreateVehicles(languageId, getVehicleDto, resultVehicles, filters, sortingOptions, badges, locationVendorContacts, pickupLocationId, memoryCacheService);
                //vehicleList.LocationInfo = new LocationInfo
                //{
                //    PickupLocation = await CreateMobileLocation(getVehicleDto.PickupLocationId, languageId),
                //    ReturnLocation = await CreateMobileLocation(getVehicleDto.ReturnLocationId, languageId)
                //};
                vehicleList.Vehicles = await _creator.SortVehicles(vehicles, pickupLocationId, languageId);
                //vehicleList.Popup = await CreateVehicleListPopup(podomain, languageId);
                vehicleList.PickupDate = DateTime.Parse(getVehicleDto.PickupDate + " " + getVehicleDto.PickupTime);
                vehicleList.ReturnDate = DateTime.Parse(getVehicleDto.ReturnDate + " " + getVehicleDto.ReturnTime);

                return vehicleList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<MobileLocation> CreateMobileLocation(int locationId, int languageId)
        {
            var location = await _memoryCacheService.GetLocationByLocationId(locationId, languageId);
            var country = await _memoryCacheService.GetCountryByLocationId(locationId, languageId);
            var city = await _memoryCacheService.GetCityByLocationId(locationId, languageId, country.Countryid);

            return new MobileLocation
            {
                City = city?.Cityname,
                Country = country?.Countryname,
                Name = location?.Locationname
            };
        }

        private async Task<List<VehicleListPopularFilter>> CreatePopularFilters(int languageId, List<VehicleListFilter> vehicleListFilter)
        {
            var popularFilterList = new List<VehicleListPopularFilter>();
            var popularFilters = await _mobileService.GetMobileVehicleListFastFilters(languageId);

            foreach (var popularFilter in popularFilters)
            {
                var filterName = Enum.GetName(typeof(MobileVehicleFutureTypes), popularFilter.Type);
                var valueName = popularFilter.Value;

                var filterIndex = vehicleListFilter.FindIndex(f => f.Type == filterName);
                var childIndex = vehicleListFilter[filterIndex].Children.FindIndex(fc => fc.Name == valueName);

                if (filterIndex >= 0 && childIndex >= 0)
                {
                    popularFilterList.Add(new VehicleListPopularFilter
                    {
                        FilterIndex = filterIndex,
                        ChildIndex = childIndex
                    });
                }
            }

            return popularFilterList;
        }

        #region Vehicle Filters
        private async Task<List<VehicleListFilter>> CreateFilters(int languageId, List<Vehicle> resultVehicles, List<MobileVehicleListFilter> filters, string podomain)
        {
            var vehicleFilters = new List<VehicleListFilter>();
            foreach (var filter in filters)
            {
                vehicleFilters.Add(new VehicleListFilter
                {
                    Order = filter.Order,
                    //Icon = podomain + filter.IconPath,
                    Header = filter.Header,
                    Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                    Children = await CreateFilterChildren(filter, resultVehicles, languageId)
                });
            }

            return vehicleFilters;
        }

        private async Task<List<VehicleListFilterDetail>> CreateFilterChildren(
            MobileVehicleListFilter filter,
            List<Vehicle> resultVehicles,
            int languageId)
        {
            var children = new List<VehicleListFilterDetail>();

            switch (filter.Type)
            {
                case MobileVehicleFutureTypes.FuelType:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => new { rv.FuelType, rv.FuelTypeName })
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.Key.FuelType.ToString(),
                                Name = g.Key.FuelTypeName
                            }));
                    break;
                case MobileVehicleFutureTypes.TransmissionType:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => new { rv.TransmissionType, rv.TransmissionTypeName })
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.Key.TransmissionType.ToString(),
                                Name = g.Key.TransmissionTypeName
                            }));
                    break;
                case MobileVehicleFutureTypes.CategoryType:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => new { rv.VehicleCategoryType, rv.VehicleCategoryTypeName })
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.Key.VehicleCategoryType.ToString(),
                                Name = g.Key.VehicleCategoryTypeName
                            }));
                    break;
                case MobileVehicleFutureTypes.VehicleType:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => new { rv.VehicleType, rv.VehicleTypeName })
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.Key.VehicleType.ToString(),
                                Name = g.Key.VehicleTypeName
                            }));
                    break;
                case MobileVehicleFutureTypes.PassengerQuantity:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => new { rv.PassangerQuantityType, rv.PassangerQuantityName })
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.Key.PassangerQuantityType.ToString(),
                                Name = g.Key.PassangerQuantityName
                            }));
                    break;
                case MobileVehicleFutureTypes.BaggageQuantity:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => new { rv.BaggageQuantityType, rv.BaggageQuantityName })
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.Key.BaggageQuantityType.ToString(),
                                Name = g.Key.BaggageQuantityName
                            }));
                    break;
                case MobileVehicleFutureTypes.Vendor:
                    children.AddRange(
                        resultVehicles
                            .OrderBy(v => v.VendorName)
                            .GroupBy(rv => rv.VendorId)
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.FirstOrDefault().VendorName,
                                Name = g.FirstOrDefault().VendorName
                            }));
                    break;
                case MobileVehicleFutureTypes.DeliveryType:
                    var deliveryTypesCache = await _memoryCacheService.GetDeliveryTypes();
                    var deliveryTypes = deliveryTypesCache.Where(d => d.LanguageId == languageId);

                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => rv.DeliveryType)
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = ((int)g.FirstOrDefault().DeliveryType).ToString(),
                                Name = deliveryTypes.FirstOrDefault(d => d.DeliveryTypeId == (int)g.FirstOrDefault().DeliveryType).Name
                            }));
                    break;
                case MobileVehicleFutureTypes.VehicleBrandName:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => rv.VehicleBrandName)
                            .OrderBy(v => v.Key)
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.FirstOrDefault().VehicleBrandName,
                                Name = g.FirstOrDefault().VehicleBrandName
                            }));
                    break;
                case MobileVehicleFutureTypes.VehicleModelName:
                    children.AddRange(
                        resultVehicles
                            .GroupBy(rv => rv.VehicleModelName)
                            .OrderBy(v => v.Key)
                            .Select(g => new VehicleListFilterDetail
                            {
                                Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = g.FirstOrDefault().VehicleModelName,
                                Name = g.FirstOrDefault().VehicleModelName
                            }));
                    break;
                case MobileVehicleFutureTypes.VehicleCampaign:
                    children.Add(new VehicleListFilterDetail
                    {
                        Type = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                        Value = "Discount",
                        Name = "İndirimli Araçlar"
                    });
                    break;
            }

            return children;
        }

        private async Task<bool> CanCouponBeUsed(int vehicleVendorId)
        {
            var couponId = _configuration.GetSectionValueInt("VendorCouponId");
            if (couponId > 0)
            {
                return await _memoryCacheService.CheckVendorInCampaign(vehicleVendorId, couponId);
            }
            return false;
        }

        #endregion

        #region Vehicle Fast Filters
        private async Task<List<VehicleListFastFilter>> CreateFastFilters(int languageId, List<VehicleListFilter> vehicleListFilter, string podomain)
        {
            var fastFilterList = new List<VehicleListFastFilter>();
            var fastFilters = await _mobileService.GetMobileVehicleListFastFilters(languageId);
            var allFilters = await _memoryCacheService.GetFilters();
            var listFilters = allFilters.Where(f => f.LanguageId == languageId);

            foreach (var fastFilter in fastFilters)
            {
                var filterName = Enum.GetName(typeof(MobileVehicleFutureTypes), fastFilter.Type);
                var valueName = fastFilter.Value;

                var filterIndex = vehicleListFilter.FindIndex(f => f.Type == filterName);
                var childIndex = vehicleListFilter[filterIndex].Children.FindIndex(fc => fc.Name == valueName);

                var iconPath = listFilters.FirstOrDefault(f => f.Type == fastFilter.Type).IconPath;

                if (filterIndex >= 0 && childIndex >= 0)
                {
                    fastFilterList.Add(new VehicleListFastFilter
                    {
                        FilterIndex = filterIndex,
                        ChildIndex = childIndex,
                        Icon = podomain + iconPath
                    });
                }
            }

            return fastFilterList;
        }
        #endregion

        #region Vehicle Order Options
        private List<VehicleListOrderOption> CreateOrderOptions(List<MobileVehicleSortingOption> allSortingOptions, string podomain, int languageId = 1)
        {
            var sortingOptions = allSortingOptions.Where(s => s.LanguageId == languageId).ToList();
            return sortingOptions
                .Select(so => new VehicleListOrderOption
                {
                    Id = languageId == 1
                         ? so.Id
                         : allSortingOptions?.FirstOrDefault(o => o.Order == so.Order && o.LanguageId == 1)?.Id ?? 0,
                    Order = so.Order,
                    IconPath = podomain + so.IconPath,
                    ValueName = so.ValueName,
                    Name = so.Name,
                    Default = so.Default,
                    SortType = so.SortType
                })
                .ToList();
        }
        #endregion

        #region Vehicle Vendor Locations
        private async Task<List<VehicleListVendorLocation>> CreateVendorLocations(int languageId, int pickupLocationId, List<Vehicle> resultVehicles, List<Vendorcontactinformation> locationVendorContacts)
        {
            var appsetting = await _mobileService.GetMobileAppSettingByParameterName("VendorLocationIconPath");
            var iconPath = appsetting?.Value;
            return resultVehicles
                .GroupBy(g => new { g.VendorId, g.VendorName })
                .Select(g => new VehicleListVendorLocation
                {
                    VendorId = g.Key.VendorId,
                    VendorName = g.Key.VendorName,
                    //Latitude = locationVendorContacts.FirstOrDefault(vc => vc.Vendorid == g.Key.VendorId)?.Latitude.ToDecimalNullSafe() > 0 ?
                    //            locationVendorContacts.FirstOrDefault(vc => vc.Vendorid == g.Key.VendorId)?.Latitude.ToDecimalNullSafe() :
                    //            null,
                    //Longitude = locationVendorContacts.FirstOrDefault(vc => vc.Vendorid == g.Key.VendorId)?.Longitude.ToDecimalNullSafe() > 0 ?
                    //            locationVendorContacts.FirstOrDefault(vc => vc.Vendorid == g.Key.VendorId)?.Longitude.ToDecimalNullSafe() :
                    //            null,
                    //Icon = iconPath
                })
                .ToList();
        }
        #endregion

        #region Vehicle List
        private async Task<List<VehicleDto>> CreateVehicles(
            int languageId,
            GetVehicleDto getVehicleDto,
            List<Vehicle> resultVehicles,
            List<MobileVehicleListFilter> filters,
            List<MobileVehicleSortingOption> sortingOptions,
            List<MobileVehicleBadge> badges,
            List<Vendorcontactinformation> locationVendorContacts,
            int pickupLocationId,
            IMemoryCacheService memoryCacheService = null)
        {
            var cache = memoryCacheService ?? _memoryCacheService;
            var podomain = _parameterService.GetParameterValue("PODOMAIN");
            var currencies = await cache.GetCurrencies();
            var currencySymbol = await GetCurrencySymbol(getVehicleDto.CurrencyCode);
            var alllabels = await cache.GetLabels(languageId);
            var labels = alllabels.Where(l => l.Dilid == languageId).ToList();
            var labelDict = labels
                .Where(l => !string.IsNullOrEmpty(l.LabelKodu))
                .GroupBy(l => l.LabelKodu)
                .ToDictionary(g => g.Key, g => g.First().Labeladi);
            var totalKmLabel = labelDict.TryGetValue("VehicleMobile.VehicleList.TotalKmLimit", out var tkl) ? tkl : "[totalkm]";
            var vehicleDetails = await cache.GetVehicleDetails();
            var vehicleFutures = await cache.GetVehicleFutures();
            var settings = await cache.GetMobileSettings();
            var vendorLocationIconPath = podomain + settings?.FirstOrDefault(s => s.Parameter == "VendorDetails")?.IconPath;

            var vehicleDtos = new List<VehicleDto>();
            var vendorOffice = await cache.GetVendorOffices(pickupLocationId);
            var vendorOfficeDict = vendorOffice?.GroupBy(x => x.VendorId).ToDictionary(g => g.Key, g => g.First());

            foreach (var rv in resultVehicles)
            {
                var vehicleDto = new VehicleDto
                {
                    PickupLocationCode = rv.PickupLocationCode,
                    ReturnLocationCode = rv.ReturnLocationCode,
                    VehicleId = rv.VehicleId,
                    BaggageQuantityName = rv.BaggageQuantityName,
                    BaggageQuantityType = (int)rv.BaggageQuantityType,
                    CurrencyCode = rv.CurrencyCode,
                    DailyPrice = rv.DailyPrice.Round(),
                    DeliveryType = (int)rv.DeliveryType,
                    DepositPrice = rv.DepositPrice ?? 0,
                    OneWayFee = rv.OneWayFee,
                    PassengerQuantityName = rv.PassangerQuantityName,
                    PassengerQuantityType = (int)rv.PassangerQuantityType,
                    RentalDuration = rv.RentalDuration,
                    ReservationToken = rv.ReservationToken,
                    TotalPrice = rv.TotalPrice.Round(),
                    VehicleCategoryTypeName = rv.VehicleCategoryTypeName,
                    VehicleCategoryType = (int)rv.VehicleCategoryType,
                    ServiceCharge = rv.ServiceCharge,
                    TotalKmLimit = rv.TotalKMLimit ?? 0,
                    VehicleFuelTypeName = rv.FuelTypeName,
                    VehicleFuelType = (int)rv.FuelType,
                    VehicleName = rv.VehicleName,
                    VehicleTransmissionTypeName = rv.TransmissionTypeName,
                    VehicleTransmissionType = (int)rv.TransmissionType,
                    VehicleTypeName = rv.VehicleTypeName,
                    VehicleType = (int)rv.VehicleType,
                    VendorId = rv.VendorId,
                    VendorMinimumDriverAge = rv.VendorMinimumDriverAge,
                    VendorMinimumDrivingLicenseAge = rv.VendorMinimumDrivingLicenseAge,
                    IsFlightNumberRequired = vendorOfficeDict != null && vendorOfficeDict.TryGetValue(rv.VendorId, out var vo) ? vo.FlightCardRequired ?? false : false,

                    SortableParameters = CreateSortableParameters(rv, sortingOptions),
                    VehicleBadges = CreateVehicleBadges(rv, badges, podomain),
                    VehicleFeatures = await CreateVehicleFeatures(rv, vehicleFutures, labels, labelDict, podomain, languageId, totalKmLabel, MobilePages.VehicleList),
                    //VehiclePromotion = CreateVehiclePromotions(rv, vendorCoupons, labels, getVehicleDto.PickupLocationId, currencySymbol),
                };

                vehicleDto.MobileFilterParameters = await CreateFilterParameters(rv, filters, vehicleDto.VehiclePromotion);

                vehicleDto.VehicleDetails = await CreateVehicleDetailsAsync(rv, vehicleDetails, labels, labelDict, podomain, languageId, currencySymbol);
                vehicleDtos.Add(vehicleDto);
            }

            return vehicleDtos;
        }

        private async Task<string> GetCurrencySymbol(string currencyCode)
        {
            var currencies = await _memoryCacheService.GetCurrencies();
            var currency = currencies.FirstOrDefault(c => c.Currencyisocode == currencyCode);
            var currencySymbol = currency?.Symbol ?? "";

            return currencySymbol;
        }

        private async Task<List<MobileFilterParameters>> CreateFilterParameters(Vehicle rv, List<MobileVehicleListFilter> filters, VehiclePromotion vehiclePromotion)
        {
            var filterParameters = new List<MobileFilterParameters>();

            foreach (var filter in filters)
            {
                switch (filter.Type)
                {
                    case MobileVehicleFutureTypes.FuelType:
                        filterParameters.Add(new MobileFilterParameters
                        {
                            DataType = MobileDataTypes.String,
                            Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                            Value = rv.FuelType.ToString()
                        });
                        break;
                    case MobileVehicleFutureTypes.TransmissionType:
                        filterParameters.Add(new MobileFilterParameters
                        {
                            DataType = MobileDataTypes.String,
                            Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                            Value = rv.TransmissionType.ToString()
                        });
                        break;
                    case MobileVehicleFutureTypes.CategoryType:
                        filterParameters.Add(new MobileFilterParameters
                        {
                            DataType = MobileDataTypes.String,
                            Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                            Value = rv.VehicleCategoryType.ToString()
                        });
                        break;
                    case MobileVehicleFutureTypes.Vendor:
                        filterParameters.Add(new MobileFilterParameters
                        {
                            DataType = MobileDataTypes.String,
                            Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                            Value = rv.VendorName
                        });
                        break;
                    case MobileVehicleFutureTypes.DeliveryType:
                        filterParameters.Add(new MobileFilterParameters
                        {
                            DataType = MobileDataTypes.String,
                            Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                            Value = ((int)rv.DeliveryType).ToString()
                        });
                        break;
                    case MobileVehicleFutureTypes.VehicleBrandName:
                        filterParameters.Add(new MobileFilterParameters
                        {
                            DataType = MobileDataTypes.String,
                            Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                            Value = rv.VehicleBrandName
                        });
                        break;
                    case MobileVehicleFutureTypes.VehicleModelName:
                        filterParameters.Add(new MobileFilterParameters
                        {
                            DataType = MobileDataTypes.String,
                            Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                            Value = rv.VehicleModelName
                        });
                        break;
                    case MobileVehicleFutureTypes.VehicleCampaign:
                        if (await CanCouponBeUsed(rv.VendorId))
                        {
                            filterParameters.Add(new MobileFilterParameters
                            {
                                DataType = MobileDataTypes.String,
                                Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = "Discount"
                            });
                        }
                        else if (vehiclePromotion != null && vehiclePromotion.Code != string.Empty)
                        {
                            filterParameters.Add(new MobileFilterParameters
                            {
                                DataType = MobileDataTypes.String,
                                Name = Enum.GetName(typeof(MobileVehicleFutureTypes), filter.Type),
                                Value = "Discount"
                            });
                        }
                        break;
                }
            }

            return filterParameters;
        }

        private List<SortableParameter> CreateSortableParameters(Vehicle rv, List<MobileVehicleSortingOption> sortingOptions)
        {
            var sortingParams = new List<SortableParameter>();

            foreach (var sortingOption in sortingOptions.GroupBy(so => so.ValueName))
            {
                switch (sortingOption.Key)
                {
                    case "DailyPrice":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.Number,
                            Name = sortingOption.Key,
                            Value = rv.DailyPrice.Round().ToStringNullSafe()
                        });
                        break;
                    case "TotalPrice":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.Number,
                            Name = sortingOption.Key,
                            Value = rv.TotalPrice.Round().ToStringNullSafe()
                        });
                        break;
                    case "DiscountedTotalPrice":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.Number,
                            Name = sortingOption.Key,
                            Value = rv.TotalPrice.Round().ToStringNullSafe()
                        });
                        break;
                    case "DepositPrice":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.Number,
                            Name = sortingOption.Key,
                            Value = rv.DepositPrice.ToStringNullSafe()
                        });
                        break;
                    case "TotalKMLimit":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.Number,
                            Name = sortingOption.Key,
                            Value = rv.TotalKMLimit.ToStringNullSafe()
                        });
                        break;
                    case "VendorName":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.String,
                            Name = sortingOption.Key,
                            Value = rv.VendorName.ToStringNullSafe()
                        });
                        break;
                    case "VendorScore":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.Number,
                            Name = sortingOption.Key,
                            Value = rv.VendorScore.ToStringNullSafe()
                        });
                        break;
                    case "VendorCommentCount":
                        sortingParams.Add(new SortableParameter
                        {
                            DataType = MobileDataTypes.Number,
                            Name = sortingOption.Key,
                            Value = rv.VendorCommentCount.ToStringNullSafe()
                        });
                        break;
                }
            }

            return sortingParams;
        }

        private List<VehicleBadge> CreateVehicleBadges(Vehicle rv, List<MobileVehicleBadge> badges, string podomain)
        {
            var matchedConditions = from rc in rv.RentalConditions
                                    join mb in badges
                                    on rc.ConditionId equals mb.ConditionId
                                    select mb;

            return matchedConditions
                .Select(vb => new VehicleBadge
                {
                    //BackgroundColor = vb.BackgroundColor,
                    //BorderColor = vb.BorderColor,
                    //IconPath = podomain + vb.IconPath,
                    //Order = vb.Order,
                    //TextColor = vb.TextColor,
                    ConditionId = vb.ConditionId,
                    //Text = vb.Text
                })
                .ToList();
        }

        private async Task<List<VehicleDetail>> CreateVehicleDetailsAsync(Vehicle rv, List<MobileVehicleDetail> details, List<Label> labels, Dictionary<string, string> labelDict, string podomain, int languageId, string currencySymbol)
        {
            var vehicleDetails = new List<VehicleDetail>();

            foreach (var detail in details)
            {
                switch (detail.Type)
                {
                    case MobileVehicleDetailTypes.DailyPrice:
                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = rv.DailyPrice.Round().ToString(),
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.DailyPrice", out var dpLabel) ? dpLabel : "[price][currencySymbol]")
                                .Replace("[price]", rv.DailyPrice.Round().ToString())
                                .Replace("[currencySymbol]", currencySymbol),
                            Title = labelDict.TryGetValue("VehicleMobile.VehicleList.DailyPriceTitle", out var dpTitle) ? dpTitle : null,
                            Description = (labelDict.TryGetValue("VehicleMobile.VehicleList.DailyPriceDescription", out var dpDesc) ? dpDesc : "[price][currencySymbol]")
                                .Replace("[price]", rv.DailyPrice.Round().ToString())
                                .Replace("[currencySymbol]", currencySymbol)
                        });
                        break;
                    case MobileVehicleDetailTypes.TotalPrice:
                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = rv.TotalPrice.Round().ToString(),
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.TotalPrice", out var tpLabel) ? tpLabel : "[price][currencySymbol]")
                                .Replace("[price]", rv.TotalPrice.Round().ToString())
                                .Replace("[currencySymbol]", currencySymbol),
                            Title = labelDict.TryGetValue("VehicleMobile.VehicleList.TotalPriceTitle", out var tpTitle) ? tpTitle : null,
                            Description = (labelDict.TryGetValue("VehicleMobile.VehicleList.TotalPriceDescription", out var tpDesc) ? tpDesc : "[price][currencySymbol]")
                                .Replace("[price]", rv.TotalPrice.Round().ToString())
                                .Replace("[currencySymbol]", currencySymbol)
                        });
                        break;
                    case MobileVehicleDetailTypes.Deposit:
                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = rv.DepositPrice?.ToString(CultureInfo.InvariantCulture) ?? "0.0",
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleDepositPrice", out var depLabel) ? depLabel : "[price][currencySymbol]")
                                .Replace("[price]", rv.DepositPrice.ToString())
                                .Replace("[currencySymbol]", currencySymbol),
                            Title = labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleDepositPriceTitle", out var depTitle) ? depTitle : null,
                            Description = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleDepositPriceDescription", out var depDesc) ? depDesc : "[price][currencySymbol]")
                                .Replace("[price]", rv.DepositPrice.ToString())
                                .Replace("[currencySymbol]", currencySymbol)
                        });
                        break;
                    case MobileVehicleDetailTypes.TotalKmLimit:
                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = rv.TotalKMLimit.ToString(),
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.TotalKmLimit", out var kmLabel) ? kmLabel : "[totalkm]")
                                .Replace("[totalkm]", rv.TotalKMLimit.ToString()),
                            Title = labelDict.TryGetValue("VehicleMobile.VehicleList.TotalKmLimitTitle", out var kmTitle) ? kmTitle : null,
                            Description = (labelDict.TryGetValue("VehicleMobile.VehicleList.TotalKmLimitDescription", out var kmDesc) ? kmDesc : "[totalkm]")
                                .Replace("[totalkm]", rv.TotalKMLimit.ToString())
                        });
                        break;
                    case MobileVehicleDetailTypes.MinimumDriverAge:
                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = rv.VendorMinimumDriverAge.ToString(),
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleMinimumDriverAge", out var ageLabel) ? ageLabel : "[age]")
                                .Replace("[age]", rv.VendorMinimumDriverAge.ToString()),
                            Title = labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleMinimumDriverAgeTitle", out var ageTitle) ? ageTitle : null,
                            Description = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleMinimumDriverAgeDescription", out var ageDesc) ? ageDesc : "[age]")
                                .Replace("[age]", rv.VendorMinimumDriverAge.ToString())
                        });
                        break;
                    case MobileVehicleDetailTypes.MinimumDriverLicenseAge:
                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = rv.VendorMinimumDrivingLicenseAge.ToString(),
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleMinimumLicenseAge", out var licLabel) ? licLabel : "[age]")
                                .Replace("[age]", rv.VendorMinimumDrivingLicenseAge.ToString()),
                            Title = labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleMinimumLicenseAgeTitle", out var licTitle) ? licTitle : null,
                            Description = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleMinimumLicenseAgeDescription", out var licDesc) ? licDesc : "[age]")
                                .Replace("[age]", rv.VendorMinimumDrivingLicenseAge.ToString())
                        });
                        break;
                    case MobileVehicleDetailTypes.DeliveryType:
                        var deliveryType = await GetDeliveryType(rv, languageId);

                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = deliveryType?.Name ?? string.Empty,
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleDeliveryType", out var delLabel) ? delLabel : "[DeliveryType]")
                                .Replace("[DeliveryType]", deliveryType?.Name ?? string.Empty),
                            Title = labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleDeliveryTypeTitle", out var delTitle) ? delTitle : null,
                            Description = labelDict.TryGetValue($"VehicleMobile.VehicleList.VehicleDeliveryTypeDescription.{rv.DeliveryType.ToString()}", out var delDesc) ? delDesc : null
                        });
                        break;
                    default:
                        vehicleDetails.Add(new VehicleDetail
                        {
                            Order = detail.Order,
                            Type = detail.Type,
                            Icon = podomain + detail.IconPath,
                            Value = rv.DailyPrice.Round().ToString(),
                            Text = detail.Text
                        });
                        break;
                }
            }

            return vehicleDetails?.OrderBy(v => v.Order)?.ToList();
        }

        private async Task<DeliveryTypeLanguage> GetDeliveryType(Vehicle rv, int languageId)
        {
            var deliveryTypesCache = await _memoryCacheService.GetDeliveryTypes();
            var deliveryTypes = deliveryTypesCache.Where(d => d.LanguageId == languageId);
            var deliveryTypeId = (int)rv.DeliveryType > 0
                                    ? (int)rv.DeliveryType
                                    : await GetDeliveryTypeId(rv.IsOffice, rv.IsAirport, languageId);

            var deliveryType = deliveryTypes.FirstOrDefault(d => d.DeliveryTypeId == deliveryTypeId);

            return deliveryType;
        }

        private async Task<List<VehicleFeature>> CreateVehicleFeatures(Vehicle rv, List<MobileVehicleFeature> features, List<Label> labels, Dictionary<string, string> labelDict, string podomain, int languageId, string totalKmLabel, MobilePages page)
        {
            var vehicleFeatures = new List<VehicleFeature>();

            var deliveryType = await GetDeliveryType(rv, languageId);
            string deliveryTypeLabel = GetDeliveryTypeLabel(labels, deliveryType.Id);
            string passengerQuantityLabelName = page == MobilePages.Details
                                                        ? "VehicleMobile.VehicleList.PassangerQuantityTypeGetDetail"
                                                        : "VehicleMobile.VehicleList.PassangerQuantityType";

            foreach (var feature in features)
            {
                switch (feature.Type)
                {
                    case MobileVehicleFutureTypes.FuelType:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = rv.FuelType.ToString(),
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.FuelType", out var fuelLabel) ? fuelLabel : "[name]")
                                .Replace("[name]", rv.FuelTypeName)
                        });
                        break;
                    case MobileVehicleFutureTypes.TransmissionType:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = rv.TransmissionType.ToString(),
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.TransmissionType", out var transLabel) ? transLabel : "[name]")
                                .Replace("[name]", rv.TransmissionTypeName)
                        });
                        break;
                    case MobileVehicleFutureTypes.CategoryType:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = rv.VehicleCategoryType.ToString(),
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.CategoryType", out var catLabel) ? catLabel : "[name]")
                                .Replace("[name]", rv.VehicleCategoryTypeName)
                        });
                        break;
                    case MobileVehicleFutureTypes.VehicleType:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = rv.VehicleType.ToString(),
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.VehicleType", out var vtLabel) ? vtLabel : "[name]")
                                .Replace("[name]", rv.VehicleTypeName)
                        });
                        break;
                    case MobileVehicleFutureTypes.PassengerQuantity:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = rv.PassangerQuantityType.ToString(),
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = (labelDict.TryGetValue(passengerQuantityLabelName, out var pqLabel) ? pqLabel : "[name]")
                                .Replace("[name]", rv.PassangerQuantityName)
                        });
                        break;
                    case MobileVehicleFutureTypes.BaggageQuantity:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = ((int)rv.BaggageQuantityType + 1).ToString(),
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.BaggageQuantityType", out var bqLabel) ? bqLabel : "[name]")
                                .Replace("[name]", rv.BaggageQuantityName)
                        });
                        break;
                    case MobileVehicleFutureTypes.KmLimit:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = rv.TotalKMLimit.ToStringNullSafe(),
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = totalKmLabel.Replace("[totalkm]", rv.TotalKMLimit.ToStringNullSafe()),
                            //Info = new Info
                            //{
                            //    IconPath = podomain + InfoIconPath,
                            //    Text = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Vehiclelist.TotalKmInfo")?.Labeladi ?? ""
                            //}
                        });
                        break;
                    case MobileVehicleFutureTypes.DeliveryType:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = deliveryType?.Name,
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = (labelDict.TryGetValue("VehicleMobile.VehicleList.DeliveryType", out var dtLabel) ? dtLabel : "[DeliveryType]")
                                .Replace("[DeliveryType]", deliveryType?.Name),
                            //Info = new Info
                            //{
                            //    IconPath = podomain + InfoIconPath,
                            //    Text = deliveryTypeLabel
                            //}
                        });
                        break;
                    default:
                        vehicleFeatures.Add(new VehicleFeature
                        {
                            Type = feature.Type,
                            Value = feature.Value,
                            Order = feature.Order,
                            //Icon = podomain + feature.IconPath,
                            Text = feature.Value
                        });
                        break;
                }
            }

            return vehicleFeatures?.OrderBy(v => v.Order)?.ToList();
        }

        private async Task<int> GetDeliveryTypeId(bool IsOffice, bool IsAirport, int languageId)
        {
            var deliveryTypeId = IsOffice switch
            {
                true when IsAirport => 1,
                true when !IsAirport => 2,
                false when IsAirport => 3,
                _ => 4
            };

            return deliveryTypeId;
        }

        private async Task<int> GetDeliveryTypeIdFromVendorLocation(int vendorId, int locationId)
        {
            var deliveryTypes = await _memoryCacheService.GetVendorLocationDeliveryTypes();
            var deliveryTypeId = deliveryTypes?.FirstOrDefault(x => x.VendorId == vendorId && x.LocationId == locationId)?.DeliveryTypeId ?? 0;

            return deliveryTypeId;
        }

        private string GetDeliveryTypeLabel(List<Label> labels, int deliveryTypeId)
        {
            string label = "";
            switch (deliveryTypeId)
            {
                case 1:
                    label = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Vehiclelist.DeliveryTypeAirportInfo")?.Labeladi ?? "";
                    break;
                case 2:
                    label = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Vehiclelist.DeliveryTypeOfficeInfo")?.Labeladi ?? "";
                    break;
                case 3:
                    label = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Vehiclelist.DeliveryTypeValeInfo")?.Labeladi ?? "";
                    break;
                case 4:
                    label = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Vehiclelist.DeliveryTypeWelcomeInfo")?.Labeladi ?? "";
                    break;
                case 5:
                    label = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Vehiclelist.DeliveryTypeTransferInfo")?.Labeladi ?? "";
                    break;
                default:
                    break;
            }

            return label;
        }

        private VendorOfficeLocation CreateVehicleOfficeLocations(Vehicle rv, List<Vendorcontactinformation> locationVendorContacts, string iconPath)
        {
            var vendorContactInformation = locationVendorContacts.FirstOrDefault(lv =>
                lv.Vendorid == rv.VendorId && lv.Locationid == rv.PickupLocationId);
            if (vendorContactInformation != null && vendorContactInformation.Id > 0 && !string.IsNullOrEmpty(vendorContactInformation?.ShortAddress))
            {
                return new VendorOfficeLocation
                {
                    Latitude = vendorContactInformation.Latitude?.ToDecimalNullSafe() > 0 ? vendorContactInformation.Latitude?.ToDecimalNullSafe() : null,
                    Longitude = vendorContactInformation.Longitude?.ToDecimalNullSafe() > 0 ? vendorContactInformation.Longitude?.ToDecimalNullSafe() : null,
                    ShortAddress = vendorContactInformation.ShortAddress,
                    GoogleLink = vendorContactInformation.GoogleLink,
                    Icon = iconPath
                };
            }
            return null;
        }
        #endregion

        #endregion

        #region Get Vendor Comment Action
        [HttpPost]
        [Route("GetComments")]
        public async Task<IActionResult> GetComments(GetVendorLocationCommentDto getVendorLocationCommentDto)
        {
            var language = await _languageService.Get(getVendorLocationCommentDto.LanguageCode.ToLower());
            var languageId = language != null ? language.Dilid : 1;

            var labels = await _memoryCacheService.GetLabels(languageId);
            var allLabels = labels.Where(l => l.Dilid == languageId);
            var commentLabel = allLabels.FirstOrDefault(l => l.LabelKodu == "VehicleMobile.VehicleList.CommentsHeader")?.Labeladi;
            string[] yildizLabels ={
                allLabels.FirstOrDefault(l => l.LabelKodu == "AnketBirYildiz")?.Labeladi,
                allLabels.FirstOrDefault(l => l.LabelKodu == "AnketIkiYildiz")?.Labeladi,
                allLabels.FirstOrDefault(l => l.LabelKodu == "AnketUcYildiz")?.Labeladi,
                allLabels.FirstOrDefault(l => l.LabelKodu == "AnketDortYildiz")?.Labeladi,
                allLabels.FirstOrDefault(l => l.LabelKodu == "AnketBesYildiz")?.Labeladi
            };

            var allComments = await _surveyService.GetCommentsByLanguageIdLocationId(languageId, getVendorLocationCommentDto.LocationId);
            var comments = allComments.Where(c => c.VendorId == getVendorLocationCommentDto.VendorId);
            var externalComments = await _surveyService.GetExternalCommentsByLanguageIdLocationId(languageId, getVendorLocationCommentDto.LocationId, getVendorLocationCommentDto.VendorId);
            var vendor = await _vendorService.GetVendorByIdAll(getVendorLocationCommentDto.VendorId);

            if (!comments.Any() && !externalComments.Any())
                return Ok(new
                {
                    data = new MobileReviewsModel
                    {
                        CommentLabel = commentLabel,
                        VendorName = vendor.Vendorname,
                        VendorLogo = vendor.Logo
                    },
                    success = true,
                    resultCode = ResultCodes.Success
                });

            #region vendorSpecialScore
            List<SpecialScore> vendorSpecialScores = await CreateVendorSpecialScores(comments, yildizLabels);
            #endregion

            #region comments
            List<Comment> commentRedesigned = await CreateCommentRedesigned(comments, yildizLabels);
            List<Comment> allCommentRedesigned = await CreateExternalCommentRedesigned(commentRedesigned, externalComments, yildizLabels);
            #endregion

            var vendorScore = comments.Count() != 0 || externalComments.Count() != 0
                ? (comments.Sum(c => c.Score) + externalComments.Sum(e => e.Score)) / (comments.Count() + externalComments.Count())
                : 0;

            int interval = (int)Math.Ceiling((decimal)(vendorScore)) - 1;

            MobileReviewsModel review = new MobileReviewsModel
            {
                VendorCommentCount = allCommentRedesigned.Count(),
                VendorScore = Math.Round((double)vendorScore, 1),
                VendorScoreLabel = interval >= 0 && interval < yildizLabels.Length ? yildizLabels[interval] : "",
                VendorSpecialScores = vendorSpecialScores,
                VendorName = vendor.Vendorname,
                VendorLogo = vendor.Logo,
                Comments = allCommentRedesigned,
                CommentLabel = commentLabel,
            };
            return Ok(new
            {
                data = review,
                success = true,
                resultCode = ResultCodes.Success
            });

        }

        private async Task<List<Comment>> CreateExternalCommentRedesigned(List<Comment> commentRedesigned, IEnumerable<ExternalComment> externalComments, string[] yildizLabels)
        {
            foreach (var comment in externalComments)
            {
                int interval = (int)Math.Ceiling((double)comment.Score) - 1;

                commentRedesigned.Add(new Comment
                {
                    Name = comment.CustomerName,
                    Surname = comment.CustomerSurname,
                    CommentText = comment.Comment,
                    CommentSource = comment.Source,
                    StatusDate = comment.CommentDate?.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                    Score = Math.Round((double)comment.Score, 1),
                    ScoreLabel = interval >= 0 && interval < yildizLabels.Length ? yildizLabels[interval] : ""
                });
            }
            return commentRedesigned;
        }

        private async Task<List<Comment>> CreateCommentRedesigned(IEnumerable<CommentDto> comments, string[] yildizLabels)
        {
            List<Comment> commentRedesigned = new List<Comment>();
            foreach (var item in comments.GroupBy(x => x.Id))
            {
                var comment = item.FirstOrDefault();
                int interval = (int)Math.Ceiling(item.Average(c => c.Score)) - 1;

                commentRedesigned.Add(new Comment
                {
                    Name = comment.Name != null ? comment.Name.ToSecureText() : null,
                    Surname = comment.Surname != null ? comment.Surname.ToSecureText() : null,
                    CommentText = comment.Comment,
                    CommentSource = comment.CommentSource,
                    StatusDate = comment.StatusDate.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                    Score = Math.Round(item.Average(c => c.Score), 1),
                    ScoreLabel = interval >= 0 && interval < yildizLabels.Length ? yildizLabels[interval] : ""
                });
            }
            return commentRedesigned;
        }

        private async Task<List<SpecialScore>> CreateVendorSpecialScores(IEnumerable<CommentDto> comments, string[] yildizLabels)
        {
            var averageScoresByQuestion = comments
                                          .GroupBy(c => c.QuestionTitle)
                                          .Select(group => new
                                          {
                                              QuestionTitle = group.Key,
                                              AverageScore = group.Any() ? group.Average(c => c.Score) : 0
                                          });
            List<SpecialScore> vendorSpecialScores = new List<SpecialScore>();

            foreach (var specialScore in averageScoresByQuestion)
            {
                int interval = (int)Math.Ceiling(specialScore.AverageScore) - 1;

                vendorSpecialScores.Add(new SpecialScore
                {
                    Name = specialScore.QuestionTitle,
                    ScoreLabel = interval >= 0 && interval < yildizLabels.Length ? yildizLabels[interval] : "",
                    Score = Math.Round(specialScore.AverageScore, 1)
                });
            }
            return vendorSpecialScores;
        }

        #endregion

        #region Get Rezervations Action

        [HttpPost]
        [Route("GetReservations")]
        public async Task<IActionResult> GetReservations(GetReservationsDto getReservationsDto)
        {
            var language = await _languageService.Get(getReservationsDto.LanguageCode);

            getReservationsDto.LanguageId = language != null ? language.Dilid : 1;

            var rezList = await _reservationService.GetReservationsByEmail(getReservationsDto.Email);
            if (!rezList?.Any() ?? true)
            {
                return Ok(new
                {
                    success = false,
                    resultCode = ResultCodes.Success,
                    message = "Rezervasyon bulunamadı!"
                });
            }
            var vehicleCategoryTypes = await _vehicleService.GetVehicleCategoryLangList(getReservationsDto.LanguageId);

            List<ReservationDto> reservations = new List<ReservationDto>();
            foreach (var rez in rezList)
            {
                var mappedReservation = ReservationMapper.Map(rez);
                ReservationDto reservationDto = new ReservationDto();

                reservationDto.ReservationDetails = await CreateRezDetails(mappedReservation, vehicleCategoryTypes);
                reservationDto.CustomerDetails = await CreateCustomerDetails(mappedReservation);
                reservationDto.PriceDetails = await CreatePriceDetails(mappedReservation);
                reservationDto.VendorDetails = await CreateVendorDetails(mappedReservation);
                reservationDto.RentalConditions = await CreateRentalConditions(mappedReservation.VendorId, getReservationsDto.LanguageId);
                reservations.Add(reservationDto);
            }

            return Ok(new
            {
                data = reservations,
                success = true,
                resultCode = ResultCodes.Success
            });
        }

        [HttpPost]
        [Route("GetReservationUpdates")]
        public async Task<IActionResult> GetReservationUpdates(GetReservationUpdateRequest request)
        {
            if (!string.IsNullOrEmpty(request.ReservationNumber))
            {
                var updates = await _reservationService.GetReservationUpdatesByReservationNumber(request.ReservationNumber);

                return Ok(new
                {
                    data = updates ?? new List<Reservationupdate>(),
                    success = true,
                    resultCode = ResultCodes.Success
                });
            }

            return Ok(new
            {
                data = string.Empty,
                success = false,
                resultCode = ResultCodes.Error,
                message = "Reservation number is required. Please provide a valid reservation number."
            });
        }

        [HttpPost]
        [Route("GetCanceledReservations")]
        public async Task<IActionResult> GetCanceledReservations(GetCanceledReservationsRequest request)
        {
            if (request.EndDate < DateTime.MaxValue && request.StartDate > DateTime.MinValue)
            {
                var rezList = await _reservationService.GetCanceledReservations(request.StartDate, request.EndDate);

                var config = new MapperConfiguration(
                    cfg => cfg.CreateMap<Rez, CanceledReservation>(),
                    Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
                var mapper = new Mapper(config);
                var mappedReservations = mapper.Map<List<CanceledReservation>>(rezList);

                return Ok(new
                {
                    data = mappedReservations,
                    success = true,
                    resultCode = ResultCodes.Success,
                    message = "Success"
                });
            }

            return Ok(new
            {
                data = string.Empty,
                success = false,
                resultCode = ResultCodes.Error,
                message = "Invalid parameters"
            });
        }

        [HttpPost]
        [Route("CheckReservationIsSuccess")]
        public async Task<IActionResult> CheckReservationIsSuccess(CheckReservationIsSeuccessRequest request)
        {
            var rez = _reservationService.GetCanceledReservationsByRezToken(request.ReservationToken);

            if (!string.IsNullOrEmpty(rez?.Rezno ?? string.Empty))
            {
                return Ok(new
                {
                    data = new
                    {
                        ReservationNumber = rez.Rezno,
                        ReservationToken = rez.Reservationtokentext,
                        ReferanceNumber = rez.Apireservationnumber,
                        IsBooked = rez.Resstatusid == -1
                    },
                    success = true,
                    resultCode = ResultCodes.Success,
                    message = "Success"
                });
            }

            return Ok(new
            {
                data = new
                {
                    ReservationNumber = string.Empty,
                    ReservationToken = string.Empty,
                    IsBooked = false
                },
                success = true,
                resultCode = ResultCodes.Success,
                message = "Success"
            });
        }

        [HttpPost]
        [Route("GetReservation")]
        public async Task<IActionResult> GetReservation(GetReservationRequest request)
        {
            if (!string.IsNullOrEmpty(request.ReservationNumber))
            {
                var rez = await _reservationService.GetReservationByRezNo(request.ReservationNumber);

                if (rez == null)
                    return Ok(new
                    {
                        data = string.Empty,
                        success = true,
                        resultCode = ResultCodes.Success
                    });

                var mappedReservation = ReservationMapper.Map(rez);

                return Ok(new
                {
                    data = mappedReservation,
                    success = true,
                    resultCode = ResultCodes.Success
                });
            }

            return Ok(new
            {
                data = string.Empty,
                success = false,
                resultCode = ResultCodes.Error,
                message = "Reservation number is required. Please provide a valid reservation number."
            });
        }

        [HttpPost]
        [Route("GetUpdatedReservations")]
        public async Task<IActionResult> GetUpdatedReservations(GetReservationRequest request)
        {
            if (!request.StartDate.HasValue || !request.EndDate.HasValue)
            {
                return BadRequest(new
                {
                    data = new List<Reservation>(),
                    success = false,
                    resultCode = ResultCodes.Error,
                    message = "Start date and end date are required."
                });
            }

            var rezList = await _reservationService
                .GetUpdatedReservations(request.StartDate.Value, request.EndDate.Value);

            var mappedReservations = rezList
                                    .Select(ReservationMapper.Map)
                                    .ToList();

            return Ok(new
            {
                data = mappedReservations,
                success = true,
                resultCode = ResultCodes.Success
            });
        }

        private async Task<List<RentalCondition>> CreateRentalConditions(int vendorId, int languageId)
        {
            var result = await _vehicleService.GetRentalConditions(vendorId, (LanguageTypes)(languageId - 1));
            return result.Data as List<RentalCondition>;
        }

        private async Task<VendorDetails> CreateVendorDetails(Reservation mappedReservation)
        {
            return new VendorDetails
            {
                VendorId = mappedReservation.VendorId,
                VendorPhone = mappedReservation.VendorPhone,
                VendorEmail = mappedReservation.VendorEmail,
                VendorName = mappedReservation.VendorName,
                VendorLogo = mappedReservation.VendorLogo
            };
        }

        private async Task<PriceDetails> CreatePriceDetails(Reservation mappedReservation)
        {
            return new PriceDetails
            {
                DailyPrice = mappedReservation.DailyPrice.Round(),
                ExtraPrice = mappedReservation.ExtraPrice,
                OneWayFee = mappedReservation.OneWayFee,
                TotalPrice = mappedReservation.TotalPrice.Round(),
                PaidAmount = mappedReservation.PaidAmount,
            };
        }

        private async Task<CustomerDetails> CreateCustomerDetails(Reservation mappedReservation)
        {
            return new CustomerDetails
            {
                CustomerName = mappedReservation.CustomerName,
                CustomerSurname = mappedReservation.CustomerSurname,
                CustomerPhone = mappedReservation.CustomerPhone,
                CustomerMail = mappedReservation.CustomerMail,
                CustomerIdentityNumber = mappedReservation.CustomerIdentityNumber,
                CustomerAddress = mappedReservation.CustomerAddress,
                CustomerArrivalFlightNumber = mappedReservation.CustomerArrivalFlightNumber,
                CustomerReturnFlightNumber = mappedReservation.CustomerReturnFlightNumber,
                CustomerNote = mappedReservation.CustomerNote
            };
        }

        private async Task<ReservationDetails> CreateRezDetails(Reservation mappedReservation, List<Vehiclecategorylang> vehicleCategoryTypes)
        {
            return new ReservationDetails
            {
                ReservationNumber = mappedReservation.ReservationNumber,
                ReservationId = mappedReservation.ReservationId,
                ReservationDate = mappedReservation.ReservationDate,
                PickupDate = mappedReservation.PickupDate,
                ReturnDate = mappedReservation.ReturnDate,
                VehicleName = mappedReservation.VehicleName,
                VehicleImageUrl = mappedReservation.VehicleImageUrl,
                VehicleBrandName = mappedReservation.VehicleBrandName,
                VehicleModelName = mappedReservation.VehicleModelName,
                PickupLocationName = mappedReservation.PickupLocationName,
                ReturnLocationName = mappedReservation.ReturnLocationName,
                RentalDuration = mappedReservation.RentalDuration,
                TotalKMLimit = mappedReservation.TotalKMLimit,
                ReservationStatusType = mappedReservation.ReservationStatusType,
                VehicleCategoryType = mappedReservation.VehicleCategoryType,
                VehicleCategoryTypeName = vehicleCategoryTypes.FirstOrDefault(c => c.Categoryid == (int)mappedReservation.VehicleCategoryType + 1).Categoryname
            };
        }

        #endregion

        #region Get Details Action

        [HttpPost]
        [Route("GetDetails")]
        public async Task<IActionResult> GetDetails(GetDetailsDto getDetailsDto)
        {
            var oldToken = getDetailsDto.ReservationToken;

            if (getDetailsDto.IsTokenUsed)
            {
                var newToken = await GetNewVehicleToken(getDetailsDto.ReservationToken);

                if (!string.IsNullOrEmpty(newToken))
                    getDetailsDto.ReservationToken = newToken;
            }

            var tokenExists = await _resTokenService.GetReservationTokenByUniqueId(getDetailsDto.ReservationToken);
            if (tokenExists == null && getDetailsDto.SearchContext != null)
            {
                var resolvedToken = await ResolveTokenFromSearchContext(getDetailsDto.SearchContext);
                if (!string.IsNullOrEmpty(resolvedToken))
                    getDetailsDto.ReservationToken = resolvedToken;
            }

            if (string.IsNullOrWhiteSpace(getDetailsDto.ReservationToken))
                return BadRequest(new
                {
                    httpResultType = (int)HttpStatusCode.BadRequest,
                    success = false,
                    message = "Reservation Token is Empty or Token Already Used!"
                });

            try
            {
                var serviceResponse = await _extraService.GetExtras(new GetExtrasRequest
                {
                    ReservationToken = getDetailsDto.ReservationToken,
                    LanguageCode = getDetailsDto.LanguageCode.ToStringNullSafe().ToUpper()
                });

                if (serviceResponse.Success)
                {
                    var sessionId =
                        await _reservationTokenService.GetReservationTokenSessionId(getDetailsDto.ReservationToken);
                    var languageId = await _memoryCacheService.GetLanguageId(getDetailsDto.LanguageCode);
                    var allLabels = await _memoryCacheService.GetLabels(languageId);
                    var labels = allLabels.Where(l => l.Dilid == languageId).ToList();
                    var vehicleFeatures = await _memoryCacheService.GetVehicleFutures();
                    var vehicle = (serviceResponse.Data as GetExtrasResponse).Vehicle;
                    var alternativeVehicles = (serviceResponse.Data as GetExtrasResponse).AlternativeVehicles;
                    var vendor = await _vendorService.GetVendorById(vehicle.VendorId);
                    var podomain = _parameterService.GetParameterValue("PODOMAIN");
                    var mobileAppSettings = await _memoryCacheService.GetMobileSettings();
                    var requiredSettings = mobileAppSettings.Where(s => s.Parameter == "RequiredDocumentsOnDelivery" && s.LanguageId == languageId).ToList();
                    var specialSettings = mobileAppSettings.Where(s => s.Parameter == "SpecialAdvantage" && s.LanguageId == languageId).ToList();
                    var vendorSettings = mobileAppSettings.Where(s => s.Parameter == "VendorDetails" && s.LanguageId == languageId).ToList();
                    var rentalConditionsSettings = mobileAppSettings.Where(s => s.Parameter.Contains("RentalConditions-"));
                    var totalKmLabel = labels.FirstOrDefault(l => l.LabelKodu == "VehicleMobile.VehicleDetail.TotalKmLimit")?.Labeladi ?? "[totalkm]";
                    var detailsLabelDict = labels
                        .Where(l => !string.IsNullOrEmpty(l.LabelKodu))
                        .GroupBy(l => l.LabelKodu)
                        .ToDictionary(g => g.Key, g => g.First().Labeladi);
                    var vehicleDetails = await _memoryCacheService.GetVehicleDetails();
                    var allBadges = await _memoryCacheService.GetBadges();
                    var badges = allBadges.Where(b => b.LanguageId == languageId).ToList();
                    //var vendorCoupons = await _memoryCacheService.GetVendorActiveCoupons();
                    var currencySymbol = await GetCurrencySymbol(vehicle.CurrencyCode);

                    var getDetailsResponseDto = new GetDetailsResponseDto();
                    getDetailsResponseDto.SearchId = sessionId;
                    var extras = (serviceResponse.Data as GetExtrasResponse).Extras?.Where(e => e.ExtraType == AdditionalProductTypes.Extra || e.ExtraType == AdditionalProductTypes.Insurance)?.ToList();
                    var deliveryTypes = await _memoryCacheService.GetVendorLocationDeliveryTypes();
                    var deliveryTypeId = await GetDeliveryTypeIdFromVendorLocation(vehicle.VendorId, vehicle.PickupLocationId);
                    var conditions = await _memoryCacheService.GetVendorLocationCondition(vehicle.VendorId, vehicle.PickupLocationId, languageId);

                    vehicle.DeliveryType = deliveryTypeId > 0 ? (DeliveryType)deliveryTypeId : (DeliveryType)await GetDeliveryTypeId(vehicle.IsOffice, vehicle.IsAirport, languageId);
                    getDetailsResponseDto.Extras = await CreateAndEditExtras(extras, vehicle.RentalDuration);
                    getDetailsResponseDto.Insurances = (serviceResponse.Data as GetExtrasResponse).Extras?.Where(e => e.ExtraType == AdditionalProductTypes.Insurance)?.ToList();
                    getDetailsResponseDto.VehicleFeatures = await CreateVehicleFeatures(vehicle, vehicleFeatures, labels, detailsLabelDict, podomain, languageId, totalKmLabel, MobilePages.Details);
                    getDetailsResponseDto.VehicleDetails = await CreateVehicleDetailsAsync(vehicle, vehicleDetails, labels, detailsLabelDict, podomain, languageId, currencySymbol);
                    getDetailsResponseDto.VehicleBadges = CreateVehicleBadges(vehicle, badges, podomain);
                    getDetailsResponseDto.RequiredDocumentsOnDelivery = GetRequiredDocuments(requiredSettings, vehicle, podomain);
                    getDetailsResponseDto.SpecialAdvantages = GetSpecialAdvantages(specialSettings, vehicle, podomain);
                    getDetailsResponseDto.VendorDetails = await CreateVendorDetailsForDetails(vehicle, labels, languageId);
                    getDetailsResponseDto.VendorDetails.ReservationVendorDetail = GetVendorDetails(vendorSettings, vehicle, podomain);
                    getDetailsResponseDto.PriceDetails = await CreatePriceDetailsForDetails(vehicle, labels);
                    getDetailsResponseDto.AllPriceDetails = await CreatePriceInfoForSuccess(new Reservation
                    {
                        VendorId = vehicle.VendorId,
                        PickupLocationId = vehicle.PickupLocationId,
                        PaidAmount = (vehicle.RentalDuration * vehicle.DailyPrice).Round(),
                        CurrencyCode = vehicle.CurrencyCode,
                        DailyPrice = vehicle.DailyPrice.Round(),
                        RentalDuration = vehicle.RentalDuration,
                        OneWayFee = vehicle.OneWayFee,
                    }, labels);
                    //getDetailsResponseDto.VehiclePromotion = CreateVehiclePromotions(vehicle, vendorCoupons, labels,vehicle.PickupLocationId, currencySymbol);
                    getDetailsResponseDto.RentalConditions = await CreateRentalConditionsForDetails(vehicle, labels, podomain, rentalConditionsSettings);
                    getDetailsResponseDto.RentalConditionsV2 = await CreateRentalConditionsV2ForDetails(vehicle, labels, podomain, languageId);
                    getDetailsResponseDto.ReservationDetails = await CreateReservationDetails(vehicle, languageId);
                    getDetailsResponseDto.ReservationNote = (conditions?.Any() ?? false) ? new VendorLocationConditionDto
                    {
                        VendorId = vehicle.VendorId,
                        LocationId = vehicle.PickupLocationId,
                        Notes = conditions?.Where(c => !string.IsNullOrWhiteSpace(c.Conditions)).Select(c => c.Conditions)?.ToList(),
                        Conditions = conditions?.FirstOrDefault()?.Conditions
                    } : null;
                    getDetailsResponseDto.AlternativeVehicles = await CreateAlternativeVehicles(languageId, "TRY", alternativeVehicles, vehicle.PickupLocationId, _memoryCacheService);

                    if (oldToken != getDetailsDto.ReservationToken)
                    {
                        getDetailsResponseDto.IsReservationTokenChange = true;
                        var oldTokenDetail = await _resTokenService.GetReservationTokenByUniqueId(oldToken);

                        if (oldTokenDetail != null)
                        {
                            var oldPrice = (oldTokenDetail.DailyPrice * oldTokenDetail.RentalDuration + oldTokenDetail.OneWayFee).Round();

                            getDetailsResponseDto.IsPriceChanged = oldTokenDetail.DailyPrice != vehicle.DailyPrice;
                            getDetailsResponseDto.OldTotalPrice = oldPrice;
                        }
                    }

                    var awsResult = await _awsService.PushCheckoutData(vehicle, vendor, userDetail: null, couponDetail: null, sessionId, "", "ReservateNow", await _agencyService.GetCurrentAgencyType());

                    return Ok(new
                    {
                        data = getDetailsResponseDto,
                        success = true,
                        resultCode = ResultCodes.Success
                    });
                }

                return Ok(new
                {
                    data = "",
                    success = ResultCodes.Success,
                    resultCode = HttpStatusCode.OK
                });
            }
            catch (Exception e)
            {
                Serilog.Log.Error("{@GetDetailsError}", $"{e.Message}-{e.StackTrace}-{e.InnerException?.Message}");

                return Ok(new
                {
                    data = "",
                    success = false,
                    resultCode = HttpStatusCode.OK,
                    message = e.ToStringNullSafe()
                });
            }
        }

        private async Task<List<VehicleDto>> CreateAlternativeVehicles(
            int languageId,
            string currencyCode,
            List<Vehicle> resultVehicles,
            int pickupLocationId,
            IMemoryCacheService memoryCacheService = null)
        {
            var cache = memoryCacheService ?? _memoryCacheService;
            var podomain = _parameterService.GetParameterValue("PODOMAIN");
            var currencies = await cache.GetCurrencies();
            var currencySymbol = await GetCurrencySymbol(currencyCode);
            var alllabels = await cache.GetLabels(languageId);
            var labels = alllabels.Where(l => l.Dilid == languageId).ToList();
            var labelDict = labels
                .Where(l => !string.IsNullOrEmpty(l.LabelKodu))
                .GroupBy(l => l.LabelKodu)
                .ToDictionary(g => g.Key, g => g.First().Labeladi);
            var totalKmLabel = labelDict.TryGetValue("VehicleMobile.VehicleList.TotalKmLimit", out var tkl) ? tkl : "[totalkm]";
            var vehicleDetails = await cache.GetVehicleDetails();
            var vehicleFutures = await cache.GetVehicleFutures();
            var settings = await cache.GetMobileSettings();
            var vendorLocationIconPath = podomain + settings?.FirstOrDefault(s => s.Parameter == "VendorDetails")?.IconPath;

            var vehicleDtos = new List<VehicleDto>();
            var vendorOffice = await cache.GetVendorOffices(pickupLocationId);
            var vendorOfficeDict = vendorOffice?.GroupBy(x => x.VendorId).ToDictionary(g => g.Key, g => g.First());

            foreach (var rv in resultVehicles)
            {
                var vehicleDto = new VehicleDto
                {
                    PickupLocationCode = rv.PickupLocationCode,
                    ReturnLocationCode = rv.ReturnLocationCode,
                    VehicleId = rv.VehicleId,
                    BaggageQuantityName = rv.BaggageQuantityName,
                    BaggageQuantityType = (int)rv.BaggageQuantityType,
                    CurrencyCode = rv.CurrencyCode,
                    DailyPrice = rv.DailyPrice.Round(),
                    DeliveryType = (int)rv.DeliveryType,
                    DepositPrice = rv.DepositPrice ?? 0,
                    OneWayFee = rv.OneWayFee,
                    PassengerQuantityName = rv.PassangerQuantityName,
                    PassengerQuantityType = (int)rv.PassangerQuantityType,
                    RentalDuration = rv.RentalDuration,
                    ReservationToken = rv.ReservationToken,
                    TotalPrice = rv.TotalPrice.Round(),
                    VehicleCategoryTypeName = rv.VehicleCategoryTypeName,
                    VehicleCategoryType = (int)rv.VehicleCategoryType,
                    ServiceCharge = rv.ServiceCharge,
                    TotalKmLimit = rv.TotalKMLimit ?? 0,
                    VehicleFuelTypeName = rv.FuelTypeName,
                    VehicleFuelType = (int)rv.FuelType,
                    VehicleName = rv.VehicleName,
                    VehicleTransmissionTypeName = rv.TransmissionTypeName,
                    VehicleTransmissionType = (int)rv.TransmissionType,
                    VehicleTypeName = rv.VehicleTypeName,
                    VehicleType = (int)rv.VehicleType,
                    VendorId = rv.VendorId,
                    VendorMinimumDriverAge = rv.VendorMinimumDriverAge,
                    VendorMinimumDrivingLicenseAge = rv.VendorMinimumDrivingLicenseAge,
                    IsFlightNumberRequired = vendorOfficeDict != null && vendorOfficeDict.TryGetValue(rv.VendorId, out var vo) ? vo.FlightCardRequired ?? false : false,
                    OrderNo = 0,
                    VehicleFeatures = await CreateVehicleFeatures(rv, vehicleFutures, labels, labelDict, podomain, languageId, totalKmLabel, MobilePages.VehicleList)
                };

                vehicleDto.VehicleDetails = await CreateVehicleDetailsAsync(rv, vehicleDetails, labels, labelDict, podomain, languageId, currencySymbol);
                vehicleDtos.Add(vehicleDto);
            }

            return vehicleDtos;
        }



        [HttpPost]
        [Route("GetNewReservationToken")]
        public async Task<IActionResult> GetNewReservationToken(GetNewReservationTokenRequest request)
        {
            try
            {
                var result = await GetNewVehicleToken(request.ReservationToken);

                if (string.IsNullOrEmpty(result))
                    return Ok(new
                    {
                        data = "",
                        success = false,
                        resultCode = HttpStatusCode.OK,
                        message = "Token alınamadı"
                    });

                var sessionId = await _reservationTokenService.GetReservationTokenSessionId(result);

                return Ok(new
                {
                    data = new
                    {
                        reservationToken = result,
                        sessionId = sessionId
                    },
                    success = true,
                    resultCode = HttpStatusCode.OK,
                    message = "Başarılı"
                });
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    data = "",
                    success = false,
                    resultCode = HttpStatusCode.OK,
                    message = e.ToStringNullSafe()
                });
            }
        }

        private async Task<string> GetNewVehicleToken(string oldReservationToken)
        {
            if (string.IsNullOrEmpty(oldReservationToken))
                return string.Empty;

            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(oldReservationToken);

            if (reservationToken != null)
            {
                var vendor = await _vendorService.GetVendorById(reservationToken.VendorId);
                var sessionCode = _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? "";

                var request = new GetVehiclesRequest
                {
                    VendorType = (VendorTypes)vendor.VendorType,
                    ApiKey = vendor.ApiKey,
                    ApiPassword = vendor.ApiPassword,
                    ApiClientId = vendor.ApiClientId,
                    ApiLocationCode = reservationToken.APIPickupLocationCode,
                    LanguageCode = reservationToken.LanguageType.ToString(),
                    CurrencyCode = reservationToken.CurrencyType.ToString(),
                    PickupLocationId = reservationToken.PickupLocationId,
                    ReturnLocationId = reservationToken.ReturnLocationId,
                    PickupDate = reservationToken.PickupDateTime.ToString("dd.MM.yyyy"),
                    ReturnDate = reservationToken.ReturnDateTime.ToString("dd.MM.yyyy"),
                    PickupTime = reservationToken.PickupDateTime.ToString("HH:mm"),
                    ReturnTime = reservationToken.ReturnDateTime.ToString("HH:mm"),
                    CouponCode = null,
                    SessionCode = sessionCode,
                    SecretKey = vendor.SecretKey
                };

                var data = await _vehicleService.GetVehicles(request, _agencyService.GetCurrentAgencyId(), request.SessionCode);
                var vehicles = data.Data as List<Vehicle>;

                var vehicle = vehicles?.FirstOrDefault(v => v.VehicleCode == reservationToken.VehicleCode);

                return vehicle != null ? vehicle.ReservationToken : string.Empty;
            }

            return string.Empty;
        }

        private async Task<string> ResolveTokenFromSearchContext(TokenSearchContext searchContext)
        {
            try
            {
                var vendors = await _memoryCacheService.GetLocationVendors(searchContext.PickupLocationId);
                var vendor = vendors?.FirstOrDefault(v => v.VendorId == searchContext.VendorId);

                if (vendor == null)
                    return string.Empty;

                var sessionCode = _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? "";

                var request = new GetVehiclesRequest
                {
                    VendorType = (VendorTypes)vendor.VendorType,
                    ApiKey = vendor.ApiKey,
                    ApiPassword = vendor.ApiPassword,
                    ApiClientId = vendor.ApiClientId,
                    ApiLocationCode = vendor.VendorLocationCode,
                    LanguageCode = "TR",
                    CurrencyCode = searchContext.CurrencyCode,
                    PickupLocationId = searchContext.PickupLocationId,
                    ReturnLocationId = searchContext.ReturnLocationId,
                    PickupDate = searchContext.PickupDate,
                    ReturnDate = searchContext.ReturnDate,
                    PickupTime = searchContext.PickupTime,
                    ReturnTime = searchContext.ReturnTime,
                    SessionCode = sessionCode,
                    SecretKey = vendor.SecretKey
                };

                var data = await _vehicleService.GetVehicles(request, _agencyService.GetCurrentAgencyId(), sessionCode);
                var vehicles = data.Data as List<Vehicle>;

                var vehicle = vehicles?.FirstOrDefault(v => v.VehicleId == searchContext.VehicleId && v.VendorId == searchContext.VendorId);

                return vehicle?.ReservationToken ?? string.Empty;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@ResolveTokenFromSearchContext}", $"{ex.Message}-{ex.StackTrace}");
                return string.Empty;
            }
        }

        private async Task<List<Extra>> CreateAndEditExtras(List<Extra> extras, int rentalDuration)
        {
            if (extras == null)
                return null;
            foreach (var extra in extras)
            {
                extra.UnitPrice = extra.Price;
                extra.Price = extra.ExtraRentalType == ExtraRentalTypes.Daily ? extra.UnitPrice * rentalDuration : extra.UnitPrice;
            }

            return extras;
        }

        private List<SpecialAdvantage> GetSpecialAdvantages(List<MobileAppSetting> specialSettings, Vehicle vehicle, string podomain)
        {
            return specialSettings.Select(s => new SpecialAdvantage
            {
                Parameter = s.Parameter,
                Value = s.Value,
                IconPath = podomain + s.IconPath,
                Order = s.Order,
            }).ToList();
        }

        private List<Detail> GetVendorDetails(List<MobileAppSetting> vendorSettings, Vehicle vehicle, string podomain)
        {
            List<Detail> detail = new List<Detail>();
            foreach (var item in vendorSettings)
            {
                var value = "";
                if (item.Value.Contains("[ShortAddress]"))
                {
                    value = vehicle.ShortAddress != null ? item.Value.Replace("[ShortAddress]", vehicle.ShortAddress.ToString()) : "";
                }

                if (!string.IsNullOrEmpty(value))
                {
                    detail.Add(new Detail
                    {
                        Parameter = item.Parameter,
                        Text = value,
                        IconPath = podomain + item.IconPath
                    });
                }
            }

            return detail;
        }


        private async Task<MobileReservationDetails> CreateReservationDetails(Vehicle vehicle, int languageId)
        {
            var vendorOffice = await _vendorOfficeService.GetVendorOffice(vehicle.VendorId, vehicle.PickupLocationId);

            var pickupLocationCode = string.Empty;
            var returnLocationCode = string.Empty;

            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(vehicle.ReservationToken);
            if (reservationToken != null)
            {
                pickupLocationCode = reservationToken.APIPickupLocationCode;
                returnLocationCode = reservationToken.APIReturnLocationCode;
            }

            return new MobileReservationDetails
            {
                RezToken = vehicle.ReservationToken,
                PickupDate = vehicle.PickupDateTime,
                ReturnDate = vehicle.ReturnDateTime,
                VehicleName = vehicle.VehicleName,
                VehicleImages = vehicle.VehicleImages,
                VehicleBrandName = vehicle.VehicleBrandName,
                VehicleModelName = vehicle.VehicleModelName,
                PickupLocationName = vehicle.PickupLocationName,
                PickupLocationId = vehicle.PickupLocationId,
                ReturnLocationName = vehicle.ReturnLocationName,
                ReturnLocationId = vehicle.ReturnLocationId,
                RentalDuration = vehicle.RentalDuration,
                TotalKMLimit = vehicle.TotalKMLimit,
                VehicleCategoryTypeName = vehicle.VehicleCategoryTypeName,
                IsFlightNumberRequired = vendorOffice?.FlightCardRequired ?? false,
                GearType = vehicle.TransmissionTypeName,
                FuelType = vehicle.FuelTypeName,
                PickupLocation = await CreateMobileLocation(vehicle.PickupLocationId, languageId),
                ReturnLocation = await CreateMobileLocation(vehicle.ReturnLocationId, languageId),
                PickupLocationCode = string.IsNullOrEmpty(pickupLocationCode) ? vehicle.PickupLocationCode : pickupLocationCode,
                ReturnLocationCode = string.IsNullOrEmpty(returnLocationCode) ? vehicle.ReturnLocationCode : returnLocationCode,
            };
        }

        private async Task<MobileRentalConditions> CreateRentalConditionsForDetails(Vehicle vehicle, List<Label> labels, string podomain, IEnumerable<MobileAppSetting> rentalConditionsSettings)
        {
            var mobileRentalConditions = new List<MobileRentalCondition>();
            foreach (var condition in vehicle.RentalConditions)
            {
                var value = rentalConditionsSettings.Where(c => c.Parameter == $"RentalConditions-{condition.ConditionId}").FirstOrDefault().Value;

                mobileRentalConditions.Add(new MobileRentalCondition
                {
                    ConditionId = condition.ConditionId,
                    ConditionName = condition.ConditionName,
                    Value = value,
                    IconPath = podomain + condition.IconPath
                });
            }

            return new MobileRentalConditions
            {
                Header = labels.FirstOrDefault(l => l.LabelKodu == "MobileDetails.RentalConditionsHeader")?.Labeladi,
                RentalConditions = mobileRentalConditions,
            };
        }

        private async Task<MobileRentalConditionsNew> CreateRentalConditionsV2ForDetails(Vehicle vehicle, List<Label> labels, string podomain, int languageId)
        {
            var mobileRentalConditions = new List<MobileRentalCondition>();

            var rentalContract = await CreateRentalContract(languageId, vehicle.ReservationToken);

            return new MobileRentalConditionsNew
            {
                Header = labels.FirstOrDefault(l => l.LabelKodu == "MobileDetails.RentalConditionsHeader")?.Labeladi,
                Text = rentalContract?.Editor?.ToString(),
            };
        }

        private async Task<MobilePriceDetails> CreatePriceDetailsForDetails(Vehicle vehicle, List<Label> labels)
        {
            var locationFee = await _vendorService.GetVendorLocationFeeByVendorIdAndLocationId(vehicle.VendorId, vehicle.PickupLocationId, vehicle.DailyPrice * vehicle.RentalDuration);
            var vendorLocationfee = locationFee;

            return new MobilePriceDetails
            {
                VendorLocationFee = vendorLocationfee,
                Header = labels.FirstOrDefault(l => l.LabelKodu == "MobileDetails.PriceDetailsHeader")?.Labeladi,
                RentalDuration = vehicle.RentalDuration,
                DailyPrice = vehicle.DailyPrice.Round(),
                OneWayFee = vehicle.OneWayFee,
                ExtraPrice = vehicle.ExtraPrice,
                TotalPrice = vehicle.TotalPrice.Round(),
                CurrencyCode = vehicle.CurrencyCode,
            };
        }

        private async Task<MobileVendorDetails> CreateVendorDetailsForDetails(Vehicle vehicle, List<Label> labels, int languageId)
        {
            var settings = await _memoryCacheService.GetMobileSettings();
            var podomain = _parameterService.GetParameterValue("PODOMAIN");
            var vendorLocationIconPath = podomain + settings?.FirstOrDefault(s => s.Parameter == "VendorDetails")?.IconPath;
            var vendor = await _vendorService.GetVendorByIdAll(vehicle.VendorId);
            var locationVendorContacts = await _memoryCacheService.GetLocationVendorContacts(vehicle.PickupLocationId);

            var allComments = await _surveyService.GetCommentsByLanguageIdLocationId(languageId, vehicle.PickupLocationId);
            var comments = allComments.Where(c => c.VendorId == vehicle.VendorId);
            var externalComments = await _surveyService.GetExternalCommentsByLanguageIdLocationId(languageId, vehicle.PickupLocationId, vehicle.VendorId);
            var vendorScore = comments.Count() != 0 || externalComments.Count() != 0
                ? (comments.Sum(c => c.Score) + externalComments.Sum(e => e.Score)) / (comments.Count() + externalComments.Count())
                : 0;
            var commentCount = comments.GroupBy(c => c.Id).Count() + externalComments.Count();

            var deliveryTypesCache = await _memoryCacheService.GetDeliveryTypes();
            var deliveryTypes = deliveryTypesCache.Where(d => d.LanguageId == languageId);
            var deliveryTypeId = (int)vehicle.DeliveryType > 0
                                    ? (int)vehicle.DeliveryType
                                    : await GetDeliveryTypeId(vehicle.IsOffice, vehicle.IsAirport, languageId);
            var returnDeliveryTypeId = vehicle.PickupLocationId == vehicle.ReturnLocationId
                                       ? deliveryTypeId
                                       : await GetDeliveryTypeIdFromVendorLocation(vendor.Vendorid, vehicle.ReturnLocationId);
            var deliveryType = deliveryTypes.FirstOrDefault(d => d.DeliveryTypeId == deliveryTypeId);

            return new MobileVendorDetails
            {
                OfficeLocation = CreateVehicleOfficeLocations(vehicle, locationVendorContacts, vendorLocationIconPath),
                Header = labels.FirstOrDefault(l => l.LabelKodu == "MobileDetails.VendorDetailsHeader")?.Labeladi,
                VendorId = vehicle.VendorId,
                VendorPhone = vehicle.VendorPhone,
                VendorEmail = vehicle.VendorEmail,
                VendorName = vehicle.VendorName,
                VendorLogo = vehicle.VendorLogo,
                VendorScore = Math.Round((decimal)vendorScore, 1),
                VendorCommentCount = commentCount,
                DeliveryType = deliveryTypeId,
                ReturnDeliveryType = returnDeliveryTypeId,
                DeliveryTypeName = vehicle.DeliveryType,
                IsFindeksRequired = vehicle.IsFindeksRequired,
                IsCouponActive = vendor.Couponcodeactive ?? false
            };

        }

        private List<RequiredDocument> GetRequiredDocuments(List<MobileAppSetting> settings, Vehicle vehicle, string podomain)
        {
            return settings.Select(s => new RequiredDocument
            {
                Id = s.Id,
                Parameter = s.Parameter,
                Value = s.Value.Replace("[MinLicenseAge]", vehicle.VendorMinimumDrivingLicenseAge.ToString()),
                IconPath = podomain + s.IconPath,
                Order = s.Order,
                Type = s.Type ?? 0
            }).ToList();
        }

        #endregion

        #region Get Coupon Action

        [HttpPost]
        [Route("GetCouponDetails")]
        public async Task<IActionResult> GetCouponDetails(GetCouponDetailsDto getCouponDetailsDto)
        {
            if (string.IsNullOrWhiteSpace(getCouponDetailsDto.ReservationToken))
                return BadRequest(new
                {
                    httpResultType = (int)HttpStatusCode.BadRequest,
                    success = false,
                    message = "Check your reservationToken!"
                });
            var currencies = await _memoryCacheService.GetCurrencies();

            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(getCouponDetailsDto.ReservationToken);
            if (reservationToken != null)
            {
                var languageId = await _memoryCacheService.GetLanguageId(getCouponDetailsDto.LanguageCode);

                if (reservationToken != null)
                {
                    GetCouponDetailsResponseDto couponProcedureParameters = new GetCouponDetailsResponseDto()
                    {
                        CouponCode = getCouponDetailsDto.CouponCode,
                        CustomerMailAddress = "",
                        LanguageId = languageId,
                        CurrencyId = currencies.FirstOrDefault(c => c.Currencyisocode == getCouponDetailsDto.CurrencyCode.ToStringNullSafe().ToUpper()).Currencyid,
                        VendorId = reservationToken.VendorId,
                        TotalPrice = (reservationToken.DailyPrice * reservationToken.RentalDuration).Round().ToString(),
                        PickupDate = reservationToken.PickupDateTime.ToString(),
                        ReturnDate = reservationToken.ReturnDateTime.ToString(),
                        PickupLocationId = reservationToken.PickupLocationId,
                        ReturnLocationId = reservationToken.ReturnLocationId,
                        RentalDuration = reservationToken.RentalDuration,
                        AgencyId = (int)reservationToken.AgencyId
                    };

                    var couponResult = await _couponService.GetCouponResults(couponProcedureParameters);

                    if (couponResult != null && couponResult.Result == 1)
                    {
                        return Ok(new
                        {
                            data = couponResult,
                            success = true,
                            resultCode = HttpStatusCode.OK
                        });
                    }
                    return Ok(new
                    {
                        data = couponResult,
                        success = false,
                        resultCode = HttpStatusCode.OK,
                        message = couponResult?.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        httpResultType = (int)HttpStatusCode.BadRequest,
                        success = false,
                        message = "Check your reservationToken!"
                    });
                }
            }
            return BadRequest(new
            {
                httpResultType = (int)HttpStatusCode.BadRequest,
                success = false,
                message = "Check your reservationToken!"
            });
        }

        #endregion

        #region Get Contracts Action

        [HttpPost]
        [Route("GetContracts")]
        public async Task<IActionResult> GetContracts(GetContractsRequestDto getContractsRequest)
        {
            if (string.IsNullOrEmpty(getContractsRequest.ContractType))
                return BadRequest(new
                {
                    httpResultType = (int)HttpStatusCode.BadRequest,
                    success = false,
                    message = "Check your contractType!"
                });

            var languageId = await _memoryCacheService.GetLanguageId(getContractsRequest.LanguageCode);
            var result = new Icerikdil();

            switch (getContractsRequest.ContractType)
            {
                case "cvc":
                    result = await CreateCvcContract(languageId);

                    return Ok(new
                    {
                        data = result != null ? result.Editor : null,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                case "clarification":
                    result = await CreateClarificationContract(languageId);

                    return Ok(new
                    {
                        data = result != null ? result.Editor : null,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                case "information" when !string.IsNullOrEmpty(getContractsRequest.ReservationToken):
                    result = await CreateInformationContract(languageId, getContractsRequest.ReservationToken);

                    return Ok(new
                    {
                        data = result != null ? result.Editor : null,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                case "kvkk":
                    result = await CreateKvkkContract(languageId);

                    return Ok(new
                    {
                        data = result != null ? result.Editor : null,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                case "rentalConditions":
                    result = await CreateRentalConditionContract(languageId);
                    return Ok(new
                    {
                        data = result != null ? result.Editor : null,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                case "rentalContract" when !string.IsNullOrEmpty(getContractsRequest.ReservationToken):
                    result = await CreateRentalContract(languageId, getContractsRequest.ReservationToken);
                    return Ok(new
                    {
                        data = result != null ? result.Editor : null,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                case "sales" when !string.IsNullOrEmpty(getContractsRequest.ReservationToken):
                    result = await CreateSalesContract(languageId, getContractsRequest.ReservationToken);

                    return Ok(new
                    {
                        data = result != null ? result.Editor : null,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                default:
                    return BadRequest(new
                    {
                        httpResultType = (int)HttpStatusCode.BadRequest,
                        success = false,
                        message = "Empty parameters!"
                    });
            }
        }

        private async Task<Icerikdil> CreateSalesContract(int languageId, string reservationToken)
        {
            var resToken = await _resTokenService.GetReservationTokenByUniqueId(reservationToken);
            if (reservationToken != null)
            {
                var salesContracts = await _memoryCacheService.GetContentLanguageBySettings(_contentService.GetSalesContractViewSettings(), languageId);

                var salesContract = salesContracts?.FirstOrDefault() ?? null;

                if (salesContract != null)
                {
                    var editorText = salesContract.Editor;

                    var allVendors = await _vendorService.GetAllActiveVendors();
                    var vendorId = resToken.VendorId;
                    var selectedVendor = allVendors.FirstOrDefault(v => v.Vendorid == vendorId);
                    var pickupContactInformation = await _vendorContactInformationService.GetVendorContactInformation(resToken.PickupLocationId, vendorId);
                    var dropContactInformation = await _vendorContactInformationService.GetVendorContactInformation(resToken.ReturnLocationId, vendorId);
                    var vehicleFuelLangs = await _memoryCacheService.GetVehicleFuelLangs();
                    var vehicleFuelName = vehicleFuelLangs
                                         .FirstOrDefault(f => f.Langid == languageId && f.Fuelid == ((int)resToken.FuelType + 1))
                                         ?.Fuelname ?? "";
                    var vehicleTransmissionTypeLangs = await _memoryCacheService.GetVehicleTransmissionLangs();
                    var vehicleTransmissionName = vehicleTransmissionTypeLangs
                                         .FirstOrDefault(t => t.Langid == languageId && t.Transmissionid == ((int)resToken.TransmissionType + 1))
                                         ?.Transmissionname ?? "";

                    var pickupAddress = pickupContactInformation?.Address ?? "";
                    var returnAddress = dropContactInformation?.Address ?? "";

                    editorText = editorText.Replace("{{VENDOR_NAME}}", selectedVendor.Vendorname);
                    editorText = editorText.Replace("{{VENDOR_ADDRESS}}", selectedVendor.Address);
                    editorText = editorText.Replace("{{VENDOR_PHONE}}", selectedVendor.Phone);
                    editorText = editorText.Replace("{{VENDOR_MAIL}}", selectedVendor.Email);

                    editorText = editorText.Replace("{{REZ_PICKUP_DATE}}", resToken.PickupDateTime.ToString("dd.MM.yyyy HH:mm"));
                    editorText = editorText.Replace("{{REZ_RETURN_DATE}}", resToken.ReturnDateTime.ToString("dd.MM.yyyy HH:mm"));
                    editorText = editorText.Replace("{{REZ_RENTAL_DURATION}}", $"{resToken.RentalDuration} Gün");

                    editorText = editorText.Replace("{{PICKUPLOCATION_ADDRESS}}", $"{pickupAddress}");
                    editorText = editorText.Replace("{{RETURNLOCATION_ADDRESS}}", $"{returnAddress}");

                    editorText = editorText.Replace("{{VEHICLE_MODEL}}", resToken.VehicleName);
                    editorText = editorText.Replace("{{FUEL_TYPE}}", vehicleFuelName);
                    editorText = editorText.Replace("{{TRANSMISSION_TYPE}}", vehicleTransmissionName);

                    editorText = editorText.Replace("{{TOTAL_PRICE}}", (resToken.RentalDuration * resToken.DailyPrice).Round().ToString());
                    editorText = editorText.Replace("{{EXTRAS_PRICE}}", 0f.ToString()); // TODO: hesaplanacak
                    editorText = editorText.Replace("{{PAYMENT_METHOD}}", ""); // TODO: sorulacak
                    editorText = editorText.Replace("{{PAYMENT_PRICE}}", ""); // TODO: sorulacak

                    salesContract.Editor = editorText;
                    return salesContract;
                }
            }
            return Activator.CreateInstance<Icerikdil>();
        }

        private async Task<Icerikdil> CreateRentalContract(int languageId, string resToken)
        {
            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(resToken);
            if (reservationToken != null)
            {
                //var contents = await _contentService.GetAllActiveContents();
                var allContactContentLanguages = await _memoryCacheService.GetAllActiveContentLanguages((int)ContentTypes.TedarikciSozlesmesi, languageId);
                var contents = allContactContentLanguages.Select(c => c.Icerik);
                var vendorContractContentId = contents.FirstOrDefault(c => c.Vendorid == reservationToken.VendorId)?.Icerikid ?? -1;
                var rentalContract = allContactContentLanguages.Where(cl => cl.Icerikid == vendorContractContentId);

                return rentalContract?.FirstOrDefault();
            }
            return Activator.CreateInstance<Icerikdil>();
        }

        private async Task<Icerikdil> CreateRentalConditionContract(int languageId)
        {
            var rentalConditions = await _memoryCacheService.GetContentLanguageBySettings(_contentService.GetRentalConditionViewSettings(), languageId);

            return rentalConditions?.FirstOrDefault();
        }

        private async Task<Icerikdil> CreateKvkkContract(int languageId)
        {
            var kvkk = await _memoryCacheService.GetContentLanguageBySettings(_contentService.GetKvkkViewSettings(), languageId);

            return kvkk?.FirstOrDefault();
        }

        private async Task<Icerikdil> CreateInformationContract(int languageId, string resToken)
        {
            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(resToken);
            if (reservationToken != null)
            {
                var informations = await _memoryCacheService.GetContentLanguageBySettings(_contentService.GetInformationViewSettings(), languageId);

                var information = informations?.FirstOrDefault() ?? null;

                if (information != null)
                {
                    var editorText = information.Editor;

                    var allVendors = await _vendorService.GetAllActiveVendors();
                    var vendorId = reservationToken.VendorId;
                    var selectedVendor = allVendors.FirstOrDefault(v => v.Vendorid == vendorId);
                    var allLocations = await _locationService.GetLocationsByLanguageId(languageId);

                    editorText = editorText.Replace("{{VENDOR_NAME}}", selectedVendor.Vendorname);
                    editorText = editorText.Replace("{{VENDOR_ADDRESS}}", selectedVendor.Address);
                    editorText = editorText.Replace("{{VENDOR_PHONE}}", selectedVendor.Phone);
                    editorText = editorText.Replace("{{VENDOR_MAIL}}", selectedVendor.Email);

                    editorText = editorText.Replace("{{REZ_DATE}}", reservationToken.PickupDateTime.ToString("dd.MM.yyyy HH:mm"));
                    editorText = editorText.Replace("{{REZ_RETURN_DATE}}", reservationToken.ReturnDateTime.ToString("dd.MM.yyyy HH:mm"));
                    editorText = editorText.Replace("{{SUM_REZ_DAY}}", $"{reservationToken.RentalDuration} Gün");
                    editorText = editorText.Replace("{{PICKUP_LOCATION}}", $"{allLocations.Where(l => l.Id == reservationToken.PickupLocationId).FirstOrDefault().Locationname}");
                    editorText = editorText.Replace("{{DROP_LOCATION}}", $"{allLocations.Where(l => l.Id == reservationToken.ReturnLocationId).FirstOrDefault().Locationname}");

                    editorText = editorText.Replace("{{VEHICLE_MODEL}}", reservationToken.VehicleName);
                    editorText = editorText.Replace("{{FUEL_TYPE}}", reservationToken.FuelType.ToString());
                    editorText = editorText.Replace("{{TRANSMISSION_TYPE}}", reservationToken.TransmissionTypeName);
                    editorText = editorText.Replace("{{TOTAL_PERSON}}", ""); // TODO: sorulacak

                    editorText = editorText.Replace("{{TOTAL_PRICE}}", reservationToken.APITotalPrice.ToString());
                    editorText = editorText.Replace("{{EXTRA_PRICE}}", 0f.ToString()); // TODO: hesaplanacak
                    editorText = editorText.Replace("{{PAYMENT_METHOD}}", ""); // TODO: sorulacak
                    editorText = editorText.Replace("{{PAYMENT_PRICE}}", ""); // TODO: sorulacak

                    editorText = editorText.Replace("{{TODAY_DATE}}", DateTime.Now.ToString("dd.MM.yyyy HH:mm")); // TODO: sorulacak

                    information.Editor = editorText;

                    return information;
                }
            }
            return Activator.CreateInstance<Icerikdil>();
        }

        private async Task<Icerikdil> CreateClarificationContract(int languageId)
        {
            var clarificationText = await _memoryCacheService.GetContentLanguageBySettings(_contentService.GetClarificationTextViewSettings(), languageId);

            return clarificationText?.FirstOrDefault();
        }

        private async Task<Icerikdil> CreateCvcContract(int languageId)
        {
            var cvc = await _memoryCacheService.GetContentLanguageBySettings(_contentService.GetCvcViewSettings(), languageId);

            return cvc?.FirstOrDefault();
        }

        #endregion

        #region Post Reservation Action
        [HttpPost]
        [Route("PostReservation")]
        public async Task<IActionResult> PostReservation(PostReservationRequestDto postReservationRequestDto)
        {
            try
            {
                Serilog.Log.Error("{@MobilePostReservationRequest}", postReservationRequestDto.ToJson());

                var reservationTokenObj = await _resTokenService.GetReservationTokenByUniqueId(postReservationRequestDto.ReservationToken);

                Serilog.Log.Error("{@MobileReservationToken}", reservationTokenObj);

                if (reservationTokenObj is null)
                {
                    return Ok(new
                    {
                        data = "",
                        success = false,
                        resultCode = HttpStatusCode.OK,
                        message = "Reservation token not exist!"
                    });
                }

                postReservationRequestDto.LanguageCode = postReservationRequestDto.LanguageCode.Contains("-")
                                                        ? postReservationRequestDto.LanguageCode.Split('-')[0]
                                                        : postReservationRequestDto.LanguageCode;
                var postReservationResponse = await CreateReservation(postReservationRequestDto, reservationTokenObj);

                if (HttpContext.RequestAborted.IsCancellationRequested)
                {
                    Serilog.Log.Error("{@MobilePostReservationCancelRequestError}", postReservationRequestDto.ToJson());
                    return Ok(new
                    {
                        data = "",
                        success = false,
                        resultCode = HttpStatusCode.OK,
                        message = "İstek client tarafından iptal edildi!"
                    });
                }

                if (postReservationResponse.Data != null)
                {
                    var response = postReservationResponse.Data as Reservation;

                    var result = await CreateSuccessModel(response, postReservationRequestDto.LanguageCode);
                    result.RequestId = postReservationRequestDto.RequestId;
                    #region AWS Payment Results

                    if (_configuration.GetSectionValueBool("AWSKinesis", "Active"))
                    {
                        var userDetails =
                            await _reservationDetailService.GetByReservationTokenWithDetail(postReservationRequestDto
                                .ReservationToken);
                        if (userDetails != null)
                        {
                            var serviceResponse = await _extraService.GetExtras(new GetExtrasRequest
                            {
                                ReservationToken = postReservationRequestDto.ReservationToken,
                                LanguageCode = postReservationRequestDto.LanguageCode.ToUpper()
                            });
                            var vehicle = (serviceResponse.Data as GetExtrasResponse).Vehicle;
                            var vendor = await _vendorService.GetVendorById(vehicle.VendorId);
                            var sessionId =
                                await _reservationTokenService.GetReservationTokenSessionId(postReservationRequestDto.ReservationToken);

                            var driverInfo = userDetails.ReservationDriverInfos.FirstOrDefault();
                            var paymentInfo = userDetails.ReservationPaymentDetails.FirstOrDefault();
                            var creditCardNo = paymentInfo?.CreditCardNumber;
                            var installmentCount = paymentInfo?.InstallmentCount;
                            var installmentCommission = paymentInfo?.InstallmentCommissionAmount;
                            var couponCode = userDetails.CouponCode;
                            var couponAmount = userDetails.CouponDiscountAmount;

                            var userDetail = new kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.UserDetailModel
                            {
                                CustomerEmail = driverInfo?.Email,
                                CustomerPhone = $"{driverInfo?.CountryPhoneCode}{driverInfo?.PhoneNumber}",
                                PaymentType = "cash",
                                PaymentMethod = "Masterpass Ödeme Sistemi",
                                PaymentCard = $"{creditCardNo[..6]}*****{creditCardNo[^4..]}",
                                InstallmentCount = installmentCount ?? 0,
                                LateCharge = (decimal)(installmentCommission ?? 0),
                                ContactPermission = driverInfo?.ContactPermission ?? false
                            };

                            var couponDetail = new kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.CouponModel
                            {
                                CouponCode = couponCode,
                                CouponName = couponCode,// TODO : Düzeltme yapılacak
                                CouponAmount = couponAmount ?? 0
                            };

                            var awsResult = await _awsService.PushCheckoutData(vehicle, vendor, userDetail, couponDetail, sessionId, paymentInfo.PaymentCode, "PaymentSuccess", await _agencyService.GetCurrentAgencyType());
                        }
                    }

                    #endregion

                    return Ok(new
                    {
                        data = result,
                        success = true,
                        resultCode = HttpStatusCode.OK
                    });
                }

                return Ok(new
                {
                    data = "",
                    success = false,
                    resultCode = HttpStatusCode.OK,
                    message = string.IsNullOrEmpty(postReservationResponse.Message)
                              ? "Rezervasyon oluşturulamadı!"
                              : postReservationResponse.Message
                });
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    data = "",
                    success = false,
                    resultCode = HttpStatusCode.OK,
                    message = e.Message + e.StackTrace
                });
            }
        }

        private async Task<MobileReservationSuccessModel> CreateSuccessModel(Reservation response, string languageCode)
        {
            var languageId = await _memoryCacheService.GetLanguageId(languageCode);
            var alllabels = await _memoryCacheService.GetLabels(languageId);
            var labels = alllabels.Where(l => l.Dilid == languageId).ToList();

            var reservationSuccessModel = new MobileReservationSuccessModel();

            reservationSuccessModel.ReservationSuccess = await CreateReserVationSuccess(response, labels);

            return reservationSuccessModel;
        }

        #region PriceInfoForSuccess

        private async Task<PriceInformationSuccess> CreatePriceInfoForSuccess(Reservation response, List<Label> labels)
        {
            var locationFee = await _vendorService.GetVendorLocationFeeByVendorIdAndLocationId(response.VendorId, response.PickupLocationId, (response.RentalDuration * response.
                DailyPrice).Round());
            var vendorLocationfee = locationFee;

            var cardTitle = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.PriceInfoCardTitle").Labeladi;
            var totalPriceTitle = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.TotalPriceTitle").Labeladi;

            var currencies = await _memoryCacheService.GetCurrencies();
            var currency = currencies.FirstOrDefault(c => c.Currencyisocode == response.CurrencyCode);
            var currencySymbol = currency?.Symbol ?? "";

            return new PriceInformationSuccess
            {
                CardTitle = cardTitle,
                CurrencyCode = response.CurrencyCode,
                TotalPriceTitle = totalPriceTitle,
                TotalPrice = response.PaidAmount.Round(),
                DailyPrice = response.DailyPrice.Round(),
                CouponInfo = response.CouponDiscountAmount > 0 ? new CouponInfo
                {
                    CouponCode = response.CouponCode,
                    CouponDiscountAmount = response.CouponDiscountAmount,
                } : null,
                CardPaymentInfo = new PaymentInfo
                {
                    TotalAmount = response.PaidAmount,
                    Details = CreateCardPaymentInfo(response, labels, vendorLocationfee, currencySymbol)
                },
                OfficePaymentInfo = new PaymentInfo
                {
                    TotalAmount = response.ExtraPrice + response.OneWayFee + vendorLocationfee,
                    Details = CreateOfficePaymentInfo(response, labels, vendorLocationfee, currencySymbol)
                },
                Extras = response.ReservationExtras != null ? response.ReservationExtras?.MapExtras() : null
            };


        }

        private List<PaymentDetail> CreateOfficePaymentInfo(Reservation response, List<Label> labels, float vendorLocationfee, string currencySymbol)
        {
            var oneWayFeeText = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.OneWayFeeText").Labeladi;
            var officeServiceFeeText = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.OfficeServiceFeeText").Labeladi;

            var paymentDetails = new List<PaymentDetail>();

            if (response.OneWayFee > 0)
            {
                paymentDetails.Add(new PaymentDetail
                {
                    Name = oneWayFeeText,
                    Description = response.OneWayFee.ToString() + currencySymbol,
                    Amount = response.OneWayFee
                });
            }

            if (vendorLocationfee > 0)
            {
                paymentDetails.Add(new PaymentDetail
                {
                    Name = officeServiceFeeText,
                    Description = vendorLocationfee.ToString() + currencySymbol,
                    Amount = vendorLocationfee
                });
            }
            if (response?.ReservationExtras != null)
            {
                foreach (var extra in response.ReservationExtras)
                {
                    paymentDetails.Add(new PaymentDetail
                    {
                        Name = extra.ExtraName,
                        Description = extra.ExtraDescription,
                        Amount = extra.ExtraRentalType == ExtraRentalTypes.PerRental
                                ? extra.Price
                                : extra.Price * response.RentalDuration
                    });
                }
            }

            return paymentDetails;
        }

        private List<PaymentDetail> CreateCardPaymentInfo(Reservation response, List<Label> labels, float locationFee, string currencySymbol)
        {
            var totalPrice = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.TotalPriceText").Labeladi;
            var totalPriceDescription = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.TotalPriceDescriptionText").Labeladi;
            var couponTitle = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.CouponTitle").Labeladi;

            var paymentDetails = new List<PaymentDetail>();

            paymentDetails.Add(new PaymentDetail
            {
                Name = totalPrice,
                Description = totalPriceDescription.Replace("[RentalDuration]", response.RentalDuration.ToString()).Replace("[CurrencySymbol]", currencySymbol).Replace("[DailyPrice]", response.DailyPrice.Round().ToString()),
                Amount = response.RentalDuration * response.DailyPrice.Round(),
            });

            if (response.CouponDiscountAmount > 0)
            {
                paymentDetails.Add(new PaymentDetail
                {
                    Name = couponTitle,
                    Description = null, //Fatmanur'un Talebi İle Null yapıldı
                    Amount = -response.CouponDiscountAmount
                });
            }
            return paymentDetails;
        }

        #endregion

        #region ReservationSuccess

        private async Task<ReservationSuccess> CreateReserVationSuccess(Reservation response, List<Label> labels)
        {
            var successMessage = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.ReservationSuccessMessage").Labeladi;
            var rezNumberTitle = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.ReservationNumberTitle").Labeladi;
            var rezDetailsText = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.Success.ReservationSuccessDetailsText").Labeladi;

            return new ReservationSuccess
            {
                SuccessMessage = successMessage,
                ReservationNumber = new ReservationNumber
                {
                    Title = rezNumberTitle,
                    Number = response.ReservationNumber.ToString()
                },
                ReferanceNumber = response.APIReservationNumber,
                DetailsText = rezDetailsText.Replace("[DriverEmail]", response.CustomerMail).Replace("[DriverPhone]", response.CustomerPhone)
            };
        }

        #endregion

        #region CreateReservation

        private async Task<HttpResult<object>> CreateReservation(PostReservationRequestDto postReservationRequestDto, ReservationToken reservationTokenObj)
        {
            var jwtToken = _agencyService.GetCurrentBearerToken();
            var postReservationResponse = new HttpResult<object>();
            var postReservationRequest = await GetPostReservationRequest(postReservationRequestDto);

            Serilog.Log.Error("{@MobilePostReservationRequestStep2}", postReservationRequest);

            var reservationId = await _reservationStepsService.CreateNewResIdIfExist();
            var reservationNumber = ReservationHelper.GenerateReservationNumber(reservationId);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = reservationNumber,
                Content = JsonConvert.SerializeObject(GetReservationRequestObjectForLog(_userRole, postReservationRequest)),
                LogType = BrokerLogTypes.ReservationRequest
            });

            var checkPostReservationRequestResult = ReservationHelper.CheckPostReservationRequestRequireProps(postReservationRequest, _userRole);

            if (!string.IsNullOrEmpty(checkPostReservationRequestResult))
            {
                postReservationResponse = HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: checkPostReservationRequestResult,
                    resultCode: ResultCodes.Error);

                Serilog.Log.Error("{@MobilePostReservationResponseCheckPropsError}", postReservationResponse.ToJson());

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = reservationNumber,
                    Content = JsonConvert.SerializeObject(postReservationResponse),
                    LogType = BrokerLogTypes.ReservationResponse
                });

                return postReservationResponse;
            }

            postReservationRequest.LanguageCode = !string.IsNullOrEmpty(postReservationRequest.LanguageCode) ? postReservationRequest.LanguageCode.TrimNullSafe().ToUpper() : reservationTokenObj.LanguageType.ToString();

            if (postReservationRequest.SpecialDailyPrice <= 0)
            {
                postReservationRequest.SpecialDailyPrice = reservationTokenObj.DailyPrice.Round();
            }

            var vendor = await _vendorService.GetVendorById(reservationTokenObj.VendorId);

            //Kiralanacak aracın müsaitliği ve fiyatı tekrar tedarikçi servisinden sorgulanır
            var getVehiclesRequest = new GetVehiclesRequest
            {
                VendorType = vendor.VendorType,
                ApiKey = vendor.ApiKey,
                ApiPassword = vendor.ApiPassword,
                ApiClientId = vendor.ApiClientId.ToStringNullSafe(),
                ApiLocationCode = reservationTokenObj.APIPickupLocationCode,
                LanguageCode = postReservationRequest.LanguageCode,
                CurrencyCode = reservationTokenObj.CurrencyType.ToString(),
                PickupLocationId = reservationTokenObj.PickupLocationId,
                ReturnLocationId = reservationTokenObj.ReturnLocationId,
                PickupDate = reservationTokenObj.PickupDateTime.ToString("dd.MM.yyyy"),
                ReturnDate = reservationTokenObj.ReturnDateTime.ToString("dd.MM.yyyy"),
                PickupTime = reservationTokenObj.PickupDateTime.ToString("HH:mm"),
                ReturnTime = reservationTokenObj.ReturnDateTime.ToString("HH:mm"),
                SecretKey = vendor.SecretKey,
            };

            Serilog.Log.Error("{@MobileGetVehiclesRequest}", getVehiclesRequest);

            #region Alış tarihi kontolü
            bool checkPickUpDate = ReservationHelper.CheckPickUpDate(reservationTokenObj);
            if (checkPickUpDate)
            {
                var message = await _configurationService.GetLabel(2277, getVehiclesRequest.LanguageCode.ToEnum<LanguageTypes>());
                postReservationResponse = HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.BadRequest,
                    success: false,
                    message: message.Replace("{time}", DateTime.Now.ToString("dd.MM.yyyy HH:mm")),
                    resultCode: ResultCodes.Error);

                Serilog.Log.Error("{@PostReservationResponse}", postReservationResponse);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = reservationNumber,
                    Content = JsonConvert.SerializeObject(postReservationResponse),
                    LogType = BrokerLogTypes.ReservationResponse
                });

                return postReservationResponse;
            }
            #endregion

            var sessionId = _httpContextAccessor?.HttpContext?.Session.GetString("user-code") ?? "";

            var checkVehicleIsAvailable = await CheckVehicleIsAvailable(getVehiclesRequest, reservationTokenObj, sessionId, postReservationRequest, vendor);

            Serilog.Log.Error("{@MobilePostReservationCheckVehicleIsAvailable}", checkVehicleIsAvailable);

            if (checkVehicleIsAvailable.Success)
            {

                if (_userRole == UserRoles.External && postReservationRequest.PaymentType == PaymentTypes.AdvancePayment && postReservationRequest.PaidAmount > 0)
                {
                    postReservationRequest.PaymentType = PaymentTypes.PayOnDelivery;
                    postReservationRequest.AdvancedPaymentWithoutPayment = true;
                }

                Serilog.Log.Error("{ReservationId}", reservationId);
                Serilog.Log.Error("{ReservationNumber:l}", reservationNumber);

                var apiExtras = MapCyrpt(reservationTokenObj.CyrptExtras ?? new List<CyrptExtra>());

                var localResponse = await _reservationService.PostReservationLocalV3(postReservationRequest, reservationId, reservationTokenObj, apiExtras);

                Serilog.Log.Error("{@MobilePostReservationLocalResult}", localResponse);

                if (localResponse.Success)
                {
                    if (HttpContext.RequestAborted.IsCancellationRequested)
                        Serilog.Log.Error("{@MobilePostReservationCancelRequestError}", postReservationRequest.ToJson());

                    string customerMail = postReservationRequest.CustomerEmail.TrimNullSafe().ToLower();

                    var serviceReservation = await _reservationService.PostReservationToVendorAPI(postReservationRequest, reservationId, apiExtras);

                    Serilog.Log.Error("{@MobilePostReservationVendorAPIResult}", serviceReservation);

                    var serviceReservationData = serviceReservation.Data as Reservation;

                    var configurations = await _configurationService.GetConfigurations();

                    if (configurations.AutoCancel && !serviceReservationData.APIReservationSuccessfully)
                    {
                        var postCancelReservationRequest = new PostCancelReservationRequest
                        {
                            ReservationNumber = reservationNumber,
                            CustomerEmail = customerMail,
                            CancelNote = "Otomatik İptal!",
                            LanguageCode = LanguageTypes.TR.ToString(),
                            IsBrokerReservation = false,
                            PenaltyStatus = _penaltyStatus.None,
                            CancelReasonId = 0,
                            UserId = string.Empty,
                            IsKpanelAdmin = true
                        };

                        await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, GetReservationCancelRequestObjectForLog(_userRole, postCancelReservationRequest).ToJson(), BrokerLogTypes.ReservationCancelRequest));

                        var cancelResponse = await _reservationService.CancelReservationLocal(postCancelReservationRequest);

                        await _configurationService.WriteLog(new BrokerLogModel(reservationNumber, "Rezervasyon tedarikçi API iletilemediği için otomatik iptal!", BrokerLogTypes.ReservationCancelVendorAPIRequest));

                        postReservationResponse = HttpResult<object>.Result(
                             data: null,
                             httpResultType: HttpStatusCode.BadGateway,
                             success: false,
                             message: "Rezervasyon tedarikçi API tarafına iletilemedi!",
                             resultCode: ResultCodes.Error);

                        await _configurationService.WriteLog(new BrokerLogModel
                        {
                            LogKey = reservationNumber,
                            Content = JsonConvert.SerializeObject(postReservationResponse),
                            LogType = BrokerLogTypes.ReservationResponse
                        });

                        return postReservationResponse;
                    }

                    await _reservationService.SetVendorLocalContactInformations(serviceReservationData);

                    serviceReservation.Data = _reservationService.UpdateReservationWhenPostReservationToServiceSuccessfully(serviceReservation.Data as Reservation);

                    Serilog.Log.Error("{@MobileUpdatedLocalReservation}", serviceReservation.Data);

                    await _reservationService.SetVendorAddress(serviceReservation.Data as Reservation);

                    var reservationData = serviceReservationData;
                    var resultReservation = _agencyService.ChechAgencyRestricted<RestrictedReservation>(reservationData);

                    postReservationResponse = HttpResult<object>.Result(
                        data: resultReservation,
                        httpResultType: HttpStatusCode.OK,
                        success: localResponse.Data != null,
                        message: localResponse.Message ?? serviceReservation.Message,
                        resultCode: ResultCodes.Success);

                    Serilog.Log.Error("{@MobilePostReservationResponse}", postReservationResponse);

                    await _configurationService.WriteLog(new BrokerLogModel
                    {
                        LogKey = reservationNumber,
                        Content = JsonConvert.SerializeObject(postReservationResponse),
                        LogType = BrokerLogTypes.ReservationResponse
                    });

                    return postReservationResponse;
                }
                else
                {
                    postReservationResponse = HttpResult<object>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.OK,
                    success: false,
                    message: localResponse.Message,
                    resultCode: ResultCodes.InvalidCouponCode);

                    Serilog.Log.Error("{@MobilePostReservationResponse}", postReservationResponse);

                    await _configurationService.WriteLog(new BrokerLogModel
                    {
                        LogKey = reservationNumber,
                        Content = JsonConvert.SerializeObject(postReservationResponse),
                        LogType = BrokerLogTypes.ReservationResponse
                    });

                    return postReservationResponse;
                }
            }
            postReservationResponse = HttpResult<object>.Result(
                data: null,
                httpResultType: HttpStatusCode.OK,
                success: false,
                message: checkVehicleIsAvailable.Message,
                resultCode: (ResultCodes)checkVehicleIsAvailable.ResultCode);

            Serilog.Log.Error("{@MobilePostReservationResponse}", postReservationResponse.ToJson());

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = reservationNumber,
                Content = JsonConvert.SerializeObject(postReservationResponse),
                LogType = BrokerLogTypes.ReservationResponse
            });

            return postReservationResponse;
        }

        private static List<Extra> MapCyrpt(List<CyrptExtra> extras)
        {
            var extraList = new List<Extra>();
            foreach (var cyrtpExtra in extras)
            {
                var extra = new Extra();
                extra.ExtraId = cyrtpExtra.I;
                extra.ExtraCode = cyrtpExtra.C;
                extra.ExtraRentalType = cyrtpExtra.R;
                extra.ExtraType = cyrtpExtra.T;
                extra.Price = cyrtpExtra.P;
                extra.ApiPrice = cyrtpExtra.A;
                extra.ApiExtraCode = cyrtpExtra.AC;
                extra.ExtraName = cyrtpExtra.N;
                extra.VendorExtraExists = cyrtpExtra.V;
                extra.Code = cyrtpExtra.CD;

                extraList.Add(extra);
            }
            return extraList;
        }

        private async Task<HttpResult<object>> CheckVehicleIsAvailable(GetVehiclesRequest getVehiclesRequest, ReservationToken reservationTokenObj, string sessionId, PostReservationRequest postReservationRequest, Domain.Models.Vendor vendor)
        {
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                var vehiclesResponse = await _vehicleService.GetVehicles(
                    getVehiclesRequest,
                    reservationTokenObj.AgencyId.ToIntNullSafe(),
                    sessionId,
                    disableTimeOut: true);

                Serilog.Log.Error("{@MobilePostReservationVehiclesResponse}" + attempt.ToString(), vehiclesResponse);

                var vehicles = vehiclesResponse.Data as List<Vehicle>;

                var result = await _reservationStepsService.CheckReservationVehicleIsAvailable(
                    vehicles,
                    reservationTokenObj,
                    postReservationRequest.LanguageCode.ToEnum<LanguageTypes>(),
                    vendor,
                    postReservationRequest.ReservationToken);

                // Araç müsaitse direkt dön
                if (result.Success)
                    return result;

                Serilog.Log.Warning(
                    "Vehicle not available. Attempt {Attempt}/2. Retrying...",
                    attempt);
            }

            // İkinci denemenin sonucunu döndür
            return await _reservationStepsService.CheckReservationVehicleIsAvailable(
                (await _vehicleService.GetVehicles(
                    getVehiclesRequest,
                    reservationTokenObj.AgencyId.ToIntNullSafe(),
                    sessionId,
                    disableTimeOut: true)).Data as List<Vehicle>,
                reservationTokenObj,
                postReservationRequest.LanguageCode.ToEnum<LanguageTypes>(),
                vendor,
                postReservationRequest.ReservationToken);
        }

        private async Task<PostReservationRequest> GetPostReservationRequest(PostReservationRequestDto postReservationRequestDto)
        {
            var extraList = postReservationRequestDto.Extras != null ? ExtraModelToString(postReservationRequestDto.Extras) : "";

            var agencyId = _agencyService.GetCurrentAgencyId();
            var agencyCode = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData).Value;

            return new PostReservationRequest
            {
                AgencyId = agencyId,
                AgencyCode = agencyCode.TrimNullSafe(),
                ExtraList = extraList,
                ReservationToken = postReservationRequestDto.ReservationToken.TrimNullSafe(),
                CustomerInstutionTypeNumber = 0,
                CustomerName = postReservationRequestDto.CustomerName.TrimNullSafe(),
                CustomerSurname = postReservationRequestDto.CustomerSurname.TrimNullSafe(),
                CustomerTelephone = postReservationRequestDto.CustomerTelephone.TrimNullSafe(),
                CustomerEmail = postReservationRequestDto.CustomerEmail.TrimNullSafe().ToLower(),
                CustomerPersonalNumber = postReservationRequestDto.CustomerPersonalNumber.TrimNullSafe(),
                FlightNumberArrival = postReservationRequestDto.FlightNumberArrival.TrimNullSafe(),
                PaidAmount = postReservationRequestDto.PaidAmount.ToFloatNullSafe(),
                CreditCardPaymentTypeActive = postReservationRequestDto.CreditCardPaymentTypeActive ?? false,
                AdvancePaymentTypeActive = postReservationRequestDto.AdvancePaymentTypeActive ?? false,
                BankId = 0,
                BankVendorId = 0,
                CouponCode = postReservationRequestDto.CouponCode,
                CreditCardHolder = postReservationRequestDto.CreditCardHolder.TrimNullSafe(),
                CreditCardNumber = postReservationRequestDto.CreditCardNumber.TrimNullSafe(),
                ExpiredYear = postReservationRequestDto.ExpiredYear,
                ExpiredMonth = postReservationRequestDto.ExpiredMonth,
                SecurityCode = postReservationRequestDto.SecurityCode.TrimNullSafe(),
                InstallmentCount = postReservationRequestDto.InstallmentCount ?? 0,
                ThreeDPaymentActive = postReservationRequestDto.ThreeDPaymentActive ?? false,
                SpecialDailyPrice = !string.IsNullOrEmpty(postReservationRequestDto.SpecialDailyPrice.ToString()) ? postReservationRequestDto.SpecialDailyPrice.ToString().ToFloatNullSafe() : -1,
                SpecialOneWayFee = !string.IsNullOrEmpty(postReservationRequestDto.SpecialOneWayFee.ToString()) ? postReservationRequestDto.SpecialOneWayFee.ToString().ToFloatNullSafe() : -1,
                IsCommissionFreePrice = postReservationRequestDto.IsCommissionFreePrice ?? false,
                ExtraAmount = postReservationRequestDto.ExtraAmount.ToFloatNullSafe(),
                SendReservationMail = postReservationRequestDto.SendReservationMail,
                CustomerBirthDay = postReservationRequestDto.CustomerBirthDay,
                PaymentType = postReservationRequestDto.PaymentType ?? PaymentTypes.PayToAgency,
                ExtraPricePayToDelivery = postReservationRequestDto.ExtraPricePayToDelivery,
                OneWayFeePayToDelivery = postReservationRequestDto.OneWayFeePayToDelivery,
                IsSpecialWebSiteAgency = postReservationRequestDto.IsSpecialWebSiteAgency,
                CommercialAllowance = postReservationRequestDto.CommercialAllowance,
                AdvancedPaymentWithoutPayment = postReservationRequestDto.AdvancedPaymentWithoutPayment,
                PaidAmountAfterUsingCouponCode = postReservationRequestDto.PaidAmount,
                HighAmountDiscountActive = postReservationRequestDto.HighAmountDiscountActive,
                FullCredit = postReservationRequestDto.FullCredit,
                Bank = postReservationRequestDto.Bank,
                BankAccountCode = postReservationRequestDto.BankAccountCode,
                BankAccountingCode = postReservationRequestDto.BankAccountingCode,
                Country = postReservationRequestDto.InvoiceData?.Country ?? "",
                City = postReservationRequestDto.InvoiceData?.City ?? "",
                District = postReservationRequestDto.InvoiceData?.District ?? "",
                CustomerAddress = postReservationRequestDto.InvoiceData?.Address ?? "",
                CompanyTitle = postReservationRequestDto.InvoiceData?.Title ?? "",
                CompanyTaxOffice = postReservationRequestDto.InvoiceData?.TaxOffice ?? "",
                CompanyTaxNumber = postReservationRequestDto.InvoiceData?.TaxNumber ?? "",
                CreditCardBank = postReservationRequestDto.CreditCardBank,
                ProvisionNumber = "",
                OrderNumber = postReservationRequestDto.OrderNumber,
                ContactPermission = postReservationRequestDto.ContactPermission,
                CustomerNote = postReservationRequestDto.CustomerNote ?? "",
                RequestId = postReservationRequestDto.RequestId,
                InstallmentFee = postReservationRequestDto.InstallmentFee ?? 0,
                CouponDiscountAmount = postReservationRequestDto.CouponDiscountAmount,
                CouponDiscountType = postReservationRequestDto.CouponDiscountType,
                CouponDiscountValue = postReservationRequestDto.CouponDiscountValue,
                UserAgent = postReservationRequestDto.UserAgent,
                CustomerIPAddress = postReservationRequestDto.IpAddress,
                InstallmentCommissionAmount = postReservationRequestDto.InstallmentCommissionAmount ?? 0,
                MarkupAmount = postReservationRequestDto.MarkupAmount,
                HasRefundableCoupon = postReservationRequestDto.HasRefundableCoupon,
                CouponBonusAmount = postReservationRequestDto.CouponBonusAmount
            };
        }

        private string ExtraModelToString(List<ExtraMobileSuccess> extraList)
        {
            var extras = string.Join('|', extraList.Select(e => $"{e.ExtraId}~1~{e.Price}").ToList());

            return extras;
        }

        private object GetReservationRequestObjectForLog(UserRoles userRole, PostReservationRequest postReservationRequest)
        {
            switch (userRole)
            {
                case UserRoles.External:
                    {
                        return new
                        {
                            postReservationRequest.ReservationToken,
                            postReservationRequest.ExtraList,
                            postReservationRequest.CustomerName,
                            postReservationRequest.CustomerSurname,
                            postReservationRequest.CustomerTelephone,
                            postReservationRequest.CustomerEmail,
                            postReservationRequest.CustomerBirthDay,
                            postReservationRequest.CustomerPersonalNumber,
                            postReservationRequest.CustomerNote,
                            postReservationRequest.CustomerAddress,
                            postReservationRequest.FlightNumberArrival,
                            postReservationRequest.FlightNumberDeparture,
                            postReservationRequest.CustomerIPAddress,
                            postReservationRequest.PaidAmount,
                            postReservationRequest.ExtraAmount,
                            postReservationRequest.SendReservationMail,
                            postReservationRequest.DepartureInfo,
                            postReservationRequest.AgencyReservationReference,
                            postReservationRequest.ExtraPricePayToDelivery,
                            postReservationRequest.OneWayFeePayToDelivery,
                            postReservationRequest.PaymentType,
                            postReservationRequest.FullCredit
                        };
                    }
                default: return StringHelper.MaskCreditCard(JsonConvert.DeserializeObject<PostReservationRequest>(JsonConvert.SerializeObject(postReservationRequest)));
            }
        }

        #endregion

        #endregion

        #region ReservationCancel

        [HttpPost]
        [Route("CancelLocal")]
        public async Task<IActionResult> CancelLocal(CancelLocalRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ReservationNumber) || string.IsNullOrEmpty(request.CustomerEmail))
                    return BadRequest("Invalid request");

                var user = await _memoryCacheService.GetUserByEmail("info@obilet.com");

                if ((user?.Kullaniciid ?? 0) <= 0)
                    return BadRequest("Invalid request");

                request.UserId = user.Kullaniciid;
                request.CancelNote = "Obilet Admin Local Cancellation";

                var result = await _reservationService.LocalCancel(request);

                return Ok(new
                {
                    data = result,
                    resultCode = ResultCodes.Success,
                    success = result,
                    message = "Successful"
                });
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    data = false,
                    resultCode = ResultCodes.Error,
                    success = false,
                    message = e.Message
                });
            }

        }

        #endregion

        #region ReservationUpdate

        [HttpPost]
        [Route("UpdateRecalculatedReservation")]
        public async Task<IActionResult> UpdateRecalculatedReservation(UpdateRecalculatedReservationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ReservationCode))
                    return BadRequest("Invalid request");

                if (request.NewReturnDate == default || request.RentalDayCount <= 0)
                    return BadRequest("Invalid request");

                var result = await _reservationService.UpdateRecalculatedReservation(request);

                return Ok(new
                {
                    data = new
                    {
                        success = result,
                        message = result ? "Successful" : "Reservation could not be updated"
                    },
                    resultCode = result ? ResultCodes.Success : ResultCodes.Error,
                    success = result,
                    message = result ? "Successful" : "Reservation could not be updated"
                });
            }
            catch (Exception e)
            {
                Serilog.Log.Error("{@UpdateRecalculatedReservationError}", $"{e.Message}-{e.StackTrace}-{e.InnerException?.Message}");

                return Ok(new
                {
                    data = new
                    {
                        success = false,
                        message = "Reservation could not be updated"
                    },
                    resultCode = ResultCodes.Error,
                    success = false,
                    message = e.Message
                });
            }
        }

        #endregion

        #region Findeks Methods

        [HttpPost]
        [Route("FindeksCheck")]
        public async Task<IActionResult> FindeksCheck(FindeksCheck findeksDto)
        {
            if (string.IsNullOrEmpty(findeksDto.Tckn) || findeksDto.VendorId <= 0) return null;

            var isActiveReport = await IsActiveFindeksReportExists(new GetIsActiveFindeksReportRequest
            {
                ReservationToken = findeksDto.ReservationToken,
                Tckn = findeksDto.Tckn,
                VendorId = findeksDto.VendorId
            });

            //Active Report Varsa
            if (isActiveReport.Data != null)
            {
                var languageId = await _memoryCacheService.GetLanguageId(findeksDto.LanguageCode);
                var alllabels = await _memoryCacheService.GetLabels(languageId);
                var labels = alllabels.Where(l => l.Dilid == languageId).ToList();

                var data = JsonConvert.SerializeObject(isActiveReport.Data);
                var convertedData = JsonConvert.DeserializeObject<KolayCAR.Broker.Domain.Models.Response.AvisResponseBase.FindeksReportExist>(data);

                if (convertedData.CompanyDecision)
                {
                    var isSuitable = await IsVehicleSuitableForCustomer(new IsVehicleSuitableForCustomer
                    {
                        Tckn = findeksDto.Tckn,
                        ReservationToken = findeksDto.ReservationToken,
                        FindeksReportExist = convertedData,
                    });

                    if (isSuitable.Data != null)
                    {
                        var isSuitableData = JsonConvert.SerializeObject(isSuitable.Data);
                        if (data != "0" && !Int32.TryParse(data, out _))
                        {
                            var entity = JsonConvert.DeserializeObject<KolayCAR.Broker.Domain.Models.Response.AvisResponseBase.IsVehicleSuitableForCustomerResponse>(isSuitableData);

                            if (entity.IsSuitable && !entity.IsRequiredYoungDriverPacked)
                            {
                                return Ok(new
                                {
                                    Data = new FindeksCheckResponseDto
                                    {
                                        Url = "",
                                        Description = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.CheckFindeks.VehicleSuitable")?.Labeladi,
                                        Decision = true
                                    },
                                    Success = true
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    Data = new FindeksCheckResponseDto
                    {
                        Url = "",
                        Description = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.CheckFindeks.VehicleNotSuitable")?.Labeladi,
                        Decision = false
                    },
                    Success = true
                });
            }

            //Active Report Yoksa
            var podomain = _parameterService.GetParameterValue("PODOMAIN");
            Guid uniqueId = Guid.NewGuid();

            var result = await _mobileService.AddFindeksMobileUrl(new MobileFindeksUrl
            {
                BirthDate = DateTime.Parse(findeksDto.BirthDate),
                DriverLicenseDate = DateTime.Parse(findeksDto.DriverLicenseDate),
                Tckn = findeksDto.Tckn,
                ReservationToken = findeksDto.ReservationToken,
                UniqueId = uniqueId.ToString(),
                VendorId = findeksDto.VendorId
            });

            if (!result.Item1)
            {
                return Ok(new
                {
                    Data = new FindeksCheckResponseDto
                    {
                        Url = "",
                        Description = result.Item2,
                        Decision = false
                    },
                    Success = false,
                    Message = result.Item2
                });
            }

            return Ok(new
            {
                Data = new FindeksCheckResponseDto
                {
                    Url = podomain + $"/{findeksDto.LanguageCode}/MobileFindeks/{uniqueId}",
                    RedirectionUrl = podomain + $"/{findeksDto.LanguageCode}/MobileFindeksSuccess"
                },
                Success = true
            });
        }

        [HttpPost]
        [Route("FindeksCheckIsSuccess")]
        public async Task<IActionResult> FindeksCheckIsSuccess(FindeksIsSuccessRequestDto findeksIsSuccess)
        {
            if (string.IsNullOrEmpty(findeksIsSuccess.ResToken) || string.IsNullOrEmpty(findeksIsSuccess.Tckn)) return Ok(new { Success = false, Message = "Missing required Findeks information. Please ensure both ResToken and TCKN are provided." });

            var languageId = await _memoryCacheService.GetLanguageId(findeksIsSuccess.LanguageCode);
            var alllabels = await _memoryCacheService.GetLabels(languageId);
            var labels = alllabels.Where(l => l.Dilid == languageId).ToList();

            var details = await _mobileService.GetReservationFindeksDetails(findeksIsSuccess.ResToken, findeksIsSuccess.Tckn);

            var detail = details?.LastOrDefault(d => d.StepName == FindeksSteps.isVehicleSuitableForCustomer);
            if (detail != null)
            {
                if (detail.isSuitableForCustomer && !detail.isRequiredYoungDriverPacked)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.CheckFindeks.VehicleSuitable")?.Labeladi
                    });
                }
            }

            return Ok(new
            {
                Success = false,
                Message = labels.FirstOrDefault(l => l.LabelKodu == "Mobile.CheckFindeks.VehicleNotSuitable")?.Labeladi
            });
        }

        [HttpPost]
        public async Task<ServiceResponseBase> IsActiveFindeksReportExists(GetIsActiveFindeksReportRequest findeksDto)
        {
            if (string.IsNullOrEmpty(findeksDto.Tckn) || findeksDto.VendorId <= 0) return null;

            var result = await _findeksService.GetIsActiveFindeksReportExists(findeksDto);

            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseBase> GetPhoneList(GetPhoneIdListRequest findeksDto)
        {
            if (string.IsNullOrEmpty(findeksDto.Tckn) || findeksDto.VendorId <= 0) return null;

            var result = await _findeksService.GetFindeksPhoneIdList(findeksDto);

            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseBase> ReportRequest(GetFindeksReportRequest findeksDto)
        {
            if (string.IsNullOrEmpty(findeksDto.Tckn) || findeksDto.VendorId <= 0 || string.IsNullOrEmpty(findeksDto.PhoneNo)) return null;

            var result = await _findeksService.GetFindeksReport(findeksDto);

            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseBase> PinRenew(CreatePinRenewRequest findeksDto)
        {
            if (findeksDto.VendorId <= 0) return null;

            var result = await _findeksService.FindeksPinRenew(findeksDto);

            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseBase> PinConfirm(ConfirmPinRequest findeksDto)
        {
            if (findeksDto.VendorId <= 0) return null;

            var result = await _findeksService.FindeksPinConfirm(findeksDto);

            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseBase> IsVehicleSuitableForCustomer(IsVehicleSuitableForCustomer findeksDto)
        {
            if (findeksDto == null) return null;

            var result = await _findeksService.IsVehicleSuitableForCustomer(findeksDto);

            return result;
        }

        #endregion

        #region Get Languages Action

        [HttpGet]
        [Route("GetLanguages")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _languageService.GetActiveLanguages();
            return Ok(new
            {
                data = languages,
                resultCode = ResultCodes.Success,
                success = true
            });
        }

        #endregion

        #region Get SearchedLocation Action

        [HttpPost]
        [Route("GetSearchedLocations")]
        public async Task<ActionResult> GetSearchedLocations(GetSearchedLocationsRequest getSearchedLocations)
        {
            var languageId = await _memoryCacheService.GetLanguageId(getSearchedLocations.LanguageCode);

            var allLocations = await _memoryCacheService.GetAllSearchLocations(_agencyService.GetCurrentAgencyId());
            var languageLocations = allLocations.Where(l => l.LanguageId == languageId);
            var activeLocations =
                getSearchedLocations.IsPickup ?
                languageLocations.Where(l => l.IsPickup) :
                languageLocations;

            if (!string.IsNullOrEmpty(getSearchedLocations.SearchText))
            {
                // Aramaya göre dönüş yapılıyor
                var searchKeys = getSearchedLocations.SearchText.ToLower().ToEnglishLetters().Split();
                IEnumerable<SearchLocationDto> searchLocations = Enumerable.Empty<SearchLocationDto>();
                foreach (var searchKey in searchKeys)
                {
                    searchLocations =
                        !searchLocations.Any() ?
                            activeLocations.Where(l => l.LocationName.ToLower().ToEnglishLetters().Contains(searchKey)).ToList() :
                            searchLocations.Where(l => l.LocationName.ToLower().ToEnglishLetters().Contains(searchKey)).ToList();
                }
                var result = searchLocations
                                //.Where(l => l.LocationName.ToEnglishLetters().ToLower().Contains(searchText))
                                .OrderByDescending(l => l.IsAirport)
                                .ThenBy(l => l.LocationName)
                                .Select(l => new
                                {
                                    cityName = l.CityName,
                                    locationId = l.LocationId,
                                    locationName = l.LocationName,
                                    isPopular = l.IsPopular,
                                    getSearchedLocations.IsPickup
                                })

                                .ToList();
                return Ok(new
                {
                    data = result,
                    resultCode = ResultCodes.Success,
                    success = true
                });
            }
            else
            {
                // Popüler Lokasyonlar Dönülüyor
                var result = activeLocations
                                .Where(l => l.IsPopular)
                                .OrderByDescending(l => l.IsAirport)
                                .ThenBy(l => l.LocationName)
                                .Select(l => new
                                {
                                    cityName = l.CityName,
                                    locationId = l.LocationId,
                                    locationName = l.LocationName,
                                    isPopular = l.IsPopular,
                                    getSearchedLocations.IsPickup
                                }).ToList();
                return Ok(new
                {
                    data = result,
                    resultCode = ResultCodes.Success,
                    success = true
                });
            }
        }

        #endregion

        #region Get Currencies Action

        [HttpGet]
        [Route("GetAllCurrencies")]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var currencies = await _currencyService.GetAllCurrencies();
            var currencyDtoList = new List<GetAllCurrenciesDto>();

            foreach (var currency in currencies)
            {
                currencyDtoList.Add(new GetAllCurrenciesDto
                {
                    CurrencyId = currency.Currencyid,
                    CurrencyName = currency.Currencyname,
                    CurrencyIsoCode = currency.Currencyisocode,
                    Symbol = currency.Symbol,
                    Active = currency.Active
                });
            }

            return Ok(new
            {
                data = currencyDtoList,
                resultCode = ResultCodes.Success,
                success = true
            });
        }

        #endregion

        #region Get ExchangeRates Action

        [HttpGet]
        [Route("GetExchangeRates")]
        public async Task<IActionResult> GetExchangeRates()
        {
            var exchanges = await _exchangeRateService.GetAllExchangeRates();
            return Ok(new
            {
                data = exchanges,
                resultCode = ResultCodes.Success,
                success = true
            });
        }

        #endregion

        #region GetVehicleClasses Action

        [HttpPost]
        [Route("GetVehicleClasses")]
        public async Task<IActionResult> GetVehicleClasses()
        {
            var vehicleClasses = await _vehicleService.GetAllVehicleClasses();

            var model = vehicleClasses?.Select(v => new GetVehicleClassesResponse
            {
                Vehicleclassid = v.Vehicleclassid,
                Active = v.Active,
                Categoryid = v.Categoryid,
                Typeid = v.Typeid,
                Personid = v.Personid,
                Baggageid = v.Baggageid,
                Transmissionid = v.Transmissionid,
                Fuelid = v.Fuelid,
                ModelName = v.VehicleModel?.Modelname,
                BrandName = v.VehicleBrand?.Brandname,
            });

            return Ok(new
            {
                data = model,
                resultCode = ResultCodes.Success,
                success = true
            });
        }


        #endregion

        #region GetAllVendors Action

        [HttpGet]
        [Route("GetAllVendors")]
        public async Task<IActionResult> GetAllVendors()
        {
            var vendors = await _vendorService.GetVendors();

            var model = vendors?.Select(v => new GetAllVendorsResponse
            {
                Vendorid = v.Vendorid,
                Vendortype = v.Vendortype,
                Vendorname = v.Vendorname,
                Active = v.Active,
                Profitmarkup = v.Profitmarkup,
                Rentalworkingtype = v.Rentalworkingtype,
                FoundationYear = v.FoundationYear,
            });

            return Ok(new
            {
                data = model,
                resultCode = ResultCodes.Success,
                success = true
            });
        }


        #endregion

        private object GetReservationCancelRequestObjectForLog(UserRoles userRole, dynamic postCancelReservationRequest)
        {
            switch (userRole)
            {
                case UserRoles.External:
                    {
                        return new
                        {
                            postCancelReservationRequest.ReservationNumber,
                            postCancelReservationRequest.CustomerEmail,
                            postCancelReservationRequest.CancelNote
                        };
                    }
                default: return postCancelReservationRequest;
            }
        }
    }
}
