using KolayCAR.Broker.API.Mappers.Cizgi;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Cizgi
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public string ProviderName => "Cizgi";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var result = await RestManager.GetAsync<CizgiResponseBase.LocationResponse>(
                    requestPath: "list-destinations",
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (result != null && result.status == 1 && result.destinations != null && result.destinations.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Success = result != null,
                        Data = result.destinations.Map()
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Kimlik doğrulama işlemi başarısız!"
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }


    }
}
