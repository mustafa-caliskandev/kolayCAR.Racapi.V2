using KolayCAR.Broker.API.Mappers.Ototur;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.OtoturResponseBase;

namespace KolayCAR.Broker.API.Providers.Ototur
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager _httpManager { get; set; }
        AuthProvider _authProvider { get; set; }
        public string ProviderName => "Ototur";
        public LocationProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Broker.Domain.Models.Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Broker.Domain.Models.Vendor vendor, int languageId, string locationName = "")
        {
            var token = await _authProvider.GetToken();
            var result = await _httpManager.GetAsync2<OtoturBaseResponse>(
                requestPath: "locations",
                headers: GetHeaders(token)
                );
            return new ServiceResponseBase { Data = result.Data.result.content.Map(), Success = true };
        }

        private IDictionary<string, object> GetHeaders(OtoturAuthResponse ototurAuthResponse)
        {
            return new Dictionary<string, object>() { { "Authorization", $"Bearer {ototurAuthResponse.token}" } };
        }
    }
}
