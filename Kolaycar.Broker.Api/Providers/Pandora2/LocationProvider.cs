using KolayCAR.Broker.API.Mappers.Pandora2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora2
{
    public class LocationProvider : ILocationProvider
    {
        public string ProviderName => "Pandora2";
        private RestManager RestManager { get; set; }
        private AuthProvider AuthProvider { get; set; }

        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(AuthProvider.NormalizeBaseUrl(apiBaseUrl));
            AuthProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null && !string.IsNullOrWhiteSpace(auth.access_token))
            {
                var result = await RestManager.PostAsyncRestClient<List<Pandora2ResponseBase.Location>>(
                    requestPath: "locations",
                    parameterType: ParameterType.RequestBody,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token, GetLanguageCode(languageId)),
                    entity: AuthProvider.CreateJsonBody(new LocationRequest
                    {
                        CountryId = vendor.CountryId > 0 ? vendor.CountryId : 228
                    }));

                var locations = result.Map();

                if (!string.IsNullOrWhiteSpace(locationName))
                    locations = locations
                        .Where(x => x.LocationName.ToStringNullSafe().ToLowerInvariant().Contains(locationName.ToLowerInvariant()))
                        .ToList();

                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = locations
                };
            }

            return new ServiceResponseBase(null, false, "Kimlik dogrulama islemi basarisiz!");
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            return Task.FromResult(new ServiceResponseBase(null, false, "Pandora2 lokasyon detay servisi desteklenmiyor."));
        }

        private static string GetLanguageCode(int languageId) =>
            languageId == 2 ? "en" : languageId == 3 ? "de" : "tr";
    }

    public class LocationRequest
    {
        public int CountryId { get; set; }
    }
}
