using KolayCAR.Broker.API.Mappers.Ayes;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Ayes
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        public string ProviderName => "Ayes";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.GetAsync<List<AyesLocation>>(
                requestPath: $"yerler/?id=" + vendor.ApiKey);
            //parameters: GetLocationsRequestParameters(vendor));

            return new ServiceResponseBase
            {
                Success = result != null && result.Count > 0,
                Data = result.Map()
            };
        }

        private Dictionary<string, object> GetLocationsRequestParameters(Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "id",  vendor.ApiKey},
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
