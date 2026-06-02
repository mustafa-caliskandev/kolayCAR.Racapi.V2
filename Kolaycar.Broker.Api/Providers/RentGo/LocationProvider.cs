using Kolaycar.Broker.Api.Mappers.RentGo;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.RentGo;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.RentGo
{
    public class LocationProvider : ILocationProvider
    {
        private readonly HttpManager _httpManager;

        public LocationProvider(string apiBaseUrl = null)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }

        public string ProviderName => "RentGo";

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var response = await _httpManager.GetAsyncWithModel<RentGoGetConstantsResponse>(
                "/broker/getconstants",
                null,
                new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {vendor.ApiClientId}" },
                }
            );

            if (response?.Offices?.Any() == true)
                return new(response.Offices.Map(), true);

            return new(null, false, "Lokasyon bilgisi alınamadı!");
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            return Task.FromResult(new ServiceResponseBase(null, true));
        }
    }
}
