using KolayCAR.Broker.API.Mappers.Circular;
using KolayCAR.Broker.API.Mappers.Cizgi;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Circular
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public string ProviderName => "Circular";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && auth.success && !string.IsNullOrEmpty(auth.token))
            {

                var result = await RestManager.GetAsync<CircularResponseBase.LocationResponse>(
                    requestPath: $"/car/carlocation/list?token={auth.token}&listsize=1000",
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.token));
                List<CircularResponseBase.Destination> list = new List<CircularResponseBase.Destination>();
                foreach (var destination in result.List)
                {
                    var a = destination.Value;
                    list.Add(a);
                }
                //if (result != null && result.status == 1 && result.list != null && result.list.Count > 0)
                if (result != null && result.status == 1 && result.List != null && result.List.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Success = result != null,
                        Data = list.Map()
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
