using Kolaycar.Broker.Api.Mappers.RentGo;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.RentGo;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.RentGo
{
    public class ExtraProvider : IExtraProvider
    {
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var httpManager = new HttpManager(vendor.APIBaseUrl, timeout: vendor.APITimeout);

            var response = await httpManager.GetAsyncWithModel<RentGoGetConstantsResponse>(
                "/broker/getconstants",
                null,
                new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {vendor.ApiClientId}" }
                }
            );

            if (response == null) return new(null, false, "Ekstra listesi alınamadı.");

            var extras = new List<Extra>();

            if (response.AdditionalProducts != null)
                extras.AddRange(response.AdditionalProducts.MapProducts());

            if (response.AdditionalPackages != null)
                extras.AddRange(response.AdditionalPackages.MapPackages());

            return new(extras, extras.Any());
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation?.ReservationToken;
            if (reservationToken == null)
                return new(null, false, "Reservation token bulunamadi.");

            var httpManager = new HttpManager(vendor.APIBaseUrl, timeout: vendor.APITimeout);

            var officeId = additionalInformation?.APIPickupLocationCode;
            if (string.IsNullOrEmpty(officeId))
                return new(null, false, "Office Id bulunamadı.");

            var response = await httpManager.GetAsyncWithModel<RentGoOfficeProductsResponse>(
                $"/broker/office-products?officeId={officeId}",
                null,
                new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {vendor.ApiClientId}" }
                }
            );

            if (response == null) return new(null, false, "Ekstra listesi alınamadı.");

            var extras = new List<Extra>();

            if (response.AdditionalProducts != null)
                extras.AddRange(response.AdditionalProducts.MapProducts());

            if (response.AdditionalPackages != null)
                extras.AddRange(response.AdditionalPackages.MapPackages());

            var vehicleProvider = new VehicleProvider(vendor, false);
            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);
            var getVehiclesResponse = await vehicleProvider.GetVehicles(
                getVehiclesRequest,
                vendor,
                additionalInformation,
                exchangeRates,
                localVehicles,
                subVendors,
                reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode)
                ?? vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle == null)
                return new(null, false, "Arac bilgisi alinamadi.");

            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            selectedVehicle.Extras = extras;

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new(getExtrasResponse, true);
        }
    }
}
