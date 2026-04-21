using Kolaycar.Broker.Api.Mappers.YesOto;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.YesOto;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.YesOto
{
    public class VehicleProvider : IVehicleProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);
            if (string.IsNullOrEmpty(accessToken))
            {
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");
            }

            var parameters = new Dictionary<string, object>();
            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            };

            var response = await _httpManager.PostAsyncWithModel<YesOtoAgeGroupListResponse>(
                "/api/app/ageGroup/ageGroupList",
                parameters,
                headers
            );

            if (response != null)
            {
                return new ServiceResponseBase(response, true);
            }

            return new ServiceResponseBase(null, false, "Araç grupları alınamadı!");
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);
            if (string.IsNullOrEmpty(accessToken))
            {
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");
            }

            var searchRequest = new YesOtoSearchVehicleRequest
            {
                BrandId = vendor.ApiClientId,
                Location = additionalInformation.APIPickupLocationCode, // mapped
                DropOffLocation = additionalInformation.APIReturnLocationCode, // mapped
                Start = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                End = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Age = "25", // Defaulting to 25 as the API doesn't provide it during search
                SalesChannelId = "68010AF6-47E7-5EBB-5A12-3A07DA8486DF", // From Document Example
                LanguageId = getVehiclesRequest.LanguageCode.ToString()
            };

            var parameters = new Dictionary<string, object>();
            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            };

            var response = await _httpManager.PostAsyncWithModel<YesOtoSearchVehicleRequest, YesOtoVehicleListResponse>(
                "/api/app/bookingUI/findReservations",
                searchRequest,
                parameters,
                headers
            );

            if (response != null && response.success && response.data?.vehicles != null)
            {
                var mappedVehicles = response.data.vehicles.Map();
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = mappedVehicles
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = response?.message ?? "Araç bulunamadı."
            };
        }
    }
}
