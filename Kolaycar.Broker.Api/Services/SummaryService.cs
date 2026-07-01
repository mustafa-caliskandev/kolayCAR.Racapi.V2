using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AkkorProvider = KolayCAR.Broker.API.Providers.Akkor;
using AssistProvider = KolayCAR.Broker.API.Providers.Assist;
using Avec2Provider = KolayCAR.Broker.API.Providers.Avec2;
using Avec3Provider = KolayCAR.Broker.API.Providers.Avec3;
using AvecProvider = KolayCAR.Broker.API.Providers.Avec;
using AvisProvider = KolayCAR.Broker.API.Providers.Avis;
using AyesProvider = KolayCAR.Broker.API.Providers.Ayes;
using AytuProvider = KolayCAR.Broker.API.Providers.Aytu;
using BesSProvider = KolayCAR.Broker.API.Providers._5S;
using BetoProvider = KolayCAR.Broker.API.Providers.Beto;
using Central2Provider = KolayCAR.Broker.API.Providers.Central2;
using CentralProvider = KolayCAR.Broker.API.Providers.Central;
using Circular2Provider = KolayCAR.Broker.API.Providers.Circular2;
using CircularProvider = KolayCAR.Broker.API.Providers.Circular;
using CizgiProvider = KolayCAR.Broker.API.Providers.Cizgi;
using CommonModels = KolayCAR.Broker.Domain.Models;
using CredyCarProvider = KolayCAR.Broker.API.Providers.CredyCar;
using DailydriveProvider = KolayCAR.Broker.API.Providers.Dailydrive;
using Ekar2Provider = KolayCAR.Broker.API.Providers.Ekar2;
using EkarProvider = KolayCAR.Broker.API.Providers.Ekar;
using ElitcarProvider = KolayCAR.Broker.API.Providers.Elitcar;
using ErboycarProvider = KolayCAR.Broker.API.Providers.Erboycar;
using EuropcarProvider = KolayCAR.Broker.API.Providers.Europcar;
using FiloNovaProvider = KolayCAR.Broker.API.Providers.FiloNova;
using GarentaProvider = KolayCAR.Broker.API.Providers.Garenta;
using GreenMotionProvider = KolayCAR.Broker.API.Providers.GreenMotion;
using HaraProvider = KolayCAR.Broker.API.Providers.Hara;
using KolayCARBrokerProvider = KolayCAR.Broker.API.Providers.KolayCARBroker;
using KolayCARProvider = KolayCAR.Broker.API.Providers.KolayCAR;
using Nissa2Provider = KolayCAR.Broker.API.Providers.Nissa2;
using NissaProvider = KolayCAR.Broker.API.Providers.Nissa;
using OtocarProvider = KolayCAR.Broker.API.Providers.Otocar;
using OtorentoProvider = KolayCAR.Broker.API.Providers.Otorento;
using PandoraProvider = KolayCAR.Broker.API.Providers.Pandora;
using Pandora2Provider = KolayCAR.Broker.API.Providers.Pandora2;
using ReservawayProvider = KolayCAR.Broker.API.Providers.Reservaway;
using RenticarProvider = KolayCAR.Broker.API.Providers.Renticar;
using RigorentProvider = KolayCAR.Broker.API.Providers.Rigorent;
using SixtProvider = KolayCAR.Broker.API.Providers.Sixt;
using Turevrac2Provider = KolayCAR.Broker.API.Providers.Turevrac2;
using TurevracProvider = KolayCAR.Broker.API.Providers.Turevrac;
using TurmobilProvider = KolayCAR.Broker.API.Providers.Turmobil;
using VonarentProvider = KolayCAR.Broker.API.Providers.Vonarent;
using WishcarProvider = KolayCAR.Broker.API.Providers.Wishcar;
using Yolcu360Provider = KolayCAR.Broker.API.Providers.Yolcu360;

namespace KolayCAR.Broker.API.Services
{
    public interface ISummaryService
    {
        Task<ServiceResponseBase> GetSummary(GetSummaryRequest getSummaryRequest, GetExtrasResponse getExtrasResponse);
    }
    public class SummaryService : ISummaryService
    {
        private readonly BrokerContext _context;
        private readonly IVendorService _vendorService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IExtraService _extraService;
        private readonly IVehicleService _vehicleService;
        private readonly IAgencyService _agencyService;
        private readonly ICouponService _couponService;
        private readonly IConfigurationService _configurationService;
        private readonly IMemoryCache _memoryCache;
        private readonly IResTokenService _resTokenService;

        public SummaryService(IOptions<AppSettings> appSettings, BrokerContext context, IConfigurationService configurationService, IAgencyService agencyService, ICouponService couponService, IVendorService vendorService, IReservationStepsService reservationStepsService, IExtraService extraService, IVehicleService vehicleService, IMemoryCache memoryCache, IResTokenService resTokenService)
        {
            _context = context;
            _vendorService = vendorService;
            _reservationStepsService = reservationStepsService;
            _agencyService = agencyService;
            _couponService = couponService;
            _extraService = extraService;
            _vehicleService = vehicleService;
            _configurationService = configurationService;
            _memoryCache = memoryCache;
            _resTokenService = resTokenService;
        }

        public async Task<ServiceResponseBase> GetSummary(GetSummaryRequest getSummaryRequest, GetExtrasResponse getExtrasResponse)
        {
            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(getSummaryRequest.ReservationToken);
            if (reservationToken != null)
            {
                var agency = await _agencyService.GetAgency(reservationToken.AgencyId.ToIntNullSafe());

                if (agency != null)
                {
                    if (await _vendorService.GetVendorById(reservationToken.VendorId, agency, subVendorId: reservationToken.APIVendorId) is CommonModels.Vendor vendor && vendor != null)
                    {
                        ReservationHelper.FillGetSummaryRequest(getSummaryRequest, reservationToken);

                        await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                        ISummaryProvider summaryProvider;
                        var subVendorsDb = await _context.Subvendor.ToListAsync();
                        var subVendors = subVendorsDb.Map();
                        var vendorLocation = await _context.Locationvendor
                            .Where(x => x.Locallocationid == getSummaryRequest.PickupLocationId &&
                            x.Active == true &&
                            x.Vendorid == vendor.VendorId)
                            .FirstOrDefaultAsync();

                        var localVehicles = await _vehicleService.GetLocalVehicleClassesByVendorId(vendor.VendorId, (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getSummaryRequest.LanguageCode));
                        var exchangeRates = await _context.Exchangerates.ToListAsync();
                        var mappedExchangeRates = exchangeRates.Map();

                        getSummaryRequest.ApiKey = vendor.ApiKey;
                        getSummaryRequest.ApiPassword = vendor.ApiPassword;
                        getSummaryRequest.VendorType = vendor.VendorType;

                        var useMappedExtras = vendor.ExtraMappingActive &&
                            (vendor.VendorType != VendorTypes.KolayCARBroker ||
                            (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations));

                        var selectedReservationExtras = ReservationHelper.GetSelectedReservationExtras(getExtrasResponse.Extras, getSummaryRequest.ExtraList);
                        getSummaryRequest.ExtraList = useMappedExtras
                            ? ReservationHelper.ReservationExtraToStringList(selectedReservationExtras)
                            : ReservationHelper.ReservationExtraToApiStringList(selectedReservationExtras);

                        if (useMappedExtras && !string.IsNullOrEmpty(getSummaryRequest.ExtraList))
                        {
                            var vendorExtras = await _context.Additionalproductvendor.Where(x =>
                            x.Vendorid == vendor.VendorId &&
                            (x.Apivendorid == reservationToken.APIVendorId || vendor.VendorType == VendorTypes.Yolcu360) &&
                            x.Active == true).ToListAsync();
                            var premiumPack = _context.Additionalproduct.FirstOrDefaultAsync(ap => ap.Producttype == 5).Result;
                            var selectedExtras = new List<Extra>();
                            string extraListStrToVendorAPI = string.Empty;
                            foreach (var extraListItem in getSummaryRequest.ExtraList.Split('|'))
                            {
                                var extraProps = extraListItem.Split('~');
                                var extraCode = extraProps[4];
                                var dbExtra = vendorExtras.Where(x => x.Apiproductcode == extraCode).FirstOrDefault();
                                //selectedExtras.Add(dbExtra);
                                var newApiExtraList = "";
                                if (premiumPack != null)
                                {
                                    if (premiumPack != null && extraProps[0].ToIntNullSafe() == premiumPack.Productid)
                                    {
                                        //newApiExtraList = $"{premiumPack.Productcode}~{extraProps[1]}~{extraProps[2]}~{extraProps[3]}~{premiumPack.Productid}~{extraProps[5]}";
                                    }
                                }
                                else
                                {
                                    newApiExtraList = $"{dbExtra.Apiproductcode}~{extraProps[1]}~{extraProps[2]}~{extraProps[3]}~{dbExtra.Productid}~{extraProps[5]}";
                                }

                                extraListStrToVendorAPI += newApiExtraList + "|";
                            }
                            extraListStrToVendorAPI = StringHelper.LastCharacterClear(extraListStrToVendorAPI, "|");
                            getSummaryRequest.ExtraList = extraListStrToVendorAPI;
                        }

                        if (await _reservationStepsService.GetAdditionalInformation(
                            vendor,
                            agency,
                            getSummaryRequest.LanguageCode,
                            getSummaryRequest.CurrencyCode,
                            vendor.CurrencyType,
                            getSummaryRequest.PickupLocationId,
                            getSummaryRequest.ReturnLocationId,
                            getSummaryRequest.PickupDate,
                            getSummaryRequest.ReturnDate,
                            getSummaryRequest.PickupTime,
                            getSummaryRequest.ReturnTime,
                            vehicleId: reservationToken.VehicleId,
                            apiVendorId: reservationToken.APIVendorId,
                            rentalDuration: reservationToken.RentalDuration,
                            reservationToken: reservationToken) is ResponseReservationStepsAdditionalInformation additionalInformation && additionalInformation != null)
                        {
                            switch (vendor.VendorType)
                            {
                                default:
                                    return new ServiceResponseBase
                                    {
                                        Success = false,
                                        Message = "Check your ReservationToken!",
                                        ServiceCode = null,
                                        ServiceMessage = null
                                    };
                                case VendorTypes.KolayCAR:
                                    {
                                        summaryProvider = new KolayCARProvider.SummaryProvider(vendor);
                                        break;
                                    }
                                case VendorTypes.Avec:
                                    {
                                        summaryProvider = new AvecProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.KolayCARBroker:
                                    {
                                        summaryProvider = new KolayCARBrokerProvider.SummaryProvider(vendor.APIBaseUrl);
                                        break;
                                    }
                                case VendorTypes.Aytu:
                                    {
                                        summaryProvider = new AytuProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Europcar:
                                    {
                                        summaryProvider = new EuropcarProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Pandora:
                                    {
                                        summaryProvider = new PandoraProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Pandora2:
                                    {
                                        summaryProvider = new Pandora2Provider.SummaryProvider(vendor.APIBaseUrl);
                                        break;
                                    }
                                case VendorTypes.Central:
                                    {
                                        summaryProvider = new CentralProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Nissa:
                                    {
                                        summaryProvider = new NissaProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Ayes:
                                    {
                                        summaryProvider = new AyesProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Rigorent:
                                    {
                                        summaryProvider = new RigorentProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.GreenMotion:
                                    {
                                        summaryProvider = new GreenMotionProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Ekar:
                                    {
                                        summaryProvider = new EkarProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Hara:
                                    {
                                        summaryProvider = new HaraProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.FiloNova:
                                    {
                                        summaryProvider = new FiloNovaProvider.SummaryProvider(_memoryCache);
                                        break;
                                    }
                                case VendorTypes.Elitcar:
                                    {
                                        summaryProvider = new ElitcarProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Cizgi:
                                    {
                                        summaryProvider = new CizgiProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Avec2:
                                    {
                                        summaryProvider = new Avec2Provider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Wishcar:
                                    {
                                        summaryProvider = new WishcarProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Otocar:
                                    {
                                        summaryProvider = new OtocarProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Assist:
                                    {
                                        summaryProvider = new AssistProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Garenta:
                                    {
                                        summaryProvider = new GarentaProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.CredyCar:
                                    {
                                        summaryProvider = new CredyCarProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Erboycar:
                                    {
                                        summaryProvider = new ErboycarProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Circular:
                                    {
                                        summaryProvider = new CircularProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Akkor:
                                    {
                                        summaryProvider = new AkkorProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Turmobil:
                                    {
                                        summaryProvider = new TurmobilProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Dailydrive:
                                    {
                                        summaryProvider = new DailydriveProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Yolcu360:
                                    {
                                        summaryProvider = new Yolcu360Provider.SummaryProvider(_memoryCache);
                                        break;
                                    }
                                case VendorTypes.BesS:
                                    {
                                        summaryProvider = new BesSProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Central2:
                                    {
                                        summaryProvider = new Central2Provider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Beto:
                                    {
                                        summaryProvider = new BetoProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Turevrac:
                                    {
                                        summaryProvider = new TurevracProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Renticar:
                                    {
                                        summaryProvider = new RenticarProvider.SummaryProvider(_memoryCache);
                                        break;
                                    }
                                case VendorTypes.Sixt:
                                    {
                                        summaryProvider = new SixtProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Nissa2:
                                    {
                                        summaryProvider = new Nissa2Provider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Avec3:
                                    {
                                        summaryProvider = new Avec3Provider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Ekar2:
                                    {
                                        summaryProvider = new Ekar2Provider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Circular2:
                                    {
                                        summaryProvider = new Circular2Provider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Otorento:
                                    {
                                        summaryProvider = new OtorentoProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Turevrac2:
                                    {
                                        summaryProvider = new Turevrac2Provider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Avis:
                                    {
                                        summaryProvider = new AvisProvider.SummaryProvider();
                                        break;
                                    }
                                case VendorTypes.Vonarent:
                                    {
                                        summaryProvider = new VonarentProvider.SummaryProvider(vendor);
                                        break;
                                    }
                                case VendorTypes.Reservaway:
                                    {
                                        summaryProvider = new ReservawayProvider.SummaryProvider(vendor);
                                        break;
                                    }
                            }

                            var summaryResult = summaryProvider.GetSummary(getSummaryRequest, vendor, additionalInformation, mappedExchangeRates, localVehicles, subVendors).Result;

                            if (summaryResult.Success)
                            {
                                if (summaryResult.Data != null)
                                {

                                    var summaryData = summaryResult.Data as GetSummaryResponse;
                                    var configurations = await _configurationService.GetConfigurations();

                                    //#region Özel ek ürünü servisten dönmüş gibi gösterme işlemi
                                    //if (configurations.SpecialExtrasIsActive || getExtrasResponse.Extras.Where(x => x.ExtraType == AdditionalProductTypes.Premium).Any())
                                    //{
                                    //    float extraPrice = 0;
                                    //    if (getExtrasResponse.Extras.Any(x => x.ExtraType == AdditionalProductTypes.Premium))
                                    //    {
                                    //        var premiumPack = _context.Additionalproduct.FirstOrDefaultAsync(ap => ap.Producttype == 5).Result;
                                    //        extraPrice = agency.AgencyCommissionAmount > 0
                                    //    ? (float)(premiumPack.Defaultprice ?? 0) - ((float)(premiumPack.Defaultprice ?? 0) * agency.AgencyCommissionAmount / 100)
                                    //    : (float)(premiumPack.Defaultprice ?? 0);
                                    //    }
                                    //    List<ReservationExtra> specialExtras = getExtrasResponse.Extras.Select(x => new ReservationExtra
                                    //    {
                                    //        APIPrice = 0,
                                    //        ExtraCode = x.ExtraCode,
                                    //        ExtraDescription = x.ExtraDescription,
                                    //        ExtraId = x.ExtraId,
                                    //        ExtraName = x.ExtraName,
                                    //        ExtraQuantityIncreasable = x.ExtraQuantityIncreasable,
                                    //        ExtraRentalType = x.ExtraRentalType,
                                    //        ExtraType = x.ExtraType,
                                    //        Icon = x.Icon,
                                    //        Label = x.Label,
                                    //        Piece = 1,
                                    //        Price = x.ExtraType == AdditionalProductTypes.Premium ? extraPrice : x.Price,
                                    //        Sequence = x.Sequence,
                                    //        VendorId = x.VendorId,
                                    //        VendorName = x.VendorName
                                    //    }).Where(x => x.ExtraType == AdditionalProductTypes.Compulsory || x.ExtraType == AdditionalProductTypes.Premium).ToList();

                                    //    summaryData.Extras.AddRange(specialExtras);
                                    //}
                                    //#endregion

                                    //var specialExtras = getExtrasResponse.Extras.Select(m => new List<ReservationExtra> {

                                    //}).Where(x => x.ExtraType == AdditionalProductTypes.Compulsory).ToList();
                                    //summaryData.Extras.AddRange();

                                    agency.AdditionalProductAmountDeliveryPayment = summaryData.Vehicle.IsAdditionalProductPricePOA ?? agency.AdditionalProductAmountDeliveryPayment;
                                    agency.OneWayAmountDeliveryPayment = summaryData.Vehicle.IsOneWayFeePOA ?? agency.OneWayAmountDeliveryPayment;

                                    VehicleHelper.SetVehiclePropertyBySIPPCode(summaryData.Vehicle);
                                    _agencyService.SetVehiclePaymentOptions(summaryData.Vehicle, agency);
                                    summaryData.Vehicle.ActivePaymentTypes = summaryData.Vehicle.ActivePaymentTypes.Distinct().ToList();
                                    var vehicleCategories = await _context.Vehiclecategorylang.ToListAsync();
                                    var vehicleFuels = await _context.Vehiclefuellang.ToListAsync();
                                    var vehicleTransmissions = await _context.Vehicletransmissionlang.ToListAsync();
                                    var vehicleTypes = await _context.Vehicletypelang.ToListAsync();
                                    var pickupLocation = await _context.Location.Where(x => x.Id == reservationToken.PickupLocationId).FirstOrDefaultAsync();
                                    //var configurations = await _configurationService.GetConfigurations();

                                    summaryData.Vehicle = VehicleHelper.MapVehicleProp(
                                        vehicleCategories,
                                        vehicleFuels,
                                        vehicleTransmissions,
                                        vehicleTypes,
                                        summaryData.Vehicle,
                                        (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getSummaryRequest.LanguageCode));

                                    List<RentalCondition> rentalConditionsData = new List<RentalCondition>();
                                    if (vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations))
                                    {
                                        var rentalConditionsResult = await _vehicleService.GetRentalConditions(vendor.VendorId, (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getSummaryRequest.LanguageCode));
                                        rentalConditionsData = rentalConditionsResult.Success ? rentalConditionsResult.Data as List<RentalCondition> : null;
                                    }

                                    summaryData.Vehicle.RentalConditions = rentalConditionsData;
                                    summaryData.Vehicle.ReservationToken = getSummaryRequest.ReservationToken;
                                    summaryData.Vehicle.CreditType = CreditHelper.ResolveTokenCreditType(reservationToken);
                                    summaryData.Vehicle.FullCredit = summaryData.Vehicle.CreditType == CreditType.FullCredit;
                                    summaryData.Vehicle.IsAirport = pickupLocation.Airport ?? false;
                                    summaryData.Vehicle.VendorLogo = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? $"{configurations.PortalOwnerDomain}{summaryData.Vehicle.VendorLogo}" : summaryData.Vehicle.VendorLogo;
                                    summaryData.Vehicle.CurrencyCode = getSummaryRequest.CurrencyCode;
                                    summaryData.Vehicle.IsOffice = vendor.VendorType != VendorTypes.KolayCARBroker || (vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations) ? vendorLocation.Isoffice ?? false : summaryData.Vehicle.IsOffice;
                                    summaryData.Vehicle.IsAdditionalProductPricePOA = agency.AdditionalProductAmountDeliveryPayment;
                                    summaryData.Vehicle.IsOneWayFeePOA = agency.OneWayAmountDeliveryPayment;

                                    var apiSummaryData = summaryResult.Data as GetSummaryResponse;

                                    if (useMappedExtras)
                                    {
                                        var localExtrasResult = await _extraService.GetMappedExtras(
                                            vendor.VendorId,
                                            (CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), getSummaryRequest.CurrencyCode),
                                            (LanguageTypes)Enum.Parse(typeof(LanguageTypes), getSummaryRequest.LanguageCode),
                                            reservationToken.RentalDuration,
                                            apiVendorId: reservationToken.APIVendorId);
                                        var localExtras = vendor.VendorType != VendorTypes.Yolcu360 ?
                                            await _context.Additionalproductvendor.Where(x =>
                                            x.Vendorid == vendor.VendorId &&
                                            x.Apivendorid == reservationToken.APIVendorId &&
                                            x.Active == true).ToListAsync() :
                                            await _context.Additionalproductvendor.Where(x =>
                                            x.Vendorid == vendor.VendorId &&
                                            //x.Apivendorid == reservationToken.APIVendorId &&
                                            x.Active == true).ToListAsync();

                                        var sourceExtraList = !agency.FreePriceShowActive ? apiSummaryData.Extras : ReservationHelper.GetReservationExtrasFromStringList(getSummaryRequest.ExtraList);

                                        var localExtrasData = localExtrasResult.Data as List<Extra>;
                                        foreach (var apiExtra in sourceExtraList)
                                        {
                                            foreach (var localExtra in localExtrasData)
                                            {
                                                if (localExtra.ExtraCode == apiExtra.ExtraId.ToString() && vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations ||
                                                    localExtra.ExtraCode == apiExtra.ExtraCode.ToString())
                                                {
                                                    //Local gün aralıkları girilmemiş, fiyatlar karşı API'dan çekilecek || Özel fiyat açık ise requestte gönderilen ek ürün fiyatları kullanılır
                                                    if (localExtrasResult.ServiceCode == "-1" || agency.FreePriceShowActive)
                                                    {
                                                        localExtra.Price = apiExtra.Price;
                                                    }

                                                    localExtra.ExtraQuantityIncreasable = apiExtra.ExtraQuantityIncreasable;
                                                    localExtra.ExtraRentalType = apiExtra.ExtraRentalType;
                                                }
                                            }
                                        }

                                        //else //Özel fiyat açık ise requestte gönderilen ek ürün fiyatları kullanılır
                                        //{
                                        //    var requestExtraList = ReservationHelper.GetReservationExtrasFromStringList(getSummaryRequest.ExtraList);
                                        //    var localExtrasData = localExtrasResult.Data as List<Extra>;
                                        //    foreach (var apiExtra in requestExtraList)
                                        //    {
                                        //        foreach (var localExtra in localExtrasData)
                                        //        {
                                        //            if (localExtra.ExtraCode == apiExtra.ExtraCode.ToString())
                                        //            {
                                        //                localExtra.Price = apiExtra.Price;
                                        //                localExtra.ExtraQuantityIncreasable = apiExtra.ExtraQuantityIncreasable;
                                        //                localExtra.ExtraRentalType = apiExtra.ExtraRentalType;
                                        //            }
                                        //        }
                                        //    }
                                        //}

                                        List<ReservationExtra> reservationExtras = new List<ReservationExtra>();
                                        List<Extra> mappedLocalExtras = new List<Extra>();
                                        if (localExtrasResult.Success)
                                        {
                                            mappedLocalExtras = localExtrasResult.Data as List<Extra>;

                                            foreach (var item in apiSummaryData.Extras)
                                            {
                                                if (apiSummaryData.Extras.Where(x => x.ExtraCode == item.ExtraCode).ToList().Count > 1)
                                                {
                                                    apiSummaryData.Extras.Remove(item);
                                                    break;
                                                }
                                            }

                                            foreach (var mappedLocalExtra in mappedLocalExtras)
                                            {

                                                foreach (var apiExtra in apiSummaryData.Extras)
                                                {
                                                    string apiProductCode = localExtras.Where(x => x.Apiproductcode == mappedLocalExtra.ExtraCode && x.Vendorid == vendor.VendorId && x.Active == true).FirstOrDefault().Apiproductcode;

                                                    if (apiProductCode == apiExtra.ExtraId.ToString() && vendor.VendorType == VendorTypes.KolayCARBroker && !vendor.UseBrokerConfigurations || apiProductCode == apiExtra.ExtraCode.ToString())
                                                    {
                                                        reservationExtras.Add(ReservationHelper.ExtraToReservationExtra(mappedLocalExtra, apiExtra.Piece));
                                                    }
                                                }
                                            }
                                        }

                                        apiSummaryData.Vehicle.ExtraPrice = CalculationHelper.RoundPrice(reservationExtras.Sum(x => x.Price * x.Piece * (x.ExtraRentalType == ExtraRentalTypes.Daily ? reservationToken.RentalDuration : 1)), (int)vendor.PriceRoundingType);
                                        apiSummaryData.Extras = reservationExtras;
                                    }
                                    else
                                    {
                                        apiSummaryData.Vehicle.ExtraPrice = CalculationHelper.RoundPrice(summaryData.Extras.Sum(x => x.Price * x.Piece * (x.ExtraRentalType == ExtraRentalTypes.Daily ? reservationToken.RentalDuration : 1)), (int)vendor.PriceRoundingType);
                                    }

                                    apiSummaryData.Vehicle.TotalPrice += apiSummaryData.Vehicle.ExtraPrice;
                                    apiSummaryData.Vehicle.TotalPrice = CalculationHelper.RoundPrice(apiSummaryData.Vehicle.TotalPrice, (int)vendor.PriceRoundingType);

                                    apiSummaryData.Vehicle.TotalPricePayNow += apiSummaryData.Vehicle.ExtraPrice;
                                    apiSummaryData.Vehicle.TotalPricePayNow = CalculationHelper.RoundPrice(apiSummaryData.Vehicle.TotalPricePayNow, (int)vendor.PriceRoundingType);

                                    apiSummaryData.Vehicle.Extras = null;


                                    if (!string.IsNullOrEmpty(getSummaryRequest.CouponCode))
                                    {
                                        var applyCouponCodeResponse = await _couponService.ApplyCouponCode(
                                            getSummaryRequest.MemberId ?? 0,
                                            getSummaryRequest.CouponCode,
                                            apiSummaryData.Vehicle,
                                            vendor,
                                            getSummaryRequest.CurrencyCode.ToEnum<CurrencyTypes>(),
                                            getSummaryRequest.PickupDate.ToDateTimeNullSafe(),
                                            getSummaryRequest.ReturnDate.ToDateTimeNullSafe(),
                                            dailyPrice: reservationToken.DailyPrice.ToDecimalNullSafe(),
                                            paidAmount: getSummaryRequest.PaidAmount,
                                            highAmountDiscountActive: getSummaryRequest.HighAmountDiscountActive,
                                            getSummaryRequest, apiSummaryData, reservationToken);

                                        apiSummaryData.Vehicle = applyCouponCodeResponse.Vehicle;
                                        apiSummaryData.CouponUsageResultType = applyCouponCodeResponse.CouponUsageResultType;
                                    }

                                    return summaryResult;
                                }
                            }

                            return new ServiceResponseBase
                            {
                                Success = false,
                                Message = "Summary sorgusu başarısız!",
                                ServiceCode = null,
                                ServiceMessage = null
                            };
                        }
                    }
                    else
                        return new ServiceResponseBase
                        {
                            Success = false,
                            Message = "Tedarikçi bilgisine ulaşılamadı!",
                            ServiceCode = null,
                            ServiceMessage = null
                        };
                }
                else
                    return new ServiceResponseBase
                    {
                        Success = false,
                        Message = "Acente bilgisine ulaşılamadı!",
                        ServiceCode = null,
                        ServiceMessage = null
                    };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Bilgileri kontrol edip tekrar deneyin!",
                ServiceCode = null,
                ServiceMessage = null
            };
        }
    }
}
