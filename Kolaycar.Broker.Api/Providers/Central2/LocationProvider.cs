using KolayCAR.Broker.API.Mappers.Central2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Central2
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        public string ProviderName => "Central2";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.GetXmlAsync<CommonModels.Response.Central2ResponseBase.CentralResponse>(//Düzenleme yapılacak!!!!!!!!!!
            requestPath: "XML_Locations.Aspx",
            parameters: GetLocationsRequestParameters(vendor));

            if (result != null && result != null && result.Sistemrent.Location != null)
                return new ServiceResponseBase
                {
                    Success = result.Sistemrent.Location.Count > 0,
                    Data = result.Sistemrent.Location.Map() //düzenleme yapılacak!!!!!!!!!!
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Central lokasyonları gelmiyor!"
            };
        }

        private Dictionary<string, object> GetLocationsRequestParameters(CommonModels.Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "Key_Hack", vendor.SecretKey }
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
