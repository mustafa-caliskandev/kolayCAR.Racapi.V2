using KolayCAR.Broker.API.Mappers.Hara;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Hara
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        public string ProviderName => "Hara";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var result = await HttpManager.GetXmlAsync<HaraResponseBase>(
            requestPath: "xml_Locations.asp",
            parameters: GetLocationsRequestParameters(vendor));

            if (result != null && result.HaraResponse != null && result.HaraResponse.Location != null)
                return new ServiceResponseBase
                {
                    Success = result.HaraResponse.Location.Count > 0,
                    Data = result.HaraResponse.Location.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Hara lokasyonları gelmiyor!"
            };
        }

        private Dictionary<string, object> GetLocationsRequestParameters(CommonModels.Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "User_Name",  vendor.ApiKey},
                { "User_Pass",  vendor.ApiPassword}
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
