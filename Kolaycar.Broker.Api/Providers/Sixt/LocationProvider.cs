using KolayCAR.Broker.API.Mappers.Sixt;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Sixt.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Sixt
{
    public class LocationProvider : ILocationProvider
    {
        public string ProviderName => "Sixt";
        RestManager RestManager { get; set; }
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            var result = await RestManager.GetXmlAsync<LocationResponseBase>(
                requestPath: "",
                parameters: GetRequestParameters(vendor)
                );

            if (result != null && result.SIXTTURKEYWEBSERVICES != null && result.SIXTTURKEYWEBSERVICES.STATIONS != null && result.SIXTTURKEYWEBSERVICES.STATIONS.STATION != null && result.SIXTTURKEYWEBSERVICES.STATIONS.STATION.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.SIXTTURKEYWEBSERVICES.STATIONS.STATION.Map().Where(x => x.LocationCode == locationCode).FirstOrDefault()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null,
                Message = "Sixt servisine ulaşılamadı."
            };
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.GetXmlAsync<LocationResponseBase>(
                requestPath: "",
                parameters: GetRequestParameters(vendor)
                );

            if (result != null && result.SIXTTURKEYWEBSERVICES != null && result.SIXTTURKEYWEBSERVICES.STATIONS != null && result.SIXTTURKEYWEBSERVICES.STATIONS.STATION != null && result.SIXTTURKEYWEBSERVICES.STATIONS.STATION.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.SIXTTURKEYWEBSERVICES.STATIONS.STATION.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null,
                Message = "Sixt servisine ulaşılamadı."
            };
        }

        private Dictionary<string, object> GetRequestParameters(Vendor vendor)
        {
            return new Dictionary<string, object>
            {
                { "aid", vendor.ApiKey },
                { "p", vendor.ApiPassword },
                { "m", "getList" },
                { "o", "station" }
            };
        }
    }
}
