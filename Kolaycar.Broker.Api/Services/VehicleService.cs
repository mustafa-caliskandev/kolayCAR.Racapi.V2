using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IVehicleService
    {
        Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehicleRequest, int agencyId, string sessionId, bool disableTimeOut = false);
        Task<ServiceResponseBase> GetVehiclesWihtBulkRequest(GetVehiclesRequest getVehicleRequest, int agencyId, string sessionId, bool disableTimeOut = false);
        Task<ServiceResponseBase> GetRentalConditions(int vendorId, LanguageTypes languageType);
        Task<string> GetFuelName(LanguageTypes languageType, FuelTypes fuelType);
        string GetFuelName(List<Vehiclefuellang> vehiclefuellangs, LanguageTypes languageType, FuelTypes fuelType);
        Task<List<Vehiclefuel>> GetFuelIconPath();
        Task<string> GetTransmissionName(LanguageTypes languageType, TransmissionTypes transmissionType);
        string GetTransmissionName(List<Vehicletransmissionlang> vehicletransmissionlangs, LanguageTypes languageType, TransmissionTypes transmissionType);
        Task<List<Vehicletransmission>> GetTransmissionIconPath();
        Task<string> GetVehicleTypeName(LanguageTypes languageType, VehicleTypes vehicleType);
        string GetVehicleTypeName(List<Vehicletypelang> vehicletypelangs, LanguageTypes languageType, VehicleTypes vehicleType);
        Task<List<Vehicletype>> GetVehicleTypeIconPath();
        Task<string> GetVehicleCategoryTypeName(LanguageTypes languageType, VehicleCategoryTypes vehicleCategoryType);
        string GetVehicleCategoryTypeName(List<Vehiclecategorylang> vehiclecategorylangs, LanguageTypes languageType, VehicleCategoryTypes vehicleCategoryType);
        Task<List<Vehiclecategory>> GetVehicleCategoryIconPath();
        string GetVehicleCategoryImagePath(LanguageTypes languageType, VehicleCategoryTypes vehicleCategoryType);
        Task<string> GetPassangerQuantityTypeName(LanguageTypes languageType, PassangerQuantityTypes passangerQuantityType);
        string GetPassangerQuantityTypeName(List<Vehiclepersonlang> vehiclepersonlangs, LanguageTypes languageType, PassangerQuantityTypes passangerQuantityType);
        Task<List<Vehicleperson>> GetPassangerQuantityTypeIconPath();
        Task<string> GetBaggageQuantityTypeName(LanguageTypes languageType, BaggageQuantityTypes baggageQuantityType);
        string GetBaggageQuantityTypeName(List<Vehiclebaggagelang> vehiclebaggagelangs, LanguageTypes languageType, BaggageQuantityTypes baggageQuantityType);
        Task<List<Vehiclebaggage>> GetBaggageQuantityTypeIconPath();
        Task<ServiceResponseBase> GetVehicleClassListFromVendorAPI(int vendorId);
        Task<List<Vehicle>> GetLocalVehicleClassesByVendorId(int vendorId, LanguageTypes languageType);
        Task<List<Vehicleclass>> GetAllVehicleClasses();
        Task<List<VehicleListItem>> GetLocalVehiclesList(LanguageTypes languageType, bool getOnlyMatched = false);
        Task<string> GetVehicleDescription(string vehicleCode, LanguageTypes languageType);
        Task<List<Filter>> GetVehicleFilters(List<Domain.Models.VehicleModel> vehicles);
        Task<List<FastFilter>> GetVehicleFastFilters(List<Domain.Models.VehicleModel> vehicles);
        Task<List<OrderOption>> GetVehicleOrderOptions(int languageId);
        Task<List<FeaturesSorting>> GetVehicleDetails();
        Task<List<FeaturesSorting>> GetVehicleFeatures();
        Task<List<MobileAppVehicleBadge>> GetMobileAppVehicleBadges();
        Task<List<SortableParam>> GetVehicleSortableParams();
        Task<DeliveryTypeLanguage> GetDeliveryTypeById(int languageId, int deliveryTypeId);
        Task<List<DeliveryTypeLanguage>> GetDeliveryTypes(int languageId);
        Task<List<Vehiclecategorylang>> GetVehicleCategoryLangList(int languageId);
    }

    public class VehicleService : IVehicleService
    {
        private readonly AppSettings _appSettings;
        private readonly BrokerContext _context;
        private readonly IVendorService _vendorService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IConfigurationService _configurationService;
        private readonly IDBHelper _dbHelper;
        private readonly IAgencyService _agencyService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILabelService _labelService;
        private readonly ILocationService _locationService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ICacheService _cacheService;
        private readonly IProfitMarkupService _profitMarkupService;
        private readonly ILocationVendorClosedDateService _locationVendorClosedDateService;
        private readonly ILocationVendorService _locationVendorService;
        private readonly ISubVendorService _subVendorService;
        private readonly ICityService _cityService;
        private readonly IRentalConditionService _rentalConditionService;
        private readonly IResTokenService _resTokenService;
        private readonly IAgencyLocationService _agencyLocationService;
        private readonly ICurrencyService _currencyService;
        private readonly IVehicleProviderFactory _vehicleProviderFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IVendorVendorService _vendorVendorService;

        public VehicleService(IOptions<AppSettings> appSettings, BrokerContext context, IConfigurationService configurationService, IAgencyService agencyService, IVendorService vendorService, IReservationStepsService reservationStepsService, IDBHelper dbHelper, IMemoryCache memoryCache, ILabelService labelService, ILocationService locationService, IExchangeRateService exchangeRateService, ICacheService cacheService, IConfiguration configuration, IProfitMarkupService profitMarkupService, ILocationVendorClosedDateService locationVendorClosedDateService, ISubVendorService subVendorService, ILocationVendorService locationVendorService, ICityService cityService, IRentalConditionService rentalConditionService, IResTokenService resTokenService, IAgencyLocationService agencyLocationService, IVehicleProviderFactory vehicleProviderFactory, IServiceScopeFactory serviceScopeFactory, ICurrencyService currencyService, IVendorVendorService vendorVendorService)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _vendorService = vendorService;
            _reservationStepsService = reservationStepsService;
            _configurationService = configurationService;
            _agencyService = agencyService;
            _dbHelper = dbHelper;
            _memoryCache = memoryCache;
            _labelService = labelService;
            _locationService = locationService;
            _exchangeRateService = exchangeRateService;
            _cacheService = cacheService;
            _profitMarkupService = profitMarkupService;
            _locationVendorClosedDateService = locationVendorClosedDateService;
            _subVendorService = subVendorService;
            _locationVendorService = locationVendorService;
            _cityService = cityService;
            _rentalConditionService = rentalConditionService;
            _resTokenService = resTokenService;
            _agencyLocationService = agencyLocationService;
            _vehicleProviderFactory = vehicleProviderFactory;
            _serviceScopeFactory = serviceScopeFactory;
            _currencyService = currencyService;
            _vendorVendorService = vendorVendorService;
        }
        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehicleRequest, int agencyId, string sessionId, bool disableTimeOut = false)
        {
            var languageType = getVehicleRequest.LanguageCode.ToEnum<LanguageTypes>();

            if (string.IsNullOrWhiteSpace(getVehicleRequest.ApiKey))
                return new(null, false, await _configurationService.GetLabel(1959, languageType));

            try
            {
                if (CacheSettings.UseCache)
                    await _cacheService.ValidateCacheAsync();

                var configurations = await _configurationService.GetConfigurations();

                var pickupDateTime = ReservationHelper.GetDateTimeToDateAndTimeStrings(getVehicleRequest.PickupDate, getVehicleRequest.PickupTime);
                var returnDateTime = ReservationHelper.GetDateTimeToDateAndTimeStrings(getVehicleRequest.ReturnDate, getVehicleRequest.ReturnTime);

                if (!(pickupDateTime >= DateTime.Now.AddHours(configurations.NearestRentalTime) || configurations.NearestRentalTime == 0))
                {
                    var pickupDateErrorMessageTemplate = await _configurationService.GetLabel(2275, languageType);
                    return new(null, false, pickupDateErrorMessageTemplate.Replace("{date}",
                        $"{DateTime.Now.AddHours(configurations.NearestRentalTime).ToShortDateString()} {DateTime.Now.AddHours(configurations.NearestRentalTime).ToShortTimeString()}"));
                }

                var agency = await _agencyService.GetAgency(agencyId);
                if (agency == null)
                    return new(null, false, await _configurationService.GetLabel(2282, languageType));

                var vendor = await _vendorService.GetVendorAsync(getVehicleRequest.ApiKey, getVehicleRequest.ApiPassword, getVehicleRequest.ApiClientId, getVehicleRequest.SecretKey, getVehicleRequest.VendorType, agency);

                if (vendor == null)
                    return new(null, false, await _configurationService.GetLabel(1959, languageType));

                var currencyType = getVehicleRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                var exchangeRates = await _exchangeRateService.GetAllExchangeRates();
                var currency = await _currencyService.GetCurrencyByCode(getVehicleRequest.CurrencyCode);

                var exchangeRate = exchangeRates.FirstOrDefault(e => e.Currencyid == currency.Currencyid)?.Exchangerate ?? 0;

                bool isCurrencyActive = ((bool)currency.Active || currency.InternationalActive);

                if (!isCurrencyActive || exchangeRate == 0)
                    return new(null, false, await _configurationService.GetLabel(1878, languageType));

                var baseVendorRequestCurrencyType = vendor.CurrencyType;

                if (vendor.UseOnlyDefaultCurrency)
                {
                    if (vendor.AvailableCurrencies.Contains(currencyType))
                    {
                        baseVendorRequestCurrencyType = vendor.CurrencyType;
                    }
                    else if (vendor.CurrencyType != currencyType)
                        return new(null, false, await _configurationService.GetLabel(1878, languageType));

                }
                else
                {
                    if (vendor.AvailableCurrencies.Contains(currencyType))
                    {
                        baseVendorRequestCurrencyType = currencyType;
                    }
                    else
                    {
                        return new(null, false, await _configurationService.GetLabel(1878, languageType));
                    }
                }


                var isVendorClosed = await _locationVendorClosedDateService.CheckVendorIsClosedCurrentDate(vendor.VendorId, getVehicleRequest.PickupLocationId, pickupDateTime);

                if (isVendorClosed)
                    return new(null, false, await _configurationService.GetLabel(1879, languageType));

                await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                var subVendorList = await _subVendorService.GetAllAsync();
                var subVendors = subVendorList.Map();
                var vendorLocation = await _locationVendorService.GetLocationVendor(getVehicleRequest.PickupLocationId, vendor.VendorId);

                if (vendorLocation == null)
                    return new(null, false, await _configurationService.GetLabel(1659, languageType));

                var minuteDiff = (ObjectHelper.CombineDateAndTime(getVehicleRequest.PickupDate, getVehicleRequest.PickupTime) - DateTime.Now).TotalMinutes;

                var earliestTime = vendorLocation?.EarliestResTime ?? vendor?.EarliestResTime;

                if (earliestTime != null && earliestTime >= minuteDiff)
                    return new(null, false, $"En erken {earliestTime} dakika sonrasına rezervasyon yapılabilir!");

                var vendorScore = new VendorScoreDto();

                if (configurations.VendorScoreActive)
                {
                    List<VendorScoreDto> vendorScoreDtos;

                    if (CacheSettings.UseCache)
                        vendorScoreDtos = await _cacheService.GetOrCreateAsync($"GETVENDORSCOREBYLOCATIONID-{(int)languageType}-{vendor.VendorId}-{getVehicleRequest.PickupLocationId}", () => _context.VendorScoresDto.FromSqlRaw("EXEC GETVENDORSCOREBYLOCATIONID @languageId = {0}, @vendorId = {1}, @locationId = {2}", (int)languageType, vendor.VendorId, getVehicleRequest.PickupLocationId).ToListAsync());
                    else
                        vendorScoreDtos = await _context.VendorScoresDto.FromSqlRaw("EXEC GETVENDORSCOREBYLOCATIONID @languageId = {0}, @vendorId = {1}, @locationId = {2}", (int)languageType, vendor.VendorId, getVehicleRequest.PickupLocationId).ToListAsync();

                    vendorScore = vendorScoreDtos?.FirstOrDefault();
                }

                var agencyLocation = await _agencyLocationService.GetAgencyLocationsByAgencyId(agency.AgencyId);
                var pickupLocation = await _locationService.GetPickupLocation(getVehicleRequest.PickupLocationId, agencyLocation);

                if (pickupLocation == null)
                    return new(null, false, await _configurationService.GetLabel(1881, languageType));

                var returnLocation = await _locationService.GetLocationById(getVehicleRequest.ReturnLocationId);
                var cityOfPickupLocation = await _cityService.GetCityById(pickupLocation.Cityid);
                var cityOfReturnLocation = returnLocation != null ? await _cityService.GetCityById(returnLocation.Cityid) : null;

                var additionalInformation = await _reservationStepsService.GetAdditionalInformation(
                    vendor,
                    agency,
                    getVehicleRequest.LanguageCode,
                    getVehicleRequest.CurrencyCode,
                    vendor.CurrencyType,
                    getVehicleRequest.PickupLocationId,
                    getVehicleRequest.ReturnLocationId,
                    getVehicleRequest.PickupDate,
                    getVehicleRequest.ReturnDate,
                    getVehicleRequest.PickupTime,
                    getVehicleRequest.ReturnTime,
                    apiLocationCode: getVehicleRequest.ApiLocationCode
                    );

                if (additionalInformation == null)
                    return new(null, false, await _configurationService.GetLabel(1880, languageType));

                Serilog.Log
                        .ForContext("AgencyId", agency.AgencyId)
                        .ForContext("PickupLocationId", pickupLocation.Id)
                        .ForContext("ReturnLocation", returnLocation.Id)
                        .Debug("{PickupLocationName}-{ReturnLocationName}, Agency: {AgencyName}", pickupLocation.Locationname, returnLocation.Locationname, agency.AgencyName);

                var localVehicles = await GetLocalVehicleClassesByVendorId(vendor.VendorId, languageType);
                var mappedExchangeRates = exchangeRates.Map();
                var profitMarkupFilter = await _profitMarkupService.GetProfitMarkupFilter(vendor.VendorId, agency.AgencyId, getVehicleRequest.PickupLocationId, pickupDateTime, returnDateTime, additionalInformation.RentalDuration);

                additionalInformation.Vendor.ApiKey = EncryptionHelper.Decrypt(additionalInformation.Vendor.ApiKey);
                additionalInformation.Vendor.ApiPassword = EncryptionHelper.Decrypt(additionalInformation.Vendor.ApiPassword);

                var vehicleProvider = _vehicleProviderFactory.CreateVehicleProvider(vendor, _memoryCache, _cacheService, disableTimeOut);

                if (vehicleProvider is null)
                    return new(null, false, await _configurationService.GetLabel(1882, languageType));

                try
                {
                    var apiVehicles = await vehicleProvider.GetVehicles(getVehicleRequest, vendor, additionalInformation, mappedExchangeRates, localVehicles, subVendors, baseVendorRequestCurrencyType, profitMarkupFilter);


                    if (apiVehicles == null)
                    {
                        if (configurations.TimeoutLog)
                            Serilog.Log.Fatal("{@Timeout}", vendor.VendorName);
                        return new(null, false, await _configurationService.GetLabel(2271, languageType), serviceCode: ResultCodes.Timeout.ToString());
                    }

                    if (!apiVehicles.Success || apiVehicles.Data == null)
                        return new(null, false, await _configurationService.GetLabel(847, languageType));


                    List<RentalCondition> rentalConditionsData = new List<RentalCondition>();
                    if (vendor.VendorType != VendorTypes.KolayCARBroker ||
                        (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations))
                    {
                        var rentalConditionsResult = await _rentalConditionService.GetRentalConditions(vendor.VendorId, languageType);
                        rentalConditionsData = rentalConditionsResult.Success ? rentalConditionsResult.Data as List<RentalCondition> : null;
                    }

                    var apiVehiclesData = apiVehicles.Data as List<Vehicle>;

                    if (configurations.OnlyAvailableVehicles)
                        apiVehiclesData = apiVehiclesData.Where(x => x.IsAvailable).ToList();

                    apiVehiclesData = apiVehiclesData.OrderBy(x => x.TotalPricePayNow).ToList();

                    if (vendor.VendorType != VendorTypes.KolayCARBroker ||
                        (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations))
                        await SetPriority(apiVehiclesData, vendor.VendorId, pickupLocation.Id, pickupDateTime);

                    var vehicleTokenList = new List<KeyValuePair<string, string>>();
                    var resTokenList = new List<Restoken>();
                    using var scope = _serviceScopeFactory.CreateScope();
                    var _extraService = scope.ServiceProvider.GetRequiredService<IExtraService>();

                    foreach (var vehicle in apiVehiclesData)
                    {
                        CreditHelper.ApplyEffectiveCreditType(vendor, agency, vehicle);

                        var guid = Guid.NewGuid().ToString();
                        resTokenList.Add(new Restoken
                        {
                            SessionId = sessionId,
                            Uniqueid = guid,
                            Token = CompressString(vehicle.ReservationToken)
                        });

                        vehicle.PickupLocationCode = additionalInformation.APIPickupLocationCode;
                        vehicle.ReturnLocationCode = additionalInformation.APIReturnLocationCode;

                        vehicle.RentalConditions = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? rentalConditionsData : vehicle.RentalConditions;
                        vehicle.IsOffice = (vendor.VendorType == VendorTypes.Yolcu360 || vendor.VendorType == VendorTypes.Yolcu360v2) ? vehicle.IsOffice : vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? vendorLocation.Isoffice ?? false : vehicle.IsOffice;
                        vehicle.IsAirport = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? pickupLocation.Airport ?? false : vehicle.IsAirport;
                        vehicle.VendorLogo = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? (vendor.VendorType != VendorTypes.Yolcu360 && vendor.VendorType != VendorTypes.EnUygun) ? GetAbsoluteUrl(vehicle.VendorLogo, configurations.PortalOwnerDomain) : vehicle.VendorLogo : vehicle.VendorLogo;
                        vehicle.CurrencyCode = getVehicleRequest.CurrencyCode;
                        VehicleHelper.SetVehiclePropertyBySIPPCode(vehicle);
                        _agencyService.SetVehiclePaymentOptions(vehicle, agency);
                        vehicle.VendorCouponUsing = vendor.CouponCodeActive.ToBoolNullSafe();
                        vehicle.CityOfPickupLocation = cityOfPickupLocation?.Cityname.ToStringNullSafe();
                        vehicle.CityOfReturnLocation = cityOfReturnLocation?.Cityname.ToStringNullSafe();

                        vehicle.VendorScore = vendorScore != null ? vendorScore.CurrentScore.ToFloatNullSafe() != 0 && vendorScore.CurrentScore != -1 ? vendorScore.CurrentScore.ToFloatNullSafe() : vendorScore.Score.ToFloatNullSafe() != 0 ? vendorScore.Score.ToFloatNullSafe() : 4 : 4;
                        vehicle.VendorCommentCount = vendorScore != null ? vendorScore.CommentCount.ToIntNullSafe() : 0;

                        vehicleTokenList.Add(new KeyValuePair<string, string>
                        (
                            key: $"{vehicle.VendorId}-{vehicle.VehicleCode}",
                            value: vehicle.ReservationToken
                        ));

                        vehicle.ReservationToken = guid;
                    }

                    if (!vendor.VehicleMappingActive)
                    {
                        var vehicleCategories = await _context.Vehiclecategorylang.ToListAsync();
                        var vehicleFuels = await _context.Vehiclefuellang.ToListAsync();
                        var vehicleTransmissions = await _context.Vehicletransmissionlang.ToListAsync();
                        var vehicleTypes = await _context.Vehicletypelang.ToListAsync();

                        apiVehiclesData = apiVehiclesData.Select(x => VehicleHelper.MapVehicleProp(
                                vehicleCategories,
                                vehicleFuels,
                                vehicleTransmissions,
                                vehicleTypes,
                                x,
                                languageType)).ToList();
                    }

                    apiVehiclesData.ForEach(vehicle =>
                    {
                        vehicle.VehicleImages?.ForEach(image =>
                        {
                            image.Url = GetAbsoluteUrl(image.Url, configurations.PortalOwnerDomain);
                        });
                    });

                    if (!vendor.UseBrokerConfigurations)
                    {
                        #region Yeni VendorVendor ekleme işlemi(Yolcu360 için)
                        if (vendor.VendorType == VendorTypes.Yolcu360 || vendor.VendorType == VendorTypes.Yolcu360v2)
                        {
                            var list = apiVehicles.Data2 as IEnumerable<Domain.Models.VendorVendor>;
                            if (list?.Any() == true)
                            {
                                var vendorVendorList = await _vendorVendorService.GetVendorVendorListByVendorId(vendor.VendorId);
                                var existingVendorNames = new HashSet<string>(
                                    vendorVendorList
                                        .Where(e => !string.IsNullOrWhiteSpace(e.VendorName))
                                        .Select(e => e.VendorName.Trim()),
                                    StringComparer.OrdinalIgnoreCase);

                                var newVendorVendors = list
                                    .Where(e => !string.IsNullOrWhiteSpace(e.VendorName))
                                    .Where(e => existingVendorNames.Add(e.VendorName.Trim()))
                                    .ToList();

                                if (newVendorVendors.Any())
                                    await _vendorVendorService.AddRangeAsync(newVendorVendors.Map(), vendor.VendorId);
                            }

                            if ((bool)vendor.FlightNumberRequired)
                            {
                                var vendorVendors = await _vendorVendorService.GetVendorVendorList();
                                foreach (var vehicle in apiVehiclesData)
                                {
                                    var vendorVendor = vendorVendors.Where(e => e.VendorName == vehicle.VendorName).FirstOrDefault();
                                    if (vendorVendor != null)
                                    {
                                        vehicle.VendorFlightPassRequired = vendorVendor.FlightCardMandatory.ToBoolNullSafe();
                                    }
                                }
                            }
                            else
                            {
                                apiVehiclesData.ForEach(e => e.VendorFlightPassRequired = false);
                            }
                        }
                        else
                        {
                            if ((bool)vendor.FlightNumberRequired)
                            {
                                apiVehiclesData.ForEach(e => e.VendorFlightPassRequired = additionalInformation.FlightCardMandatory);
                            }
                            else
                            {
                                apiVehiclesData.ForEach(e => e.VendorFlightPassRequired = false);
                            }
                        }
                    }
                    apiVehiclesData.ForEach(x => x.VendorFlightPassRequired = x.VendorFlightPassRequired ?? false);
                    #endregion

                    try
                    {
                        await _resTokenService.AddRestokenList(resTokenList);
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error("{@AddRestokenList}", ex.ToJson());
                    }

                    if (vendor.AppearingProfitMarkup != 0)
                        apiVehiclesData.ForEach(x => x.ApparentPrice = (float)(x.TotalPrice * (vendor.AppearingProfitMarkup.ToFloatNullSafe() + 100) / 100));

                    apiVehicles.Data = apiVehiclesData;

                    return apiVehicles;
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error("{@VehicleService}", ex.ToJson());
                    return new(null, false, await _configurationService.GetLabel(1880, languageType));
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@VehicleServiceVehicleRequest}", ex.ToJson());
                return new(null, false, await _configurationService.GetLabel(1880, languageType));
            }
        }

        private static string GetAbsoluteUrl(string url, string domain)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                Uri.TryCreate(url, UriKind.Absolute, out _))
                return url;

            return $"{domain.TrimEnd('/')}/{url.TrimStart('/')}";
        }

        public async Task<ServiceResponseBase> GetVehiclesWihtBulkRequest(GetVehiclesRequest getVehicleRequest, int agencyId, string sessionId, bool disableTimeOut = false)
        {
            var vehicles = new List<Vehicle>();
            var languageType = getVehicleRequest.LanguageCode.ToEnum<LanguageTypes>();
            try
            {
                var agency = await _agencyService.GetAgency(agencyId);
                if (agency == null)
                    return new(null, false, await _configurationService.GetLabel(2282, languageType));

                //if (agency.UserRole != UserRoles.Agency)
                //{
                var vendors = await _agencyService.GetVendorsByAgencyAndLocationId(agencyId, getVehicleRequest.PickupLocationId);
                var vehicleTasks = vendors.Select(vendor =>
                {
                    var request = new GetVehiclesRequest
                    {
                        VendorType = (VendorTypes)vendor.VendorType,
                        ApiKey = vendor.ApiKey,
                        ApiPassword = vendor.ApiPassword,
                        ApiClientId = vendor.ApiClientId,
                        //ApiLocationCode = vendor.VendorLocationCode,
                        LanguageCode = getVehicleRequest.LanguageCode,
                        CurrencyCode = getVehicleRequest.CurrencyCode,
                        PickupLocationId = getVehicleRequest.PickupLocationId,
                        ReturnLocationId = getVehicleRequest.ReturnLocationId,
                        PickupDate = getVehicleRequest.PickupDate,
                        ReturnDate = getVehicleRequest.ReturnDate,
                        PickupTime = getVehicleRequest.PickupTime,
                        ReturnTime = getVehicleRequest.ReturnTime,
                        CouponCode = getVehicleRequest.CouponCode,
                        SessionCode = getVehicleRequest.SessionCode
                    };
                    return ExecuteWithTimeout(request);
                });

                var results = await Task.WhenAll(vehicleTasks);
                var allVehicles = results.Where(r => r != null).SelectMany(r => r).ToList();
                vehicles.AddRange(allVehicles);

                if (agency.IsActiveSendCheapestCar.ToBoolNullSafe())
                {
                    var cheapestDuplicates = vehicles
                        .GroupBy(v => v.VehicleId)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.OrderBy(v => v.DailyPrice).First()).ToList();

                    var duplicateIds = new HashSet<int>(cheapestDuplicates.Select(v => v.VehicleId));

                    vehicles.RemoveAll(v => duplicateIds.Contains(v.VehicleId));
                    vehicles.AddRange(cheapestDuplicates);
                }
                    ;
                return new(vehicles, true);
                //}
                //else
                //{
                //    return new(null, false, await _configurationService.GetLabel(2272, languageType));
                //}
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@NullReferenceException}", ex.ToJson());
                return new(null, false, await _configurationService.GetLabel(1880, languageType));
            }
        }
        private async Task<List<Vehicle>> ExecuteWithTimeout(GetVehiclesRequest request)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var _vehicleService = scope.ServiceProvider.GetRequiredService<IVehicleService>();
                var data = await _vehicleService.GetVehicles(request, _agencyService.GetCurrentAgencyId(), request.SessionCode);
                return data.Data as List<Vehicle>;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@ExecuteWithTimeout}", $"Vendor fetch failed: {ex.Message}");
                return null;
            }
        }

        public List<Vehicle> GetCheapestVehicles(List<Vehicle> vehicles) =>
            vehicles.Where(x => x.IsAvailable).GroupBy(x => x.VendorId, (key, g) => g.OrderBy(e => e.DailyPrice).FirstOrDefault()).ToList();

        public async Task InsertCheapestVehicles(List<Vehicle> vehicles, List<KeyValuePair<string, string>> vehiclesTokenList, LanguageTypes languageType, List<Domain.Models.ExchangeRates> exchangeRates)
        {
            if (vehicles != null && vehicles.Count > 0)
            {
                foreach (var vehicle in vehicles)
                {
                    try
                    {
                        var resToken = JsonConvert.DeserializeObject<ReservationToken>(EncryptionHelper.DecryptAES256(vehiclesTokenList.Where(x => x.Key == $"{vehicle.VendorId}-{vehicle.VehicleCode}").FirstOrDefault().Value));
                        if (resToken != null)
                            _context.Vehicleresultstatistic.Add(new Vehicleresultstatistic
                            {
                                Id = 0,
                                Pickuplocationid = vehicle.PickupLocationId,
                                Returnlocationid = vehicle.ReturnLocationId,
                                Vendorid = vehicle.VendorId,
                                Vendorname = vehicle.VendorName,
                                Apivendorid = resToken.APIVendorId,
                                Apivendorname = resToken.APIVendorName,
                                Vehicleid = resToken.VehicleId,
                                Vehiclecode = resToken.VehicleCode,
                                Vehiclename = vehicle.VehicleName,
                                Dailyprice = resToken.DailyPrice.ToDecimalNullSafe(),
                                Apidailyprice = resToken.APIDailyPrice.ToDecimalNullSafe(),
                                Rentalduration = resToken.RentalDuration,
                                Vehicleimageurl = vehicle.VehicleImages != null && vehicle.VehicleImages.Count > 0 ? vehicle.VehicleImages[0].Url : null,
                                Agencyid = resToken.AgencyId,
                                Vendorlogourl = vehicle.VendorLogo,
                                Fuelid = (int)vehicle.FuelType + 1,
                                Transmissionid = (int)vehicle.TransmissionType + 1,
                                Baggageid = (int)vehicle.BaggageQuantityType + 1,
                                Categoryid = (int)vehicle.VehicleCategoryType + 1,
                                Personid = (int)vehicle.PassangerQuantityType + 1,
                                Typeid = (int)vehicle.VehicleType + 1,
                                Pickupdate = vehicle.PickupDateTime,
                                Returndate = vehicle.ReturnDateTime,
                                Currencyid = (int)resToken.CurrencyType + 1,
                                Langid = (int)languageType + 1,
                                Exchangerate = exchangeRates.Where(x => x.CurrencyType == resToken.CurrencyType).FirstOrDefault().ExchangeRate.ToDecimalNullSafe(),

                            });
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error("{@VehicleResultStatistic}", ex.Message);
                    }
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error("{@VehicleResultStatisticSaveChanges}", ex.Message);
                }
            }
        }

        public async Task<ServiceResponseBase> GetRentalConditions(int vendorId, LanguageTypes languageType)
        {
            var rentalConditions = await (from ap in _context.Additionalproduct
                                          join apv in _context.Additionalproductvendor on ap.Productid equals apv.Productid
                                          join v in _context.Vendor on apv.Vendorid equals v.Vendorid
                                          where ap.Active == true &&
                                          ap.Producttype == (int)AdditionalProductTypes.InternalService &&
                                          ap.Langid == (int)languageType + 1 &&
                                          apv.Vendorid == vendorId &&
                                          apv.Active == true
                                          select new RentalCondition
                                          {
                                              ConditionId = ap.Productid,
                                              ConditionCode = ap.Productcode,
                                              ConditionName = ap.Productname,
                                              ConditionSequence = ap.Sequence ?? 1,
                                              IconPath = ap.Iconpath,
                                          }).OrderBy(x => x.ConditionSequence).ToListAsync();

            return new ServiceResponseBase
            {
                Success = true,
                Data = rentalConditions
            };
        }

        public async Task<string> GetFuelName(LanguageTypes languageType, FuelTypes fuelType)
        {
            if (fuelType != FuelTypes.None)
            {
                var vehicleFuel = await _context.Vehiclefuellang.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Fuelid == (int)fuelType + 1).FirstOrDefaultAsync();

                return vehicleFuel.Fuelname;
            }

            return string.Empty;
        }

        public string GetFuelName(List<Vehiclefuellang> vehiclefuellangs, LanguageTypes languageType, FuelTypes fuelType)
        {
            if (fuelType != FuelTypes.None)
                return vehiclefuellangs.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Fuelid == (int)fuelType + 1).FirstOrDefault().Fuelname;

            return string.Empty;
        }

        public async Task<string> GetTransmissionName(LanguageTypes languageType, TransmissionTypes transmissionType)
        {
            if (transmissionType != TransmissionTypes.None)
            {
                var vehicleTransmission = await _context.Vehicletransmissionlang.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Transmissionid == (int)transmissionType + 1).FirstOrDefaultAsync();

                return vehicleTransmission.Transmissionname;
            }

            return string.Empty;
        }

        public string GetTransmissionName(List<Vehicletransmissionlang> vehicletransmissionlangs, LanguageTypes languageType, TransmissionTypes transmissionType)
        {
            if (transmissionType != TransmissionTypes.None)
                return vehicletransmissionlangs.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Transmissionid == (int)transmissionType + 1).FirstOrDefault().Transmissionname;

            return string.Empty;
        }

        public async Task<string> GetVehicleTypeName(LanguageTypes languageType, VehicleTypes vehicleType)
        {
            if (vehicleType != VehicleTypes.None)
            {
                var type = await _context.Vehicletypelang.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Typeid == (int)vehicleType + 1).FirstOrDefaultAsync();

                return type.Typename;
            }

            return string.Empty;
        }

        public string GetVehicleTypeName(List<Vehicletypelang> vehicletypelangs, LanguageTypes languageType, VehicleTypes vehicleType)
        {
            if (vehicleType != VehicleTypes.None)
                return vehicletypelangs.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Typeid == (int)vehicleType + 1).FirstOrDefault().Typename;

            return string.Empty;
        }

        public async Task<string> GetVehicleCategoryTypeName(LanguageTypes languageType, VehicleCategoryTypes vehicleCategoryType)
        {
            if (vehicleCategoryType != VehicleCategoryTypes.None)
            {
                var categoryType = await _context.Vehiclecategorylang.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Categoryid == (int)vehicleCategoryType + 1).FirstOrDefaultAsync();

                return categoryType.Categoryname;
            }

            return string.Empty;
        }

        public string GetVehicleCategoryTypeName(List<Vehiclecategorylang> vehiclecategorylangs, LanguageTypes languageType, VehicleCategoryTypes vehicleCategoryType)
        {
            if (vehicleCategoryType != VehicleCategoryTypes.None)
                return vehiclecategorylangs.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Categoryid == (int)vehicleCategoryType + 1).FirstOrDefault().Categoryname;

            return string.Empty;
        }

        public async Task<string> GetPassangerQuantityTypeName(LanguageTypes languageType, PassangerQuantityTypes passangerQuantityType)
        {
            if (passangerQuantityType != PassangerQuantityTypes.None)
            {
                var passangerQuantity = await _context.Vehiclepersonlang.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Personid == (int)passangerQuantityType + 1).FirstOrDefaultAsync();

                return passangerQuantity.Personname;
            }

            return string.Empty;
        }

        public string GetPassangerQuantityTypeName(List<Vehiclepersonlang> vehiclepersonlangs, LanguageTypes languageType, PassangerQuantityTypes passangerQuantityType)
        {
            if (passangerQuantityType != PassangerQuantityTypes.None)
                return vehiclepersonlangs.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Personid == (int)passangerQuantityType + 1).FirstOrDefault().Personname;

            return string.Empty;
        }

        public async Task<string> GetBaggageQuantityTypeName(LanguageTypes languageType, BaggageQuantityTypes baggageQuantityType)
        {
            if (baggageQuantityType != BaggageQuantityTypes.None)
            {
                var baggageQuantity = await _context.Vehiclebaggagelang.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Baggageid == (int)baggageQuantityType + 1).FirstOrDefaultAsync();

                return baggageQuantity.Baggagename;
            }

            return string.Empty;
        }

        public string GetBaggageQuantityTypeName(List<Vehiclebaggagelang> vehiclebaggagelangs, LanguageTypes languageType, BaggageQuantityTypes baggageQuantityType)
        {
            if (baggageQuantityType != BaggageQuantityTypes.None)
                return vehiclebaggagelangs.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Baggageid == (int)baggageQuantityType + 1).FirstOrDefault().Baggagename;

            return string.Empty;
        }

        public async Task<ServiceResponseBase> GetVehicleClassListFromVendorAPI(int vendorId)
        {
            try
            {
                var dbAgency = await _context.Agency.Where(x => x.Agencyid == _agencyService.GetCurrentAgencyId()).FirstOrDefaultAsync();
                var agency = dbAgency.Map();
                if (await _vendorService.GetVendorById(vendorId, agency) is Domain.Models.Vendor vendor && vendor != null)
                {
                    var vehicleProvider = _vehicleProviderFactory.CreateVehicleProvider(vendor, _memoryCache, _cacheService, false);

                    if (vehicleProvider is null)
                        return new ServiceResponseBase(null, false, "Tedarikçi tipi bulunamadı!");

                    return await vehicleProvider.GetVehicleList(vendor);
                }
                return new ServiceResponseBase(null, false, "Tedarikçi bilgisine ulaşılamadı!");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GetVehicleClassListFromVendorAPI}", ex.ToJson());
                return new(ex.ToJson(), false);
            }
        }

        public async Task<List<Vehicle>> GetLocalVehicleClassesByVendorId(int vendorId, LanguageTypes languageType)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.VehicleVendorKey}-{vendorId}-{languageType.ToString()}", () => GetLocalVehicleClassesByVendorIdAndByLanguage(vendorId, languageType));

            return await GetLocalVehicleClassesByVendorIdAndByLanguage(vendorId, languageType);
        }

        public async Task<List<Vehicle>> GetLocalVehicleClassesByVendorIdAndByLanguage(int vendorId, LanguageTypes languageType)
        {
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                ADODBHelper.GetSqlParameter("OPERATIONID", VehicleClassOperationTypes.GetMappedVehicleClassListForBrokerAPI),
                ADODBHelper.GetSqlParameter("LANGID", (int)languageType + 1),
                ADODBHelper.GetSqlParameter("VENDORID", vendorId),
            };

            //var configurations = await _configurationService.GetConfigurations();

            var portalOwnerDomain = await _configurationService.GetConfigurationByDegisken("PortalOwnerDomain");

            var dtVehicles = _dbHelper.GetDataTableToProc(DBConstants.VEHICLE_CLASS_OPERATIONS_PROC_NAME, parameters.ToArray());

            var vehicleList = dtVehicles.AsEnumerable().Select(dr => new Vehicle
            {
                VehicleId = dr.Field<int>("VEHICLECLASSID").ToIntNullSafe(),
                VehicleCode = dr.Field<string>("APIVEHICLECLASSCODE").ToStringNullSafe(),
                VendorName = dr.Field<string>("VENDORNAME").ToStringNullSafe(),
                SippCode = dr.Field<string>("SIPPCODE").ToStringNullSafe(),
                DepositPrice = dr.Field<decimal?>("DEPOSIT").ToFloatNullAvailable(),
                VendorMinimumDriverAge = dr.Field<int?>("MINDRIVERAGE").ToIntNullAvailable(),
                VendorMinimumDrivingLicenseAge = dr.Field<int?>("MINDRIVINGLICENSEAGE").ToIntNullAvailable(),
                DailyKMLimit = dr.Field<int?>("DAILYKMLIMIT").ToIntNullAvailable(),
                TotalKMLimit = dr.Field<int?>("MAXKMLIMIT").ToIntNullAvailable(),
                VehicleName = dr.Field<string>("VEHICLECLASSNAME").ToStringNullSafe(),
                VehicleCategoryType = (VehicleCategoryTypes)(dr.Field<int>("CATEGORYID").ToIntNullSafe() - 1),
                VehicleCategoryTypeName = dr.Field<string>("CATEGORYNAME").ToStringNullSafe(),
                VehicleType = (VehicleTypes)(dr.Field<int>("TYPEID").ToIntNullSafe() - 1),
                VehicleTypeName = dr.Field<string>("TYPENAME").ToStringNullSafe(),
                PassangerQuantityType = (PassangerQuantityTypes)(dr.Field<int>("PERSONID").ToIntNullSafe() - 1),
                PassangerQuantityName = dr.Field<string>("PERSONNAME").ToStringNullSafe(),
                BaggageQuantityType = (BaggageQuantityTypes)(dr.Field<int>("BAGGAGEID").ToIntNullSafe() - 1),
                BaggageQuantityName = dr.Field<string>("BAGGAGENAME").ToStringNullSafe(),
                TransmissionType = (TransmissionTypes)(dr.Field<int>("TRANSMISSIONID").ToIntNullSafe() - 1),
                TransmissionTypeName = dr.Field<string>("TRANSMISSIONNAME").ToStringNullSafe(),
                FuelType = (FuelTypes)(dr.Field<int>("FUELID").ToIntNullSafe() - 1),
                FuelTypeName = dr.Field<string>("FUELNAME").ToStringNullSafe(),
                IsThereAirCondition = dr.Field<bool>("AIRCONDITION").ToBoolNullSafe(),
                VehicleModelName = dr.Field<string>("MODELNAME").ToStringNullSafe(),
                VehicleBrandName = dr.Field<string>("BRANDNAME").ToStringNullSafe(),
                VehicleDescription = dr.Field<string>("DESCRIPTION").ToStringNullSafe(),
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        //Url = $"{configurations.PortalOwnerDomain}{dr.Field<string>("IMAGEURL").ToStringNullSafe()}",
                        Url = $"{portalOwnerDomain.Deger}{dr.Field<string>("IMAGEURL").ToStringNullSafe()}",
                    }
                },
                VehicleClassNo = dr.Field<int?>("VEHICLECLASSNO").ToIntNullSafe(),
                VehicleGroupName = dr.Field<string>("VEHICLEGROUPNAME").ToStringNullSafe(),
                OldDriverMinAge = dr.Field<int?>("OLDDRIVERMINAGE").ToIntNullSafe(),
                OldDriverMaxAge = dr.Field<int?>("OLDDRIVERMAXAGE").ToIntNullSafe(),
                YoungDriverMinAge = dr.Field<int?>("MINDRIVERAGE").ToIntNullSafe(),
                YoungDriverMaxAge = dr.Field<int?>("YOUNGDRIVERMAXAGE").ToIntNullSafe(),
                SimilarVehicleId = dr.Field<int?>("SIMILARVEHICLEID").ToIntNullSafe()
            }).ToList();

            return vehicleList;
        }
        public async Task<List<VehicleListItem>> GetLocalVehiclesList(LanguageTypes languageType, bool getOnlyMatched = false)
        {
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                ADODBHelper.GetSqlParameter("OPERATIONID", VehicleClassOperationTypes.GetVehicleClassList),
                ADODBHelper.GetSqlParameter("LANGID", (int)languageType + 1),
                ADODBHelper.GetSqlParameter("VEHICLECLASSNAME", string.Empty),
                ADODBHelper.GetSqlParameter("GETONLYMATCHED", getOnlyMatched)
            };

            var configurations = await _configurationService.GetConfigurations();
            var dtVehicles = _dbHelper.GetDataTableToProc(DBConstants.VEHICLE_CLASS_OPERATIONS_PROC_NAME, parameters.ToArray());

            var vehicleList = dtVehicles.AsEnumerable().Select(dr => new VehicleListItem
            {
                VehicleId = dr.Field<int>("VEHICLECLASSID").ToIntNullSafe(),
                VehicleName = dr.Field<string>("VEHICLECLASSNAME").ToStringNullSafe(),
                VehicleDescription = dr.Field<string>("DESCRIPTION").ToStringNullSafe(),
                SippCode = dr.Field<string>("SIPPCODE").ToStringNullSafe(),
                VehicleCategoryType = (VehicleCategoryTypes)(dr.Field<int>("CATEGORYID").ToIntNullSafe() - 1),
                VehicleCategoryTypeName = dr.Field<string>("CATEGORYNAME").ToStringNullSafe(),
                VehicleType = (VehicleTypes)(dr.Field<int>("TYPEID").ToIntNullSafe() - 1),
                VehicleTypeName = dr.Field<string>("TYPENAME").ToStringNullSafe(),
                PassangerQuantityType = (PassangerQuantityTypes)(dr.Field<int>("PERSONID").ToIntNullSafe() - 1),
                PassangerQuantityName = dr.Field<string>("PERSONNAME").ToStringNullSafe(),
                BaggageQuantityType = (BaggageQuantityTypes)(dr.Field<int>("BAGGAGEID").ToIntNullSafe() - 1),
                BaggageQuantityName = dr.Field<string>("BAGGAGENAME").ToStringNullSafe(),
                TransmissionType = (TransmissionTypes)(dr.Field<int>("TRANSMISSIONID").ToIntNullSafe() - 1),
                TransmissionTypeName = dr.Field<string>("TRANSMISSIONNAME").ToStringNullSafe(),
                FuelType = (FuelTypes)(dr.Field<int>("FUELID").ToIntNullSafe() - 1),
                FuelTypeName = dr.Field<string>("FUELNAME").ToStringNullSafe(),
                IsThereAirCondition = dr.Field<bool>("AIRCONDITION").ToBoolNullSafe(),
                VehicleImages = new List<VehicleImage>
                {
                    new VehicleImage
                    {
                        Url = $"{configurations.PortalOwnerDomain}{dr.Field<string>("IMAGEURL").ToStringNullSafe()}",
                    }
                }
            }).ToList();

            return vehicleList;
        }

        public async Task SetPriority(List<Vehicle> vehicles, int vendorId, int locationId, DateTime pickupDate)
        {
            if (vehicles != null && vehicles.Count > 0)
            {
                var priorityConfigurations = await _context.Smartvehiclesequence.Where(x =>
                    x.Active == true &&
                    x.Vendorid == vendorId &&
                    (x.Locationid == locationId || x.Locationid == null))
                .ToListAsync();

                if (priorityConfigurations != null && priorityConfigurations.Count > 0)
                {
                    var priorityConfiguration = new Smartvehiclesequence();
                    if (priorityConfigurations.Count > 1) // Tüm lokasyonlar ve sorgulanan lokasyon önceliklendirilmiş. Bu durumda sorgulanan lokasyonun conf. kullanılır
                        priorityConfiguration = priorityConfigurations.Where(x => x.Locationid != null &&
                        (x.Startdate <= pickupDate || x.Startdate == null) &&
                        (x.Enddate >= pickupDate || x.Enddate == null)).FirstOrDefault();
                    else
                        priorityConfiguration = priorityConfigurations.Where(x =>
                        (x.Startdate <= pickupDate || x.Startdate == null) &&
                        (x.Enddate >= pickupDate || x.Enddate == null)).FirstOrDefault();

                    if (priorityConfiguration != null)
                    {
                        vehicles.ForEach(x => x.Priority = PriorityTypes.P1);
                    }
                }
            }
        }
        public async Task<string> GetVehicleDescription(string vehicleCode, LanguageTypes languageType)
        {
            var vehicleId = await _context.Vehicleclassvendor.Where(x => x.Apivehicleclasscode == vehicleCode).Select(x => x.Vehicleclassid).FirstOrDefaultAsync();
            var vehicleDescription = await _context.Vehicleclasslang.Where(x => x.Vehicleclassid == vehicleId && x.Langid == (int)languageType + 1).Select(x => x.Description).FirstOrDefaultAsync();
            return vehicleDescription;
        }
        public async Task<List<Vehiclefuel>> GetFuelIconPath()
        {
            return _context.Vehiclefuel.ToList();
        }

        public async Task<List<Vehicletransmission>> GetTransmissionIconPath()
        {
            return _context.Vehicletransmission.ToList();
        }

        public async Task<List<Vehicletype>> GetVehicleTypeIconPath()
        {
            return _context.Vehicletype.ToList();
        }

        public async Task<List<Vehiclecategory>> GetVehicleCategoryIconPath()
        {
            return _context.Vehiclecategory.ToList();
        }

        public async Task<List<Vehicleperson>> GetPassangerQuantityTypeIconPath()
        {
            return _context.Vehicleperson.ToList();
        }

        public async Task<List<Vehiclebaggage>> GetBaggageQuantityTypeIconPath()
        {
            return _context.Vehiclebaggage.ToList();
        }

        public string GetVehicleCategoryImagePath(LanguageTypes languageType, VehicleCategoryTypes vehicleCategoryType)
        {
            if (vehicleCategoryType != VehicleCategoryTypes.None)
            {
                var categoryType = _context.Vehiclecategorylang.Where(x =>
                x.Langid == (int)languageType + 1 &&
                x.Categoryid == (int)vehicleCategoryType + 1).FirstOrDefault();

                return categoryType.CategoryImage;
            }

            return string.Empty;
        }

        public async Task<List<Filter>> GetVehicleFilters(List<Domain.Models.VehicleModel> vehicles)
        {
            var vehicleFilter = _context.VehicleFilter.Where(x => x.ParentKey == "Filters");
            List<Filter> filters = new List<Filter>();

            if (vehicleFilter != null)
            {
                foreach (var filter in vehicleFilter)
                {
                    List<ChildFeatures> childFeatures = new List<ChildFeatures>();
                    var newFilter = new Filter
                    {
                        Order = (int)filter.Order,
                        Header = filter.Header,
                        Icon = filter.Icon
                    };

                    if (filter.Key == "FuelFilters")
                    {
                        foreach (var fuelType in vehicles.GroupBy(g => g.VehicleFuelType))
                        {
                            childFeatures.Add(new ChildFeatures
                            {
                                Type = filter.Type,
                                Name = vehicles.FirstOrDefault(v => v.VehicleFuelType == fuelType.Key).VehicleFuelTypeName,
                                Value = fuelType.Key.ToString()
                            });
                        }
                    }
                    else if (filter.Key == "TransmissionFilters")
                    {
                        foreach (var transmission in vehicles.GroupBy(g => g.VehicleTransmissionType))
                        {
                            childFeatures.Add(new ChildFeatures
                            {
                                Type = filter.Type,
                                Name = vehicles.FirstOrDefault(v => v.VehicleTransmissionType == transmission.Key).VehicleTransmissionTypeName,
                                Value = transmission.Key.ToString()
                            });
                        }
                    }
                    else if (filter.Key == "VendorFilters")
                    {
                        foreach (var vendor in vehicles.GroupBy(g => g.VendorName))
                        {
                            childFeatures.Add(new ChildFeatures
                            {
                                Icon = vehicles.FirstOrDefault(v => v.VendorName == vendor.Key).VendorLogo,
                                Type = filter.Type,
                                Name = vehicles.FirstOrDefault(v => v.VendorName == vendor.Key).VendorName,
                                Value = vendor.Key.ToString()
                            });
                        }
                    }
                    newFilter.Children = childFeatures;
                    filters.Add(newFilter);
                }
            }
            return filters;
        }

        public async Task<List<FastFilter>> GetVehicleFastFilters(List<Domain.Models.VehicleModel> vehicles)
        {
            var fastFilter = _context.VehicleFilter.Where(x => x.ParentKey == "FastFilters");
            List<FastFilter> filters = new List<FastFilter>();

            if (fastFilter != null)
            {
                foreach (var filter in fastFilter)
                {
                    List<ChildFeatures> childFeatures = new List<ChildFeatures>();
                    var newFilter = new FastFilter
                    {
                        Name = filter.Name,
                        Value = filter.Value,
                        Icon = filter.Icon
                    };

                    if (filter.Key == "FuelFilter")
                    {
                        foreach (var fuelType in vehicles.GroupBy(g => g.VehicleFuelType))
                        {
                            childFeatures.Add(new ChildFeatures
                            {
                                Type = filter.Type,
                                Name = vehicles.FirstOrDefault(v => v.VehicleFuelType == fuelType.Key).VehicleFuelTypeName,
                                Value = fuelType.Key.ToString()
                            });
                        }
                    }
                    else if (filter.Key == "TransmissionFilter")
                    {
                        foreach (var transmission in vehicles.GroupBy(g => g.VehicleTransmissionType))
                        {
                            childFeatures.Add(new ChildFeatures
                            {
                                Type = filter.Type,
                                Name = vehicles.FirstOrDefault(v => v.VehicleTransmissionType == transmission.Key).VehicleTransmissionTypeName,
                                Value = transmission.Key.ToString()
                            });
                        }
                    }
                    else if (filter.Key == "VehicleCategory")
                    {
                        foreach (var category in vehicles.GroupBy(g => g.VehicleCategoryType))
                        {
                            childFeatures.Add(new ChildFeatures
                            {
                                Type = filter.Type,
                                Name = vehicles.FirstOrDefault(v => v.VehicleCategoryType == category.Key).VehicleCategoryTypeName,
                                Value = category.Key.ToString()
                            });
                        }
                    }
                    newFilter.Children = childFeatures;
                    filters.Add(newFilter);
                }
            }
            return filters;
        }

        public async Task<List<OrderOption>> GetVehicleOrderOptions(int languageId)
        {
            var vehicleOrderOptions = _context.VehicleFilter.Where(x => x.ParentKey == "OrderOptions");
            List<OrderOption> orderOptions = new List<OrderOption>();
            List<Label> labels = await _labelService.GetAllLabelsByLanguageId(languageId);
            foreach (var option in vehicleOrderOptions)
            {
                string label = labels.Where(l => l.LabelKodu == option.Value)?.FirstOrDefault()?.Labeladi;
                orderOptions.Add(new OrderOption
                {
                    Order = option.Order,
                    Icon = option.Icon,
                    IsDefault = option.IsDefault,
                    TargetName = option.Name,
                    SortType = option.Key.StartsWith("Increase") ? (int)SortTypes.IncreaseOrder : (int)SortTypes.DecreaseOrder,
                    Text = label ?? ""
                });
            }
            return orderOptions;
        }

        public async Task<List<FeaturesSorting>> GetVehicleDetails()
        {
            var vehicleDetails = _context.VehicleFilter.Where(x => x.ParentKey == "VehicleDetails").OrderBy(x => x.Order);
            List<FeaturesSorting> featuresSortings = new List<FeaturesSorting>();
            foreach (var detail in vehicleDetails)
            {
                featuresSortings.Add(new FeaturesSorting
                {
                    Order = detail.Order,
                    Icon = detail.Icon,
                    Type = detail.Type,
                    Value = detail.Value,
                    Key = detail.Key,
                    ParentKey = detail.ParentKey,
                    Header = detail.Header,
                });
            }
            return featuresSortings;
        }

        public async Task<List<FeaturesSorting>> GetVehicleFeatures()
        {
            var vehicleFeatures = _context.VehicleFilter.Where(x => x.ParentKey == "VehicleFeatures").OrderBy(x => x.Order);
            List<FeaturesSorting> featuresSortings = new List<FeaturesSorting>();
            foreach (var detail in vehicleFeatures)
            {
                featuresSortings.Add(new FeaturesSorting
                {
                    Order = detail.Order,
                    Icon = detail.Icon,
                    Type = detail.Type,
                    Value = detail.Value,
                    Key = detail.Key,
                    ParentKey = detail.ParentKey,
                    Header = detail.Header,
                });
            }
            return featuresSortings;
        }

        public async Task<List<MobileAppVehicleBadge>> GetMobileAppVehicleBadges()
        {
            return await _context.MobileAppVehicleBadge.ToListAsync();
        }

        public async Task<List<SortableParam>> GetVehicleSortableParams()
        {
            var vehicleOrderOptions = _context.VehicleFilter.Where(x => x.ParentKey == "OrderOptions");
            List<SortableParam> sortableParams = new List<SortableParam>();
            foreach (var option in vehicleOrderOptions)
            {
                sortableParams.Add(new SortableParam
                {
                    Name = option.Name,
                    DataType = (int)Enum.Parse(typeof(DataTypes), option.Type)
                });
            }
            return sortableParams;
        }
        public async Task<List<Vehiclecategorylang>> GetVehicleCategoryLangList(int languageId)
        {
            return await _context.Vehiclecategorylang.Where(c => c.Langid == languageId).ToListAsync();
        }

        public async Task<DeliveryTypeLanguage> GetDeliveryTypeById(int languageId, int deliveryTypeId)
        {
            return await _context.DeliveryTypeLanguages.FirstOrDefaultAsync(d => d.LanguageId == languageId && d.DeliveryTypeId == deliveryTypeId);
        }

        public async Task<List<DeliveryTypeLanguage>> GetDeliveryTypes(int languageId)
        {
            return await _context.DeliveryTypeLanguages.Where(d => d.LanguageId == languageId).ToListAsync();
        }
        public async Task<CouponCode> GetCouponCode(
           int memberId,
           string couponCode,
           DateTime pickupDate,
           DateTime returnDate,
           decimal dailyPrice, CurrencyTypes currencyType)
        {
            // TODO : gkursad
            Coupon couponDb;
            try
            {
                couponDb = (await _context.Coupon.FromSqlRaw("EXEC GETCOUPON @code = {0}, @pickupDate = {1}, @returnDate = {2}, @dailyPrice = {3}",
                    couponCode,
                    pickupDate.ToString("yyyy-MM-dd"),
                    returnDate.ToString("yyyy-MM-dd"),
                    dailyPrice).ToListAsync()).FirstOrDefault();
            }
            catch (Exception ex)
            {
                couponDb = await _context.Coupon.Where(x => x.Code == couponCode && (x.MemberId == memberId || x.MemberId == null) && (x.AgencyId == 2 || x.AgencyId == null || x.AgencyId == 0)).FirstOrDefaultAsync();
            }
            return couponDb.Map(currencyType);
        }
        public async Task<List<Vehicleclass>> GetAllVehicleClasses()
        {
            var vehicleClasses = await _context.Vehicleclass
                                               .Include(v => v.VehicleBrand)
                                               .Include(v => v.VehicleModel)
                                               .ToListAsync();
            return vehicleClasses;
        }
        public static byte[] CompressString(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }
                return output.ToArray();
            }
        }
    }
}
