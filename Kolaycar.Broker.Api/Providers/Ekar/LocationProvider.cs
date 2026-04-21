using KolayCAR.Broker.API.Mappers.Ekar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Ekar
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        public string ProviderName => "Ekar";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var result = await HttpManager.GetXmlAsync<EkarResponseBase>(
            requestPath: "XML_Lokasyon.Asp",
            parameters: GetLocationsRequestParameters(vendor));

            if (result != null && result.EkarSistemrent != null && result.EkarSistemrent.Sube != null && result.EkarSistemrent.Sube.Count > 0)
                return new ServiceResponseBase
                {
                    Success = result.EkarSistemrent.Sube.Count > 0,
                    Data = result.EkarSistemrent.Sube.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Ekar lokasyonları gelmiyor!"
            };
        }

        private Dictionary<string, object> GetLocationsRequestParameters(CommonModels.Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "Key_Hack",  vendor.ApiClientId}
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
