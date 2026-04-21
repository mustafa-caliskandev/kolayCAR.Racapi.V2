using KolayCAR.Broker.API.Mappers.Ekar2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Ekar2
{
    public class LocationProvider : ILocationProvider
    {
        private HttpManager _httpManager { get; set; }
        private AuthProvider _authProvider { get; set; }
        public string ProviderName => "Ekar2";
        public LocationProvider(string ApiBaseUrl)
        {
            _httpManager = new HttpManager(ApiBaseUrl);
            _authProvider = new AuthProvider(ApiBaseUrl);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await _httpManager.GetAsync2<Ekar2ResponseBase.Ekar2LocationListResponse>(
                requestPath: @"/api/v1/locations",
                headers: _authProvider.GetHeaders(vendor)

                );

            if (result != null && result.Data.result.content.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Data = result.Data.result.content.Map(),
                    Message = result.Data.message,
                    Success = result.Data.success
                };
            }
            return new ServiceResponseBase
            {
                Data = null,
                Message = "Ekar servisine ulaşılamadı!",
                Success = false
            };
        }
    }
}
