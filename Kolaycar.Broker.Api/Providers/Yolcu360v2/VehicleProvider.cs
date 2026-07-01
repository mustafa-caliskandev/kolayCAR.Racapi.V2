using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Yolcu360v2;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.Yolcu360v2RequestBase;
using static KolayCAR.Broker.Domain.Models.Response.Yolcu360v2.Yolcu360v2ResponseBaseDeleted;

namespace KolayCAR.Broker.API.Providers.Yolcu360v2
{
    public class VehicleProvider : IVehicleProvider
    {
        private readonly AuthProvider _authProvider;
        private readonly HttpManager _httpManager;
        private readonly ICacheService _cacheService;
        public VehicleProvider(Vendor vendor, bool disableTimeout, ICacheService cacheService)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl, cacheService);
            _cacheService = cacheService;
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            return new(null, false, $"{vendor.VendorName} tedarikçisine ait araç listesi yok!");
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var token = await _authProvider.GetToken(vendor);
            if (token == null) return new(null, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var result = await _httpManager.PostAsyncWithModel<Yolcu360v2SearchRequest, Yolcu360v2VehicleResponseBase.Root>(
              requestPath: "/api/v1/search/point",
              entity: GetEntity(additionalInformation, vendor),
              headers: GetHeaders(token, baseVendorRequestCurrencyType));

            if (result == null || result?.count == 0)
                return new(null, false, $"{vendor.VendorName} müsait araç bulunamadı!");

            var apiVehicleList = result.results;
            var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var apiVendorList = apiVehicleList.Select(e => new VendorVendor(vendor.VendorId, true, e.vendor.name.ToLowerInvariant()))
                    .GroupBy(e => e.VendorName).Select(e => e.First()).Where(e => vendor.VendorVendors?.Any(v => v.VendorName == e.VendorName) != true).ToList();

            var vendorVendorByName = vendor.VendorVendors?
                .Where(e => e?.Active == true && !string.IsNullOrWhiteSpace(e.VendorName))
                .GroupBy(e => e.VendorName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(e => e.Key, e => e.First(), StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, VendorVendor>(StringComparer.OrdinalIgnoreCase);

            var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor, baseVendorRequestCurrencyType, exchangeRates, vendorVendorByName);

            CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
            VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

            if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

            var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
            var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
            var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

            foreach (var vehicle in apiVehicleList)
            {
                var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.code).ToList();

                for (int i = 0; i < tempMappedVehicleList.Count; i++)
                {
                    var mappedVehicle = tempMappedVehicleList[i];
                    var apiVendorName = vehicle?.vendor?.name?.Trim();
                    var passportRequired = !string.IsNullOrWhiteSpace(apiVendorName)
                        && vendorVendorByName.TryGetValue(apiVendorName, out var vendorVendor)
                        && vendorVendor.PassportNumberRequired.ToBoolNullSafe() == true;

                    mappedVehicle.PassportRequired = passportRequired;

                    var reservationToken = new ReservationToken
                    {
                        AgencyId = additionalInformation.Agency.AgencyId,
                        VendorId = vendor.VendorId,
                        //APIVendorId = vendor.VendorId,
                        APIVendorName = mappedVehicle.VendorName,
                        APIVendorPhone = vendor.VendorPhone,
                        APIVendorEmail = vendor.VendorEmail,
                        APIVendorLogo = mappedVehicle.VendorLogo,
                        VehicleId = mappedVehicle.VehicleId,
                        VehicleCode = mappedVehicle.VehicleCode,
                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                        CurrencyType = requestCurrencyType,
                        RentalDuration = mappedVehicle.RentalDuration,
                        DailyPrice = mappedVehicle.DailyPrice,
                        OneWayFee = mappedVehicle.OneWayFee,
                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                        APIDailyPrice = (vehicle.pricing.paymentTotal.amount / 100f / vehicle.rentalDurationInDays).ToFloatNullSafe(),
                        APIDailyPricePayNow = (vehicle.pricing.paymentTotal.amount / 100f / vehicle.rentalDurationInDays).ToFloatNullSafe(),
                        APIOneWayFee = (vehicle.pricing.prices.FirstOrDefault(e => e.type == "oneWayFee")?.amount?.amount / 100f).ToFloatNullSafe() + (vehicle.pricing.prices.FirstOrDefault(e => e.type == "deliveryFee")?.amount?.amount / 100f).ToFloatNullSafe(),
                        DepositPrice = mappedVehicle.DepositPrice,
                        VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                        VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                        ServiceCharge = mappedVehicle.ServiceCharge,
                        FuelType = mappedVehicle.FuelType,
                        TransmissionType = mappedVehicle.TransmissionType,
                        DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                        PickupLocationId = getVehiclesRequest.PickupLocationId,
                        ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                        LanguageType = languageType,
                        PickupDateTime = pickupDateTime,
                        ReturnDateTime = returnDateTime,
                        VehicleName = mappedVehicle.VehicleName,
                        VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                        BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                        APIReferenceCode = vehicle.searchID,
                        BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                        PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                        TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                        VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                        VehicleType = mappedVehicle.VehicleType,
                        VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
                        FullCredit = mappedVehicle.FullCredit,
                        CreditType = mappedVehicle.CreditType,
                        APICreditType = mappedVehicle.CreditType,
                        APIFullCredit = mappedVehicle.FullCredit,
                        SippCode = mappedVehicle.SippCode,
                        APIDeliveryTypeId = mappedVehicle.ApiDeliveryTypeId,
                        APIReferenceCode2 = mappedVehicle.PassportRequired.ToStringNullSafe()
                    };

                    mappedVehicle.ReservationToken = reservationToken.ToJson();

                    if (additionalInformation.Agency.SpecialParameters)
                    {
                        mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                        mappedVehicle.SpecialVendorName = vendor.VendorName;
                        mappedVehicle.SpecialVendorLogo = vendor.Logo;
                    }
                }
            }
            if (mappedVehicleList.Count > 0 && vendor.VendorVendors?.Count > 0)
            {
                var blockedVendorNames = await _cacheService.GetOrCreateAsync($"{CacheSettings.VendorKey}-VendorVendorNameList-{vendor.VendorId}", async () =>
                {
                    return vendor.VendorVendors.Where(e => e.Active == false && e.VendorId == vendor.VendorId).Select(v => v.VendorName.ToLowerInvariant()).ToList();
                }, TimeSpan.FromHours(1));

                mappedVehicleList.RemoveAll(mv => blockedVendorNames.Contains(mv.VendorName.ToLowerInvariant()));
            }
            return new(mappedVehicleList, mappedVehicleList.Count > 0, data2: apiVendorList);
        }

        private IDictionary<string, object> GetHeaders(IDictionary<string, object> token, CurrencyTypes baseVendorRequestCurrencyType)
        {
            var headers = new Dictionary<string, object>(token ?? new Dictionary<string, object>());

            headers["X-Currency"] = GetYolcu360v2CurrencyTypes(baseVendorRequestCurrencyType.ToString());

            return headers;
        }

        private string GetYolcu360v2CurrencyTypes(string baseVendorRequestCurrencyType) => baseVendorRequestCurrencyType switch
        {
            "TRY" => Yolcu360v2CurrencyTypes.TRY.ToString(),
            "USD" => Yolcu360v2CurrencyTypes.USD.ToString(),
            "EUR" => Yolcu360v2CurrencyTypes.EUR.ToString(),
            _ => "Error"
        };

        private Yolcu360v2SearchRequest GetEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor)
        {
            return new Yolcu360v2SearchRequest
            {
                checkInDateTime = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                checkOutDateTime = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                age = "30-65",
                country = "TR",
                paymentType = "limit",
                checkInLocation = new CheckLocation
                {
                    lat = additionalInformation.APIPickupLocationCode.Split("~")[0].Replace(',', '.').ToFloatNullSafe(),
                    lon = additionalInformation.APIPickupLocationCode.Split("~")[1].Replace(',', '.').ToFloatNullSafe()
                },
                checkOutLocation = new CheckLocation
                {
                    lat = additionalInformation.APIReturnLocationCode.Split("~")[0].Replace(',', '.').ToFloatNullSafe(),
                    lon = additionalInformation.APIReturnLocationCode.Split("~")[1].Replace(',', '.').ToFloatNullSafe()
                },
                commission = new Yolcu360v2RequestBase.Commission
                {
                    type = "percentage",
                    percentage = 0
                },
                fullCredit = true
            };
        }
    }
}
