using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Central;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentralProvider = KolayCAR.Broker.API.Providers.Central;

namespace KolayCAR.Broker.API.Providers.Central
{
    public class ExtraProvider : IExtraProvider
    {
        RestManager RestManager { get; set; }
        IVehicleProvider vehicleProvider;

        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = await RestManager.GetAsync<List<CentralResponseBase.CentralAdditionalProduct>>(
                requestPath: $"operation/API/GetAdditionalProducts.php",
                parameters: GetExtraListRequestParameters(vendor));

            return new ServiceResponseBase
            {
                Success = result != null && result.Count > 0,
                Data = result.Map()
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var result = await RestManager.GetAsync<List<CentralResponseBase.CentralAdditionalProduct>>(
                requestPath: $"operation/API/GetAdditionalProducts.php",
                parameters: GetExtrasRequestParameters(vendor, additionalInformation));

            if (result?.Count > 0)
            {
                var reservationToken = additionalInformation.ReservationToken;
                var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                vehicleProvider = new VehicleProvider(vendor, false);

                var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

                var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

                var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
                var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

                if (selectedVehicle == null)
                    return new ServiceResponseBase(null, false);

                var extras = result?.Map();

                CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
                CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

                var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
                return new ServiceResponseBase(getExtrasResponse, true);
            }

            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisinden veri alınamadı!");
        }

        private Dictionary<string, object> GetExtraListRequestParameters(Vendor vendor) =>
            new Dictionary<string, object>()
            {
                { "login",  vendor.ApiKey},
                { "passwd",  vendor.ApiPassword}
            };

        private Dictionary<string, object> GetExtrasRequestParameters(Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation) =>
            new Dictionary<string, object>()
            {
                { "login", vendor.ApiKey},
                { "passwd", vendor.ApiPassword},
                { "subGroupId", additionalInformation.ReservationToken.APIReferenceCode}
            };
    }
}
