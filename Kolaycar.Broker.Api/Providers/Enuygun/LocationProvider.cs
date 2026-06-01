using KolayCAR.Broker.API.Mappers.Enuygun;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Enuygun
{

    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public string ProviderName => "Enuygun";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);

        }

        public Task<ServiceResponseBase> GetLocationDetail(CommonModels.Vendor vendor, int languageId, string locationCode)
        {

            throw new System.NotImplementedException();

        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {

            string username = vendor.ApiKey;
            string password = vendor.ApiPassword;

            var auth = await AuthProvider.GetToken(username, password);


            if (auth?.Status == "OK")
            {

                var result = await RestManager.GetAsync<EnuygunResponse.Locations.Root>(
                               requestPath: "/api/v1/location-list",
                               headers: AuthProvider.CreateHeaderWithToken(auth.Data.Token),
                               parameters: LocationPaginationParams()
                               );

                if (result != null && result.Data.Count > 0)
                {

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = result.Data.Map()
                    };

                }


                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Enuygun location error -> " + result.UserMessage
                };


            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Enuygun için" + auth.UserMessage,
            };

        }

        public Dictionary<string, object> LocationPaginationParams() =>
            new Dictionary<string, object>
            {
                { "pagination[page]", "1" },
                { "pagination[limit]", "10000" }

            };


    }
}
