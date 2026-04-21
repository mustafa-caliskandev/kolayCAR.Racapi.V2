using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Yolcu360;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yolcu360Provider = KolayCAR.Broker.API.Providers.Yolcu360;

namespace KolayCAR.Broker.API.Providers.Yolcu360
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        private readonly IMemoryCache _memoryCache;
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider;
        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }
        public ExtraProvider(string apiBaseUrl, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl);
            _memoryCache = memoryCache;
            AuthProvider = new AuthProvider(apiBaseUrl, memoryCache);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = new List<Extra>
            {
                new Extra
                {
                    ExtraId = 1,
                    ExtraCode = YolcuProductsTypes.additionalDriver.ToString(),
                    ExtraName = YolcuProductsTypes.additionalDriver.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 2,
                    ExtraCode = YolcuProductsTypes.gpsNavigation.ToString(),
                    ExtraName = YolcuProductsTypes.gpsNavigation.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 3,
                    ExtraCode = YolcuProductsTypes.childSeat.ToString(),
                    ExtraName = YolcuProductsTypes.childSeat.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 4,
                    ExtraCode = YolcuProductsTypes.youngDriver.ToString(),
                    ExtraName = YolcuProductsTypes.youngDriver.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 5,
                    ExtraCode = YolcuProductsTypes.babySeat.ToString(),
                    ExtraName = YolcuProductsTypes.babySeat.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 6,
                    ExtraCode = YolcuProductsTypes.extraRange.ToString(),
                    ExtraName = YolcuProductsTypes.extraRange.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 7,
                    ExtraCode = YolcuProductsTypes.seatAdapter.ToString(),
                    ExtraName = YolcuProductsTypes.seatAdapter.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 8,
                    ExtraCode = YolcuProductsTypes.snowChain.ToString(),
                    ExtraName = YolcuProductsTypes.snowChain.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 9,
                    ExtraCode = YolcuProductsTypes.portBaggage.ToString(),
                    ExtraName = YolcuProductsTypes.portBaggage.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 10,
                    ExtraCode = YolcuProductsTypes.winterTires.ToString(),
                    ExtraName = YolcuProductsTypes.winterTires.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 11,
                    ExtraCode = YolcuProductsTypes.wifiModem.ToString(),
                    ExtraName = YolcuProductsTypes.wifiModem.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 12,
                    ExtraCode = YolcuProductsTypes.damageInsurance.ToString(),
                    ExtraName = YolcuProductsTypes.damageInsurance.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 13,
                    ExtraCode = YolcuProductsTypes.roofRack.ToString(),
                    ExtraName = YolcuProductsTypes.roofRack.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 14,
                    ExtraCode = YolcuProductsTypes.privateDriver.ToString(),
                    ExtraName = YolcuProductsTypes.privateDriver.ToString(),
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                }
            };

            return new ServiceResponseBase
            {
                Success = true,
                Data = extras
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, _memoryCache, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var cookie = await AuthProvider.Login(vendor.ApiKey, vendor.ApiPassword);

            var result = await RestManager.GetAsync<List<Yolcu360ProductResponseBase.Data>>(
                requestPath: "car/listing/extraProducts/" + GetProductsRequestParameters(additionalInformation.ReservationToken.APIReferenceCode),
                headers: AuthProvider.CreateHeaderWithCookie(cookie)
                );

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;

            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VendorId == reservationToken.APIVendorId && x.VehicleName == reservationToken.VehicleName && x.FuelType == reservationToken.FuelType && x.TransmissionType == reservationToken.TransmissionType && x.VehicleCategoryType == reservationToken.VehicleCategoryType);

            var extras = result?.Map(selectedVehicle);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            extras = ReservationHelper.RemoveZeroPriceExtras(extras);
            var extras2 = extras?.GroupBy(x => x.ExtraCode)
                                .SelectMany(g => g.OrderBy(u => u.Price).Take(1))
                                .ToList();

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras2), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private string GetProductsRequestParameters(string param)
        {
            var val = param.Split('|');
            return val.Length == 2 ? val[0] + "/" + val[1] : string.Empty;
        }
    }
}
