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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace KolayCAR.Broker.API.Providers.Yolcu360v2
{
    public class ExtraProvider : IExtraProvider
    {
        private readonly HttpManager _httpManager;
        private readonly VehicleProvider _vehicleProvider;
        private readonly AuthProvider _authProvider;
        public ExtraProvider(Vendor vendor, ICacheService cacheService)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl);
            _authProvider = new AuthProvider(vendor.APIBaseUrl, cacheService);
            _vehicleProvider = new VehicleProvider(vendor, false, cacheService);
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
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 2,
                    ExtraCode = YolcuProductsTypes.gpsNavigation.ToString(),
                    ExtraName = YolcuProductsTypes.gpsNavigation.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 3,
                    ExtraCode = YolcuProductsTypes.childSeat.ToString(),
                    ExtraName = YolcuProductsTypes.childSeat.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 4,
                    ExtraCode = YolcuProductsTypes.youngDriver.ToString(),
                    ExtraName = YolcuProductsTypes.youngDriver.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 5,
                    ExtraCode = YolcuProductsTypes.babySeat.ToString(),
                    ExtraName = YolcuProductsTypes.babySeat.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 6,
                    ExtraCode = YolcuProductsTypes.extraRange.ToString(),
                    ExtraName = YolcuProductsTypes.extraRange.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 7,
                    ExtraCode = YolcuProductsTypes.seatAdapter.ToString(),
                    ExtraName = YolcuProductsTypes.seatAdapter.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 8,
                    ExtraCode = YolcuProductsTypes.snowChain.ToString(),
                    ExtraName = YolcuProductsTypes.snowChain.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 9,
                    ExtraCode = YolcuProductsTypes.portBaggage.ToString(),
                    ExtraName = YolcuProductsTypes.portBaggage.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 10,
                    ExtraCode = YolcuProductsTypes.winterTires.ToString(),
                    ExtraName = YolcuProductsTypes.winterTires.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 11,
                    ExtraCode = YolcuProductsTypes.wifiModem.ToString(),
                    ExtraName = YolcuProductsTypes.wifiModem.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 12,
                    ExtraCode = YolcuProductsTypes.damageInsurance.ToString(),
                    ExtraName = YolcuProductsTypes.damageInsurance.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 13,
                    ExtraCode = YolcuProductsTypes.roofRack.ToString(),
                    ExtraName = YolcuProductsTypes.roofRack.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 14,
                    ExtraCode = YolcuProductsTypes.privateDriver.ToString(),
                    ExtraName = YolcuProductsTypes.privateDriver.ToString(),
                    ExtraRentalType = ExtraRentalTypes.PerRental,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 15,
                    ExtraCode = "TMHS-insurance",
                    ExtraName = "Tamamlayıcı Mini Hasar Sigortası",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                },
                new Extra
                {
                    ExtraId = 16,
                    ExtraCode = "MHS-insurance",
                    ExtraName = "Mini Hasar Sigortası",
                    ExtraRentalType = ExtraRentalTypes.Daily,
                    ExtraQuantityIncreasable = false
                }
            };

            return new(extras, true);
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var token = await _authProvider.GetToken(vendor);

            var result = await _httpManager.GetAsyncWithModel<List<Yolcu360v2ExtraResponseBase.Root>>(
                requestPath: $"/api/v1/search/{reservationToken.APIReferenceCode}/{reservationToken.VehicleCode}/extra-products",
                headers: token
                );

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;

            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            var extras = result.Map(additionalInformation.RentalDuration);

            if (selectedVehicle is null)
                return new ServiceResponseBase($"{vendor.VendorName} araç bilgisi alınamadı!", false);

            selectedVehicle.Extras = extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            extras = ReservationHelper.RemoveZeroPriceExtras(extras);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}
