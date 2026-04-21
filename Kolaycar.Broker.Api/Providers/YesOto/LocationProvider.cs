using Kolaycar.Broker.Api.Mappers.YesOto;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests.YesOto;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.YesOto
{
    public class LocationProvider : ILocationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;

        public LocationProvider(string apiBaseUrl = null)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }

        public string ProviderName => "YesOto";

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            if (string.IsNullOrEmpty(accessToken))
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");

            var request = new YesOtoSearchLocationRequest
            {
                searchTerm = "",
                brandId = vendor.APIBaseUrl,
                languageId = null
            };

            var parameters = new Dictionary<string, object>();
            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            };

            var response = await _httpManager.PostAsyncWithModel<YesOtoSearchLocationRequest, YesOtoLocationResponse>(
                "/api/app/locationUI/searchDomesticLocation",
                request,
                parameters,
                headers
            );

            if (response != null && response.data != null && response.data.Count > 0)
            {
                return new ServiceResponseBase(response.data.Map(), true);
            }

            return new ServiceResponseBase(null, false, "Lokasyon bilgisi alınamadı!");
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            return Task.FromResult(new ServiceResponseBase(null, true));
        }
    }
}
