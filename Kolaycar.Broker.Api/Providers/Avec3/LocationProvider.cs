using KolayCAR.Broker.API.Mappers.Avec3;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Avec3
{
    public class LocationProvider : ILocationProvider
    {
        public HttpManager _httpManager;
        public AuthProvider _authProvider;
        public string ProviderName => "Avec3";
        public LocationProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var user = await _authProvider.GetTokenAsync(vendor.ApiKey, vendor.ApiPassword);
            var result = await _httpManager.GetAsync2<Avec3ResponseBase.LocationResponseBase>
                (
                    requestPath: @"branch_base/branch",
                    headers: _authProvider.CreateAuthHeader(user.Data.access_token)
                );
            if (result != null && result.Success && result.Data.data.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result.Data.total_count > 0,
                    Data = result.Data.data.Map()
                };
            }
            return new ServiceResponseBase
            {
                Success = false,
                Message = "Avec lokasyonları gelmiyor!"
            };
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }


    }
}
