using KolayCAR.Broker.API.Mappers.Sixt2;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Sixt2
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager _httpManager;
        AuthProvider _authProvider;
        public string ProviderName => "Sixt2";
        public LocationProvider(string apiBaseUrl, ICacheService cacheService)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl, cacheService);
        }

        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {

            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var token = await _authProvider.GetBearerToken(vendor);
            if (token == null) return new(null, false, "Sixt token bilgisi alınamadı!");

            var locations = await _httpManager.GetAsyncWithModel<SixtResponseBase<List<SixtLocationItem>>>("/api/v1/stations", headers: token);
            //var locations = new SixtResponseBase<List<SixtLocationListResponse>>();
            return locations?.result.Any() == true
                ? new(locations.result.Map(), true)
                : new(null, false, "Sixt lokasyon listesi alınamadı!");
        }
    }
}
