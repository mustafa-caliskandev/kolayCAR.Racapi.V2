using KolayCAR.Broker.API.Mappers.Elitcar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Elitcar
{
    public class LocationProvider : ILocationProvider
    {
        public string ProviderName => "Elitcar";
        public LocationProvider(string apiBaseUrl)
        {
        }
        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {

            var locations = LocationMapper.Map();
            if (locations != null && locations.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = locations != null,
                    Data = locations
                };

            }
            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }
        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }


    }
}
