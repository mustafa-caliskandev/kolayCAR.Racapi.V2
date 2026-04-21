using KolayCAR.Broker.API.Mappers.Erboycar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Erboycar
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        public string ProviderName => "Erboycar";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.GetAsync<List<ErboycarResponseBase.Location>>(
                requestPath: "stations");

            if (result != null && result.Count > 0)
                return new ServiceResponseBase
                {
                    Success = result != null && result.Count > 0,
                    Data = result.Map()
                };

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
