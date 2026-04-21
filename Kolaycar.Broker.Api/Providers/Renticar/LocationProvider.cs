using KolayCAR.Broker.API.Mappers.Renticar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Renticar
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public string ProviderName => "Renticar";
        public LocationProvider(string apiBaseUrl, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl, memoryCache);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            return await GetLocations(vendor, languageId, false);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, bool isRetryRequest = false)
        {
            var auth = await AuthProvider.GetToken(vendor, isRetryRequest);

            if (auth != null && auth.status == "success")
            {
                var result = await RestManager.GetAsync<List<LocationsResponseBase>>(
                    requestPath: "locations",
                    headers: AuthProvider.CreateHeader(auth.token));

                if (!isRetryRequest && result != null && result.Count > 0 && (result[0].status == "error"))
                {
                    await GetLocations(vendor, languageId, true);
                }

                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Renticar kullanıcı girişi yapılamadı!"
            };
        }
    }
}
