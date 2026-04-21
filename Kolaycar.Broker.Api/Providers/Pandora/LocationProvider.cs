using KolayCAR.Broker.API.Mappers.Pandora;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora
{
    public class LocationProvider : ILocationProvider
    {
        public string ProviderName => "Pandora";
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null)
            {
                var result = await RestManager.GetAsync<List<PandoraResponseBase.Office>>(
                    requestPath: $"tr/api/offices",
                    headers: AuthProvider.CreateAuthHeader(auth.access_token));

                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Kimlik doğrulama işlemi başarısız!",
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
