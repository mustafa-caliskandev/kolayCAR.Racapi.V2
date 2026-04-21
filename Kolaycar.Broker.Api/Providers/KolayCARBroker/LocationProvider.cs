using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.KolayCARBroker
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public string ProviderName => "KolayCARBroker";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var auth = await AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword));

            if (auth != null)
            {
                var user = auth.Data as User;

                var result = await HttpManager.GetAsync<List<CommonModels.Location>>(
                    requestPath: "locations",
                    parameters: CreateBrokerGetLocationsRequestParameters(languageId),
                    headers: AuthProvider.CreateAuthHeader(user.Token));

                if (result.Success)
                    return new ServiceResponseBase
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ServiceMessage = result.Message,
                        Data = result.Data
                    };

                return new ServiceResponseBase
                {
                    Success = result.Success,
                    Message = result.Message,
                    ServiceMessage = result.Message
                };
            }

            return new ServiceResponseBase
            {
                Success = auth.Success,
                Message = "Kimlik doğrulama işlemi başarısız!",
                ServiceMessage = auth.Message
            };
        }

        private IDictionary<string, object> CreateBrokerGetLocationsRequestParameters(int languageId) =>
            new Dictionary<string, object>()
            {
                { "languageId", languageId}
            };

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
