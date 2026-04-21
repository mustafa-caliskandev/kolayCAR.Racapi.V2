using KolayCAR.Broker.API.Mappers.Central;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Central
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        public string ProviderName => "Central";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.GetAsync<List<CentralResponseBase.CentralLocation>>(
                requestPath: $"operation/API/GetLocations.php",
                parameters: GetLocationsRequestParameters(vendor));

            return new ServiceResponseBase
            {
                Success = result != null && result.Count > 0,
                Data = result.Map()
            };
        }

        public async Task<List<CentralResponseBase.CentralLocation>> GetAPILocation(Vendor vendor, List<string> apiLocationCodes)
        {
            var result = await RestManager.GetAsync<List<CentralResponseBase.CentralLocation>>(
                requestPath: $"operation/API/GetLocations.php",
                parameters: GetLocationsRequestParameters(vendor));

            if (result != null && result.Count > 0)
                return result.Where(x => apiLocationCodes.Contains(x.code)).ToList();

            return null;
        }

        private Dictionary<string, object> GetLocationsRequestParameters(Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "login",  vendor.ApiKey},
                { "passwd",  vendor.ApiPassword}
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
