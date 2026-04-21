using KolayCAR.Broker.API.Mappers.ZiraatFilo;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.ZiraatFilo
{
    public class LocationProvider : ILocationProvider
    {
        private HttpManager _httpManager;
        private AuthProvider _authProvider;
        public string ProviderName => "ZiraatFilo";
        public LocationProvider(string apiBaseUrl) 
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var headers = await _authProvider.GetHeaders(vendor);
            if (headers == null)
                return new (null, false, "Token bilgisi alınamadı!");

            var locations = await _httpManager.GetXmlAsync<ZiraatFiloResposeBase.Root>(
                requestPath: "/api/broker-service/locations/",
                headers: headers);

            if (locations?.Sistemrent?.Location?.Count > 0)
                return new (locations.Sistemrent.Location.Map(), true);

            return new (null, false, "Lokasyon bilgisi alınamadı!");
        }
    }
}
