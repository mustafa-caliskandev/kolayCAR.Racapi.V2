using Kolaycar.Broker.Api.Mappers.Eren;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.Eren
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

        public string ProviderName => "Eren";

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            var response = await _httpManager.GetAsyncWithModel<ErenLocationListResponse>(
                "/v1/locations",
                headers: accessToken
            );

            if (response != null && response.Locations != null && response.Locations.Count > 0)
            {
                var mappedLocations = response.Locations.Map();
                return new ServiceResponseBase(mappedLocations, true);
            }

            return new ServiceResponseBase(null, false, "Lokasyon bilgisi alınamadı!");
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            return Task.FromResult(new ServiceResponseBase(null, true));
        }
    }
}
