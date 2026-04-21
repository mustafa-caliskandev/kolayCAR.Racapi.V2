using KolayCAR.Broker.API.Mappers.Wheelsys;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Wheelsys
{
    public class LocationProvider : ILocationProvider
    {
        public HttpManager _httpManager;
        public string ProviderName => "Wheelsys";
        public LocationProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await _httpManager.GetXmlAsync<WheelsysResponseBase.Root>(
            requestPath: $"{vendor.ApiKey}/link/v3/stations_{vendor.ApiPassword.Split('-')[0]}.html",
            parameters: GetLocationsRequestParameters(vendor));
            if (result != null && result.response.station != null && result.response.station.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result.response.station.Count > 0,
                    Data = result.response.station.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }
        private Dictionary<string, object> GetLocationsRequestParameters(CommonModels.Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "AGENT",  vendor.ApiPassword.Split('-')[1]}
            };
        }
    }
}
