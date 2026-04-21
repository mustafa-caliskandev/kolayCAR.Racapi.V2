using KolayCAR.Broker.API.Mappers.Otocar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Otocar
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        public string ProviderName => "Otocar";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await HttpManager.GetXmlAsync<OtocarResponseBase.LocationResponse>(
                     requestPath: "AllLocation");
            if (result != null && result.ArrayOfLocation.Location.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.ArrayOfLocation.Location.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }


    }
}
