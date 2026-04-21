using KolayCAR.Broker.API.Mappers.Circular2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Circular2
{
    public class LocationProvider : ILocationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        public string ProviderName => "Circular2";
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
            var auth = await _authProvider.GetTokenAsync(vendor.ApiKey);

            if (auth is null || !auth.success || string.IsNullOrEmpty(auth.token))
                return new(null, false, "Kimlik doğrulama işlemi başarısız!");

            var result = await _httpManager.GetAsync2<Circular2ResponseBase.ApiLocationResponse>(
                    requestPath: $"/car/carlocation/list?token={auth.token}&listsize=1000"
                    );

            var list = result?.Data?.list;
            if (list?.Count > 0)
                return new(list.Map(), true);

            return new(null, false, "Lokasyon verisi bulunamadı.");
        }
    }
}
