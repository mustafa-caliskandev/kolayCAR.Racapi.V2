using Kolaycar.Broker.Api.Mappers.RentGo;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.RentGo;
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
                    { "Authorization", $"Bearer {vendor.ApiKey + vendor.ApiPassword + vendor.ApiClientId}" },
                    { "x-channel-token", vendor.ApiKey }
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
    }
}
