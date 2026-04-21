using KolayCAR.Broker.API.Mappers.Nissa;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Nissa2
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        public string ProviderName => "Nissa2";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var result = await HttpManager.GetXmlAsync<NissaResponseBase>(
            requestPath: "XML_Locations.Asp",
            parameters: GetLocationsRequestParameters(vendor));

            if (result != null && result.Sistemrent != null && result.Sistemrent.Location != null)
                return new ServiceResponseBase
                {
                    Success = result.Sistemrent.Location.Count > 0,
                    Data = result.Sistemrent.Location.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Nissa lokasyonları gelmiyor!"
            };
        }

        private Dictionary<string, object> GetLocationsRequestParameters(CommonModels.Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "Key_Hack", vendor.ApiClientId }
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
