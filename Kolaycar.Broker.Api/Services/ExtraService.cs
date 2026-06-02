using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;
namespace KolayCAR.Broker.API.Services
{
    public interface IExtraService
    {
        Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtraRequest, bool getMarkupPrice = true, bool getAPIPrices = false, bool isReservationStep = false);
        Task<ServiceResponseBase> GetExtraListToVendorAPI(int vendorId, int agencyId, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration);
        Task<ServiceResponseBase> GetMappedExtras(int vendorId, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration, bool getAllVendorExtras = false, int apiVendorId = 0);
        //Task<ServiceResponseBase> GetSpecialProductsByLocationVendor(int vendorId, int productId);
        Task<List<Extra>> GetPremiumPackets(ReservationToken token, List<int> extras);
        Task<List<Extra>> GetActiveLocalExtras(LanguageTypes languageTypes);
        Task<Extra> GetPremiumPacketByReservationToken(ReservationToken reservationToken);
    }
    public class ExtraService : IExtraService
    {
        private readonly BrokerContext _context;
        private readonly IVendorService _vendorService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IVehicleService _vehicleService;
        private readonly IAgencyService _agencyService;
        private readonly IConfigurationService _configurationService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILocationService _locationService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ICacheService _cacheService;
        private readonly IResTokenService _resTokenService;
        private readonly IExtraProviderFactory _extraProviderFactory;
        private readonly IConfiguration _configuration;
        private readonly IVendorVendorService _vendorVendorService;
        private readonly ISpecialRequestService _specialRequestService;
        private readonly IAdditionalProductService _additionalProductService;
        private readonly IVendorOfficeService _vendorOfficeService;
        private readonly IVendorContactInformationService _vendorContactInformationService;
        public ExtraService(IOptions<AppSettings> appSettings, BrokerContext context, IConfigurationService configurationService, IAgencyService agencyService, IVehicleService vehicleService, IVendorService vendorService, IReservationStepsService reservationStepsService, IMemoryCache memoryCache, ILocationService locationService, IAgencyVendorService agencyVendorService, IExchangeRateService exchangeRateService, ICacheService cacheService, IExtraProviderFactory extraProviderFactory, IResTokenService resTokenService, IConfiguration configuration, IVendorVendorService vendorVendorService, ISpecialRequestService specialRequestService, IAdditionalProductService additionalProductService, IVendorOfficeService vendorOfficeService, IVendorContactInformationService vendorContactInformationService)
        {
            _context = context;
            _configurationService = configurationService;
            _vendorService = vendorService;
            _reservationStepsService = reservationStepsService;
            _agencyService = agencyService;
            _vehicleService = vehicleService;
            _memoryCache = memoryCache;
            _locationService = locationService;
            _exchangeRateService = exchangeRateService;
            _cacheService = cacheService;
            _extraProviderFactory = extraProviderFactory;
            _resTokenService = resTokenService;
            _configuration = configuration;
            _vendorVendorService = vendorVendorService;
            _specialRequestService = specialRequestService;
            _additionalProductService = additionalProductService;
            _vendorOfficeService = vendorOfficeService;
            _vendorContactInformationService = vendorContactInformationService;
        }

        public async Task<ServiceResponseBase> GetExtraListToVendorAPI(int vendorId, int agencyId, CommonModels.CurrencyTypes currencyType, CommonModels.LanguageTypes languageType, int rentalDuration)
        {
            var agency = await _agencyService.GetAgency(agencyId);
            var configurations = await _configurationService.GetConfigurations();
            if (agency != null)
            {
                if (await _vendorService.GetVendorById(vendorId, agency) is CommonModels.Vendor vendor && vendor != null)
                {
                    await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                    var extraProvider = _extraProviderFactory.CreateExtraProvider(vendor, _memoryCache, _cacheService, _configuration);

                    if (extraProvider is null)
                        return new ServiceResponseBase(null, false, "Tedarikçi tipi bulunamadı!");

                    var result = await extraProvider.GetExtraList(vendor, currencyType, languageType, rentalDuration);

                    //#region Özel ek ürün paketi test
                    //if (configurations.SpecialExtrasIsActive)
                    //{
                    //    //var extraList = new List<CommonModels.Extra>();
                    //    var extraList = result.Data != null ? result.Data as List<CommonModels.Extra> : new List<Extra>();

                    //    extraList.AddRange(GetSpecialExtrasList(vendor, extraList));

                    //    result.Data = extraList;
                    //}
                    //#endregion

                    return result;
                }
                else
                    return new(null, false, "Tedarikçi bilgisine ulaşılamadı!");

            }
            else
                return new(null, false, "Acente bilgisine ulaşılamadı!");
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, bool getMarkupPrice = true, bool getAPIPrices = false, bool isReservationStep = false)
        {
            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(getExtrasRequest.ReservationToken);
            if (reservationToken == null)
                return new(null, false, "ReservationToken hatalı!");

            var agency = await _agencyService.GetAgency(reservationToken.AgencyId);
            if (agency == null)
                return new(null, false, "The agency information could not be reached!");

            var vendor = await _vendorService.GetVendorById(reservationToken.VendorId, agency, reservationToken.APIVendorId);
            if (vendor == null)
                return new(null, false, "The vendor information could not be reached!");

            ReservationHelper.FillGetExtrasRequest(getExtrasRequest, reservationToken);
            await _agencyService.SetAgencyPaymentOptions(agency, vendor);
            var subVendorsDb = await _vendorService.GetSubVendorList();
            var subVendors = subVendorsDb.Map();

            if (await _reservationStepsService.GetAdditionalInformation(
                vendor,
                agency,
                getExtrasRequest.LanguageCode,
                getExtrasRequest.CurrencyCode,
                vendor.CurrencyType,
                getExtrasRequest.PickupLocationId,
                getExtrasRequest.ReturnLocationId,
                getExtrasRequest.PickupDate,
                getExtrasRequest.ReturnDate,
                getExtrasRequest.PickupTime,
                getExtrasRequest.ReturnTime,
                vehicleId: reservationToken.VehicleId,
                apiVendorId: reservationToken.APIVendorId,
                rentalDuration: reservationToken.RentalDuration,
                reservationToken: reservationToken,
                apiLocationCode: reservationToken.APIPickupLocationCode) is ResponseReservationStepsAdditionalInformation additionalInformation && additionalInformation != null)
            {
                var localVehicles = await _vehicleService.GetLocalVehicleClassesByVendorId(vendor.VendorId, (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getExtrasRequest.LanguageCode));
                var exchangeRates = await _exchangeRateService.GetAllExchangeRates();
                var mappedExchangeRates = exchangeRates.Map();

                getExtrasRequest.ApiKey = vendor.ApiKey;
                getExtrasRequest.ApiPassword = vendor.ApiPassword;
                getExtrasRequest.VendorType = vendor.VendorType;

                var extraProvider = _extraProviderFactory.CreateExtraProvider(vendor, _memoryCache, _cacheService, _configuration);

                if (extraProvider is null)
                    return new(null, false, "Tedarikçi tipi bulunamadı!");

                var extrasResult = await extraProvider.GetExtras(getExtrasRequest, vendor, additionalInformation, mappedExchangeRates, localVehicles, subVendors,
                                addProfitMarkup: getMarkupPrice, getAPIPrices: getAPIPrices);

                if (extrasResult.Success)
                {
                    if (extrasResult.Data != null)
                    {
                        var extrasData = extrasResult.Data as GetExtrasResponse;

                        VehicleHelper.SetVehiclePropertyBySIPPCode(extrasData.Vehicle);
                        var configurations = await _configurationService.GetConfigurations();
                        _agencyService.SetVehiclePaymentOptions(extrasData.Vehicle, agency, configurations.GetPaymentSettingsFromBroker);

                        var vehicleCategories = await _context.Vehiclecategorylang.ToListAsync();
                        var vehicleFuels = await _context.Vehiclefuellang.ToListAsync();
                        var vehicleTransmissions = await _context.Vehicletransmissionlang.ToListAsync();
                        var vehicleTypes = await _context.Vehicletypelang.ToListAsync();
                        var pickupLocation = await _context.Location.Where(x => x.Id == reservationToken.PickupLocationId).FirstOrDefaultAsync();
                        var vendorLocation = await _locationService.GetLocationVendor(reservationToken.PickupLocationId, vendor.VendorId);

                        #region tedarikçi score ve yorum saati
                        var vendorScoreResult = await _context.VendorScoresDto.FromSqlRaw("EXEC GETVENDORSCOREBYLOCATIONID @languageId = {0}, @vendorId = {1}, @locationId = {2}", (int)(LanguageTypes)Enum.Parse(typeof(LanguageTypes), getExtrasRequest.LanguageCode, true), vendor.VendorId, getExtrasRequest.PickupLocationId).ToListAsync();
                        var vendorScore = vendorScoreResult.FirstOrDefault();
                        #endregion

                        if (!vendor.HideLocationAddressOnPayment && configurations.ShowLocationAddressOnPayment)
                        {
                            var pickupOffice = await _vendorContactInformationService.GetVendorContactInformation(getExtrasRequest.PickupLocationId, vendor.VendorId);
                            var returnOffice = await _vendorContactInformationService.GetVendorContactInformation(getExtrasRequest.ReturnLocationId, vendor.VendorId);

                            extrasData.PickupLocationAddress = pickupOffice?.Address ?? "";
                            extrasData.ReturnLocationAddress = returnOffice?.Address ?? "";
                        }

                        extrasData.Vehicle = VehicleHelper.MapVehicleProp(
                            vehicleCategories,
                            vehicleFuels,
                            vehicleTransmissions,
                            vehicleTypes,
                            extrasData.Vehicle,
                            (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getExtrasRequest.LanguageCode),
                            vendor);

                        List<RentalCondition> rentalConditionsData = new List<RentalCondition>();
                        if (vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations))
                        {
                            var rentalConditionsResult = await _vehicleService.GetRentalConditions(vendor.VendorId, (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getExtrasRequest.LanguageCode));
                            rentalConditionsData = rentalConditionsResult.Success ? rentalConditionsResult.Data as List<RentalCondition> : null;
                        }

                        #region Bazı tedarikçi extra servislerinde araç listelemeye gitmemekte. Bundan dolayı şehir bilgisi gelmiyor. Bu işlem eklendi.
                        if (string.IsNullOrEmpty(extrasData.Vehicle.CityOfPickupLocation) || string.IsNullOrEmpty(extrasData.Vehicle.CityOfReturnLocation))
                        {
                            var returnLocation = await _context.Location.Where(x => x.Id == getExtrasRequest.ReturnLocationId).FirstOrDefaultAsync();
                            var cityOfPickupLocation = await _context.City.Where(x => x.Cityid == pickupLocation.Cityid).FirstOrDefaultAsync();
                            var cityOfReturnLocation = returnLocation != null ? await _context.City.Where(x => x.Cityid == returnLocation.Cityid).FirstOrDefaultAsync() : null;

                            extrasData.Vehicle.CityOfPickupLocation = cityOfPickupLocation?.Cityname;
                            extrasData.Vehicle.CityOfReturnLocation = cityOfReturnLocation?.Cityname;
                        }
                        #endregion

                        extrasData.Vehicle.RentalConditions = rentalConditionsData;
                        extrasData.Vehicle.ReservationToken = getExtrasRequest.ReservationToken;
                        extrasData.Vehicle.IsOffice = (vendor.VendorType == VendorTypes.Yolcu360 || vendor.VendorType == VendorTypes.Yolcu360v2) ? extrasData.Vehicle.IsOffice : vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? vendorLocation.Isoffice ?? false : extrasData.Vehicle.IsOffice;
                        extrasData.Vehicle.IsAirport = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? pickupLocation.Airport ?? false : extrasData.Vehicle.IsAirport;
                        extrasData.Vehicle.VendorLogo = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? $"{configurations.PortalOwnerDomain}{extrasData.Vehicle.VendorLogo}" : extrasData.Vehicle.VendorLogo;
                        extrasData.Vehicle.CurrencyCode = getExtrasRequest.CurrencyCode;
                        extrasData.Vehicle.VendorCouponUsing = vendor.CouponCodeActive.ToBoolNullSafe();
                        extrasData.Vehicle.VendorScore = vendorScoreResult != null && vendorScore != null ? vendorScore.CurrentScore.ToFloatNullSafe() != 0 && vendorScore.CurrentScore != -1 ? vendorScore.CurrentScore.ToFloatNullSafe() : vendorScore.Score.ToFloatNullSafe() != 0 ? vendorScore.Score.ToFloatNullSafe() : 4 : 4;
                        extrasData.Vehicle.VendorCommentCount = vendorScoreResult != null && vendorScore != null ? vendorScore.CommentCount.ToIntNullSafe() : 0;
                        extrasData.Vehicle.IsFindeksRequired = (bool)vendor.FindeksRequired;
                        extrasData.Vehicle.BaseVendorId = reservationToken.VendorId;

                        if (vendor.ExtraMappingActive && !getAPIPrices && (vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations)))
                        {
                            var apiExtrasData = extrasResult.Data as GetExtrasResponse;
                            var listAll = apiExtrasData.Extras != null && apiExtrasData.Extras.Count > 0 ? apiExtrasData.Extras : new List<Extra>();

                            var localExtrasResult = await GetMappedExtras(
                                vendor.VendorId,
                                (CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), getExtrasRequest.CurrencyCode),
                                (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getExtrasRequest.LanguageCode),
                                reservationToken.RentalDuration,
                                apiVendorId: vendor.VendorType != VendorTypes.Yolcu360 ? reservationToken.APIVendorId : 0);

                            if (localExtrasResult.Success)
                            {
                                var localExtras = localExtrasResult.Data as List<Extra>;

                                if (apiExtrasData.Extras?.Any() == true)
                                {
                                    var apiExtras = apiExtrasData.Extras;

                                    localExtras = localExtras.Where(x => apiExtras.Any(a => a.ExtraCode == x.ExtraCode)).ToList();

                                    var localExtrasByCode = localExtras
                                        .GroupBy(x => x.ExtraCode)
                                        .ToDictionary(g => g.Key, g => g.OrderByDescending(e => e.Price).First());

                                    foreach (var apiExtra in apiExtras
                                        .GroupBy(x => x.ExtraCode)
                                        .Select(g => g.OrderByDescending(e => e.Price).First()))
                                    {
                                        if (!localExtrasByCode.TryGetValue(apiExtra.ExtraCode, out var localExtra))
                                            continue;

                                        if (localExtrasResult.ServiceCode == "-1" &&
                                            localExtra.ExtraType != AdditionalProductTypes.Premium)
                                            localExtra.Price = apiExtra.Price;

                                        localExtra.ApiPrice = apiExtra.ApiPrice;
                                        localExtra.ExtraQuantityIncreasable = apiExtra.ExtraQuantityIncreasable;
                                        localExtra.ExtraRentalType = apiExtra.ExtraRentalType;
                                        localExtra.CurrencyCode = apiExtra.CurrencyCode;
                                        localExtra.Label = apiExtra.Label;
                                        localExtra.DamageInsuranceCategory = apiExtra.DamageInsuranceCategory;
                                        localExtra.ApiExtraCode = apiExtra.ApiExtraCode;

                                        if (vendor.ExtraDescriptionFromVendor)
                                            localExtra.ExtraDescription = apiExtra.ExtraDescription ?? localExtra.ExtraDescription;

                                        localExtra.Code = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{localExtra.ExtraId}~{localExtra.ExtraCode}"));

                                        var cyrptExtra = localExtra.Map();

                                        if (!reservationToken.CyrptExtras.Any(e => e.I == cyrptExtra.I && e.C == cyrptExtra.C))
                                            reservationToken.CyrptExtras.Add(cyrptExtra);
                                    }
                                }

                                apiExtrasData.Extras = ReservationHelper.RemoveZeroPriceExtras(localExtras);
                            }
                        }
                        else
                        {
                            PrepareDynamicProviderExtras(extrasData, reservationToken, getExtrasRequest.CurrencyCode);
                        }

                        if (!vendor.UseBrokerConfigurations)
                        {
                            if (vendor.VendorType == VendorTypes.Yolcu360)
                            {
                                if ((bool)vendor.FlightNumberRequired)
                                {
                                    var vendorVendors = await _vendorVendorService.GetVendorVendorList();
                                    var vendorVendor = vendorVendors.Where(e => e.VendorName == extrasData.Vehicle.VendorName).FirstOrDefault();
                                    if (vendorVendor != null)
                                        extrasData.Vehicle.VendorFlightPassRequired = vendorVendor.FlightCardMandatory.ToBoolNullSafe();
                                }
                                else
                                {
                                    extrasData.Vehicle.VendorFlightPassRequired = false;
                                }
                            }
                            else
                            {
                                if ((bool)vendor.FlightNumberRequired)
                                    extrasData.Vehicle.VendorFlightPassRequired = additionalInformation.FlightCardMandatory;
                                else
                                    extrasData.Vehicle.VendorFlightPassRequired = false;
                            }
                        }

                        extrasData.Vehicle.VendorFlightPassRequired = extrasData.Vehicle.VendorFlightPassRequired ?? false;

                        var newAgency = await _agencyService.GetAgency(reservationToken.AgencyId);
                        if (!agency.FullCreditPermission)
                            extrasData.Vehicle.FullCredit = false;

                        var langId = Enum.TryParse<LanguageTypes>(
                                            getExtrasRequest.LanguageCode,
                                            true,
                                            out var parsed)
                                            ? parsed
                                            : LanguageTypes.EN;

                        var specialRequests = await _specialRequestService.GetSpecialRequestByFilter(additionalInformation, extrasData.Vehicle.DailyPrice, (int)extrasData.Vehicle.VehicleCategoryType);

                        var premiumPackets = await _additionalProductService.GetPremiumPackets(specialRequests, reservationToken, additionalInformation.RentalDuration, (int)langId);

                        if (premiumPackets?.Any() == true)
                        {
                            extrasData.Extras ??= new List<Extra>();

                            foreach (var extra in premiumPackets)
                            {
                                extra.Code = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(extra.ExtraId + "~" + extra.ExtraCode))}";

                                var cyrptExtra = extra.Map();

                                if (!reservationToken.CyrptExtras.Any(e => e.CD == cyrptExtra.CD))
                                    reservationToken.CyrptExtras.Add(cyrptExtra);
                            }

                            extrasData.Extras = extrasData.Extras.Concat(premiumPackets).GroupBy(x => x.ExtraId).Select(g => g.First()).ToList();
                        }
                        if (
                            agency.UserRole == UserRoles.External &&
                            agency.AgencyName.Contains("Airtuerk") &&
                            extrasData?.Extras?.Any(e => e.ExtraType == AdditionalProductTypes.Premium) == true
                            )
                        {
                            var premiumExtras = extrasData.Extras.Where(e => e.ExtraType == AdditionalProductTypes.Premium);
                            foreach (var extra in premiumExtras)
                            {
                                extra.Price = CalculationHelper.CurrencyExchange(exchangeRates.Map(), vendor, extra.ApiPrice, reservationToken.BaseVendorRequestCurrencyType, reservationToken.CurrencyType);
                            }
                        }
                    }
                }

                if (!isReservationStep)
                    await _resTokenService.UpdateResToken(getExtrasRequest.ReservationToken, reservationToken);

                return extrasResult;
            }
            return new(null, false, "Check your request parameters!");
        }

        private static void PrepareDynamicProviderExtras(GetExtrasResponse extrasData, ReservationToken reservationToken, string currencyCode)
        {
            if (extrasData?.Extras == null)
                return;

            for (var i = 0; i < extrasData.Extras.Count; i++)
            {
                var extra = extrasData.Extras[i];

                if (extra == null)
                    continue;

                if (extra.ExtraId <= 0)
                    extra.ExtraId = CreateDynamicExtraId(extra, i);

                extra.ExtraCode = extra.ExtraCode.ToStringNullSafe();
                extra.ApiExtraCode = string.IsNullOrWhiteSpace(extra.ApiExtraCode) ? extra.ExtraCode : extra.ApiExtraCode;
                extra.ExtraRentalType ??= ExtraRentalTypes.Daily;
                extra.ExtraType ??= AdditionalProductTypes.Extra;
                extra.CurrencyCode = string.IsNullOrWhiteSpace(extra.CurrencyCode) ? currencyCode : extra.CurrencyCode;
                extra.Code = string.IsNullOrWhiteSpace(extra.Code)
                    ? Convert.ToBase64String(Encoding.UTF8.GetBytes($"{extra.ExtraId}~{extra.ExtraCode}"))
                    : extra.Code;

                var cyrptExtra = extra.Map();

                if (!reservationToken.CyrptExtras.Any(e => e.I == cyrptExtra.I && e.C == cyrptExtra.C))
                    reservationToken.CyrptExtras.Add(cyrptExtra);
            }
        }

        private static int CreateDynamicExtraId(Extra extra, int index)
        {
            var value = $"{extra.ExtraCode.ToStringNullSafe()}|{extra.ApiExtraCode.ToStringNullSafe()}|{extra.ExtraName.ToStringNullSafe()}|{index}";

            unchecked
            {
                var hash = 23;
                foreach (var character in value)
                    hash = (hash * 31) + character;

                var id = hash & 0x7fffffff;
                return id == 0 ? index + 1 : id;
            }
        }

        public async Task<ServiceResponseBase> GetMappedExtras(int vendorId, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration, bool getAllVendorExtras = false, int apiVendorId = 0)
        {
            if (!getAllVendorExtras)
            {
                var configurations = await _configurationService.GetConfigurations();
                if (vendorId > 0)
                {
                    var days = await _context.Additionalproductvendordays.Where(x => x.Vendorid == vendorId).ToListAsync();
                    var extras = await (from ap in _context.Additionalproduct
                                        join apv in _context.Additionalproductvendor on ap.Productid equals apv.Productid
                                        join v in _context.Vendor on apv.Vendorid equals v.Vendorid
                                        where ap.Active == true &&
                                        ap.Langid == (int)languageType + 1 &&
                                        (ap.Producttype == (int)AdditionalProductTypes.Extra || ap.Producttype == (int)AdditionalProductTypes.Insurance) &&
                                        apv.Vendorid == vendorId &&
                                        apv.Active == true &&
                                        apiVendorId != 0 ? apv.Apivendorid == apiVendorId :
                                        (ap.Active == true &&
                                        ap.Langid == (int)languageType + 1 &&
                                        (ap.Producttype == (int)AdditionalProductTypes.Extra || ap.Producttype == (int)AdditionalProductTypes.Insurance || ap.Producttype == (int)AdditionalProductTypes.Compulsory || ap.Producttype == (int)AdditionalProductTypes.Premium) &&
                                        apv.Vendorid == vendorId &&
                                        apv.Active == true)
                                        select new Extra
                                        {
                                            ExtraId = ap.Productid,
                                            ExtraCode = apv.Apiproductcode,
                                            ExtraName = ap.Productname,
                                            ExtraDescription = ap.Productdescription,
                                            ExtraRentalType = (ExtraRentalTypes)ap.Rentaltype,
                                            ExtraType = (AdditionalProductTypes)ap.Producttype,
                                            ExtraQuantityIncreasable = ap.Quantityincreasable ?? false,
                                            Price = ap.Defaultprice.ToFloatNullSafe(),
                                            VendorId = v.Vendorid,
                                            VendorName = v.Vendorname,
                                            ShowDayCountStart = ap.Showdaycountstart,
                                            ShowDayCountEnd = ap.Showdaycountend,
                                            Icon = $"{configurations.PortalOwnerDomain}{ap.Iconpath}",
                                            Sequence = ap.Sequence ?? 1,
                                            VendorExtraExists = ap.VendorExtraExists ?? false,
                                            DefaultPrice = ap.Defaultprice.ToFloatNullSafe()
                                        }).OrderBy(x => x.Sequence).ToListAsync();
                    if (extras?.Count == 0)
                    {
                        extras = await (from ap in _context.Additionalproduct
                                        join apv in _context.Additionalproductvendor on ap.Productid equals apv.Productid
                                        join v in _context.Vendor on apv.Vendorid equals v.Vendorid
                                        where ap.Active == true &&
                                        ap.Langid == (int)languageType + 1 &&
                                        (ap.Producttype == (int)AdditionalProductTypes.Extra || ap.Producttype == (int)AdditionalProductTypes.Insurance) &&
                                        apv.Vendorid == vendorId &&
                                        apv.Active == true &&
                                        apv.Apivendorid == 0
                                        select new Extra
                                        {
                                            ExtraId = ap.Productid,
                                            ExtraCode = apv.Apiproductcode,
                                            ExtraName = ap.Productname,
                                            ExtraDescription = ap.Productdescription,
                                            ExtraRentalType = (ExtraRentalTypes)ap.Rentaltype,
                                            ExtraType = (AdditionalProductTypes)ap.Producttype,
                                            ExtraQuantityIncreasable = ap.Quantityincreasable ?? false,
                                            Price = ap.Defaultprice.ToFloatNullSafe(),
                                            VendorId = v.Vendorid,
                                            VendorName = v.Vendorname,
                                            ShowDayCountStart = ap.Showdaycountstart,
                                            ShowDayCountEnd = ap.Showdaycountend,
                                            Icon = $"{configurations.PortalOwnerDomain}{ap.Iconpath}",
                                            Sequence = ap.Sequence ?? 1,
                                            VendorExtraExists = ap.VendorExtraExists ?? false,
                                            DefaultPrice = ap.Defaultprice.ToFloatNullSafe()
                                        }).OrderBy(x => x.Sequence).ToListAsync();
                    }

                    foreach (var extra in extras.ToList())
                        if (extra.ShowDayCountStart != null && extra.ShowDayCountEnd != null)
                            if (rentalDuration < extra.ShowDayCountStart || rentalDuration > extra.ShowDayCountEnd)
                                extras.Remove(extra);

                    if (days.Count > 0)
                    {
                        var prices = await _context.Additionalproductprice.Where(x => x.Vendorid == vendorId).ToListAsync();

                        if (rentalDuration > 0)
                        {
                            var agencyBasedPassiveVendors = await _vendorService.GetAgencyBasedPassiveVendors(_agencyService.GetCurrentAgencyId());
                            var dbVendor = await _context.Vendor.Where(x => x.Vendorid == vendorId && x.Active == true && !agencyBasedPassiveVendors.Contains(x.Vendorid)).FirstOrDefaultAsync();
                            var vendor = dbVendor.Map();

                            var exchangeRates = await _context.Exchangerates.ToListAsync();
                            var mappedExchangeRates = exchangeRates.Map();

                            foreach (var extra in extras.ToList())
                            {
                                int maxDay = 1;
                                float maxPrice = 0;
                                float maxDayPrice = 0;
                                bool found = false;
                                foreach (var price in prices)
                                {
                                    int extraId = extra.ExtraId;
                                    float extraPrice = price.Price.ToFloatNullSafe();

                                    if (price.Productid == extraId)
                                    {
                                        if (price.Startday <= rentalDuration && price.Endday >= rentalDuration)
                                        {
                                            found = true;
                                            extra.Price = extraPrice;
                                        }
                                        if (maxDay < price.Endday)
                                        {
                                            maxDayPrice = extraPrice;
                                            maxDay = price.Endday;
                                        }
                                        if (maxPrice < extraPrice)
                                            maxPrice = extraPrice;
                                    }
                                }

                                if (rentalDuration > maxDay && maxPrice > 0)
                                {
                                    extra.Price = maxPrice;
                                    found = true;
                                }

                                if (!found)
                                    extras.Remove(extra);
                                else
                                {
                                    extra.Price = CalculationHelper.CurrencyExchange(mappedExchangeRates, vendor, extra.Price, vendor.CurrencyType, currencyType);
                                }
                            }
                            return new(extras, true);
                        }
                        else
                            return new(null, false, "Kiralama gün sayısı 0'dan büyük olmalıdır!", "Kiralama gün sayısı 0'dan büyük olmalıdır!");
                    }

                    return new(extras, true, serviceCode: "-1");
                }
                else if (vendorId == 0) //Broker kendi extra listesini dönüyor
                {
                    var extras = await (from ap in _context.Additionalproduct
                                        where ap.Active == true &&
                                        ap.Langid == (int)languageType + 1 &&
                                        ((ap.Producttype == (int)AdditionalProductTypes.Extra || ap.Producttype == (int)AdditionalProductTypes.Insurance) || (ap.Producttype == (int)AdditionalProductTypes.Premium && ap.VendorExtraExists == true)) &&
                                        ap.Active == true
                                        select new Extra
                                        {
                                            ExtraId = ap.Productid,
                                            ExtraCode = ap.Productcode,
                                            ExtraName = ap.Productname,
                                            ExtraDescription = ap.Productdescription,
                                            ExtraRentalType = (ExtraRentalTypes)ap.Rentaltype,
                                            ExtraType = (AdditionalProductTypes)ap.Producttype,
                                            ExtraQuantityIncreasable = ap.Quantityincreasable ?? false,
                                            Price = ap.Defaultprice.ToFloatNullSafe(),
                                            ShowDayCountStart = ap.Showdaycountstart,
                                            ShowDayCountEnd = ap.Showdaycountend,
                                            Icon = $"{configurations.PortalOwnerDomain}{ap.Iconpath}",
                                            VendorExtraExists = ap.VendorExtraExists ?? false,
                                        }).ToListAsync();

                    return new(extras, true);
                }
            }
            else
            {
                var allExtras = new List<Extra>();
                var vendors = await _context.Vendor.ToListAsync();
                foreach (var vendor in vendors)
                {
                    var vendorExtras = await GetMappedExtras(vendor.Vendorid, currencyType, languageType, rentalDuration);
                    if (vendorExtras.Success)
                    {
                        if (vendorExtras.Data != null)
                        {
                            var extras = vendorExtras.Data as List<Extra>;
                            allExtras.AddRange(extras);
                        }
                    }
                }
                return new(allExtras, true);
            }
            return new(null, false, "Lütfen geçerli bir vendorId değeri gönderin!");
        }

        //private async Task<List<CommonModels.Extra>> GetSpecialExtras(CommonModels.Vendor vendor, List<ExchangeRates> exchangeRates, GetExtrasRequest getExtrasRequest, ReservationToken reservationToken, List<CommonModels.Extra> extras = null, bool addProfitMarkup = true, bool getAPIPrices = false)
        //{
        //    var specialExtraList = await (from spp in _context.SpecialProductPrices
        //                                  join apv in _context.Additionalproductvendor on spp.AdditionalProductId equals apv.Productid
        //                                  where spp.VendorId == vendor.VendorId && spp.LocationId == getExtrasRequest.PickupLocationId && apv.Vendorid == vendor.VendorId
        //                                  select new CommonModels.SpecialProductPrice
        //                                  {
        //                                      Id = spp.Id,
        //                                      LocationId = spp.LocationId,
        //                                      VendorId = spp.VendorId,
        //                                      AdditionalProductId = spp.AdditionalProductId,
        //                                      Price = spp.Price,
        //                                      CurrencyId = spp.CurrencyId,
        //                                      RentalTypes = (ExtraRentalTypes)apv.Rentaltype.ToIntNullSafe()
        //                                  }
        //                ).ToListAsync();

        //    var extraList = new List<CommonModels.Extra>();
        //    var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
        //    var dailyExtra = specialExtraList.Where(x => x.RentalTypes == ExtraRentalTypes.Daily).FirstOrDefault();
        //    var perRentalExtra = specialExtraList.Where(x => x.RentalTypes == ExtraRentalTypes.PerRental).FirstOrDefault();

        //    //var specialExtraList = _context.SpecialProductPrices.Where(x => x.VendorId == vendor.VendorId && x.LocationId == getExtrasRequest.PickupLocationId).ToList();
        //    //var extraParams = _context.Parametre.Where(x => x.Degisken == "SpecialExtraPrice" || x.Degisken == "SpecialExtraCurrency").ToList();
        //    //var price = extraParams.Where(x => x.Degisken == "SpecialExtraPrice").FirstOrDefault();
        //    //var currency = extraParams.Where(x => x.Degisken == "SpecialExtraCurrency").FirstOrDefault();
        //    //var baseCurrency = currency != null ? currency.Deger.ToString().ToUpper().ToEnum<CurrencyTypes>() : CurrencyTypes.TRY;
        //    ////var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
        //    //var rentalDuration = reservationToken.RentalDuration;

        //    if (perRentalExtra != null)
        //    {
        //        #region RentalType: PerRental
        //        extraList.Add(new Extra
        //        {
        //            ExtraId = 1931,
        //            ExtraCode = "BrokerSpecialPack001",
        //            ExtraName = "***Özel Ekstra Paketi(Kiralama Başına-Broker)",
        //            ExtraRentalType = ExtraRentalTypes.PerRental,
        //            ExtraQuantityIncreasable = false,
        //            VendorId = extras?.Count > 0 ? extras[0].VendorId : 0,
        //            Price = perRentalExtra.Price.ToFloatNullSafe(),
        //            CurrencyCode = ((CurrencyTypes)(perRentalExtra.CurrencyId - 1)).ToString(),
        //            CurrencyType = (CurrencyTypes)(perRentalExtra.CurrencyId - 1)
        //        });
        //        #endregion
        //    }

        //    if (dailyExtra != null)
        //    {
        //        #region RentalType: Daily
        //        extraList.Add(new Extra
        //        {
        //            ExtraId = 1932,
        //            ExtraCode = "BrokerSpecialPack002",
        //            ExtraName = "***Özel Ekstra Paketi(Günlük-Broker)",
        //            ExtraRentalType = ExtraRentalTypes.Daily,
        //            ExtraQuantityIncreasable = false,
        //            VendorId = extras?.Count > 0 ? extras[0].VendorId : 0,
        //            Price = dailyExtra.Price.ToFloatNullSafe(),
        //            CurrencyCode = ((CurrencyTypes)(dailyExtra.CurrencyId - 1)).ToString(),
        //            CurrencyType = (CurrencyTypes)(dailyExtra.CurrencyId - 1)
        //        });
        //        #endregion
        //    }

        //    if (extraList.Count > 0)
        //        CalculationHelper.SetExtraPricesForSpecialExtras(extraList, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices);

        //    return extraList;
        //}

        //private List<CommonModels.Extra> GetSpecialExtrasList(CommonModels.Vendor vendor, List<CommonModels.Extra> extras = null)
        //{
        //    int vendorId = extras?.FirstOrDefault()?.VendorId ?? 0;

        //    return new List<CommonModels.Extra>
        //        {
        //            new Extra
        //            {
        //                ExtraId = 1931,
        //                ExtraCode = "BrokerSpecialPack001",
        //                ExtraName = "***Özel Ekstra Paketi(Kiralama Başına-Broker)",
        //                ExtraRentalType = ExtraRentalTypes.PerRental,
        //                ExtraQuantityIncreasable = false,
        //                VendorId = vendorId
        //            },
        //            new Extra
        //            {
        //                ExtraId = 1932,
        //                ExtraCode = "BrokerSpecialPack002",
        //                ExtraName = "***Özel Ekstra Paketi(Günlük-Broker)",
        //                ExtraRentalType = ExtraRentalTypes.Daily,
        //                ExtraQuantityIncreasable = false,
        //                VendorId = vendorId
        //            }
        //        };
        //}

        //private bool CheckLocationSpecialExtraRequired(int locationId, CommonModels.LocationVendor locationVendor, CommonModels.Vendor vendor, ReservationToken _reservationToken)
        //{
        //    if (vendor.VendorType != VendorTypes.Yolcu360)
        //        return locationVendor != null && locationVendor.SpecialPackrequirement;
        //    else
        //        return locationVendor != null && locationVendor.SpecialPackrequirement && vendor.VendorVendors.Where(x => x.VendorName.ToLower() == _reservationToken.APIVendorName.ToLower() && x.HgsPackage).FirstOrDefault() != null;
        //}
        public async Task<List<Extra>> GetPremiumPackets(ReservationToken token, List<int> extraIdList)
        {
            var agency = await _agencyService.GetAgency(token.AgencyId);
            var premiumPackets = await _context.Additionalproduct.Where(ap => ap.Active && ap.Producttype == 5 && ap.Langid == (int)token.LanguageType + 1).ToListAsync();
            extraIdList = extraIdList ?? new List<int>();
            premiumPackets.RemoveAll(e => !extraIdList.Contains(e.Productid) && e.VendorExtraExists == true);
            if (premiumPackets == null || !premiumPackets.Any())
                return new List<Extra>();

            bool isMandatoryOptional = true;
            Locationvendor vendorLocation = null;

            if (!isMandatoryOptional)
            {
                vendorLocation = await _context.Locationvendor.FirstOrDefaultAsync(lv =>
                    lv.Vendorid == token.VendorId &&
                    lv.Locallocationid == token.PickupLocationId);
            }

            if (agency.UserRole == UserRoles.External)
                premiumPackets.RemoveAll(e => e.VendorExtraExists != true);

            var premiumExtras = new List<Extra>();
            foreach (var premiumPacket in premiumPackets.Where(p => p?.Productid > 0))
            {
                var packetPrice = agency.AgencyCommissionAmount > 0
                    ? (float)(premiumPacket.Defaultprice ?? 0) *
                      (1 + agency.AgencyCommissionAmount / 100f)
                    : (float)(premiumPacket.Defaultprice ?? 0);

                var currency = await _context.Currency
                    .FirstOrDefaultAsync(c => c.Currencyid == premiumPacket.CurrencyId);

                premiumExtras.Add(new Extra
                {
                    ExtraId = premiumPacket.Productid,
                    ExtraCode = premiumPacket.Productcode,
                    ExtraName = premiumPacket.Productname,
                    ExtraDescription = premiumPacket.Productdescription,
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraType = AdditionalProductTypes.Premium,
                    ExtraQuantityIncreasable = false,
                    Price = (float)premiumPacket.Defaultprice,
                    Icon = premiumPacket.Iconpath,
                    CurrencyCode = currency?.Currencyisocode,
                    VendorId = token.VendorId,
                    VendorName = token.APIVendorName,
                    ShowDayCountStart = premiumPacket.Showdaycountstart,
                    ShowDayCountEnd = premiumPacket.Showdaycountend,
                    Sequence = premiumPacket.Sequence ?? 1,
                    IsRequired = false,
                    VendorExtraExists = premiumPacket.VendorExtraExists ?? false,
                    DefaultPrice = (float)premiumPacket.Defaultprice

                });
            }
            return premiumExtras;
        }

        //public async Task<ServiceResponseBase> GetSpecialProductsByLocationVendor(int vendorId, int productId)
        //{
        //    var specialExtraList = await (from lv in _context.Locationvendor
        //                                  join l in _context.Location on lv.Locallocationid equals l.Id
        //                                  join x in _context.SpecialProductPrices.Where(x => x.AdditionalProductId == productId && x.VendorId == vendorId) on lv.Locallocationid equals x.LocationId into gj
        //                                  from spp in gj.DefaultIfEmpty()
        //                                  join apv in _context.Additionalproductvendor.Where(x => x.Vendorid == vendorId && x.Productid == productId) on lv.Vendorid equals apv.Vendorid
        //                                  where lv.Vendorid == vendorId && lv.SpecialVendorRequirement && l.Langid == 1
        //                                  select new CommonModels.SpecialProductPrice
        //                                  {
        //                                      Id = spp.Id,
        //                                      LocationId = lv.Locallocationid,
        //                                      VendorId = lv.Vendorid.ToIntNullSafe(),
        //                                      LocationName = l.Locationname,
        //                                      ApiProductName = apv.Apiproductname,
        //                                      ApiProductCode = apv.Apiproductcode,
        //                                      AdditionalProductId = spp.AdditionalProductId,
        //                                      Price = spp.Price,
        //                                      CurrencyId = spp.CurrencyId
        //                                  }
        //                                  ).ToListAsync();

        //    if (specialExtraList != null && specialExtraList.Count > 0)
        //        return new ServiceResponseBase(specialExtraList, true);

        //    return new ServiceResponseBase(null, false, "there is no special extra in this location.");
        //}

        public async Task<List<Extra>> GetActiveLocalExtras(LanguageTypes languageTypes)
        {
            return await _cacheService.GetOrCreateAsync($"GetActiveLocalExtras-{languageTypes}", async () =>
            {
                return _context.Additionalproduct.Where(e => e.Langid == (int)languageTypes + 1 && e.Active).ToList().Map();
            }, TimeSpan.FromDays(1));
        }

        public async Task<Extra> GetPremiumPacketByReservationToken(ReservationToken reservationToken)
        {
            var langId = reservationToken.LanguageType;

            var additionalInformation = new ResponseReservationStepsAdditionalInformation();

            additionalInformation.Vendor = new CommonModels.Vendor();
            additionalInformation.Vendor.VendorId = reservationToken.VendorId;

            additionalInformation.Agency = new CommonModels.Agency();
            additionalInformation.Agency.AgencyId = reservationToken.AgencyId;

            additionalInformation.PickupDateTime = reservationToken.PickupDateTime;
            additionalInformation.RentalDuration = reservationToken.RentalDuration;
            additionalInformation.PickupLocationId = reservationToken.PickupLocationId;

            var specialRequests = await _specialRequestService.GetSpecialRequestByFilter(additionalInformation, reservationToken.DailyPrice, (int)reservationToken.VehicleCategoryType);

            var premiumPackets = await _additionalProductService.GetPremiumPackets(specialRequests, reservationToken, additionalInformation.RentalDuration, (int)langId);

            return premiumPackets.FirstOrDefault(e => e.ShowInVehicleList);
        }
    }
}
