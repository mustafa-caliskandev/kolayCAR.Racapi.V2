using KolayCAR.Broker.API.Mappers._5S;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers._5S
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        //Configuration Configuration { get; set; }
        public string ProviderName => "BesS";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            //Configuration = new Configuration();
        }
        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.GetAsync<BesSResponseBase.LocationResponse>(
                requestPath: "extservice/location",
                headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey)
                );

            if (result != null && result.data != null && result.data.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Data = result.Map(),
                    Success = true
                };
            }

            return new ServiceResponseBase
            {
                Data = null,
                Success = false
            };
        }
    }
}
