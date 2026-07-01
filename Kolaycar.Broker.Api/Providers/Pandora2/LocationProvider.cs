using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using RestSharp;
using System.Collections.Generic;
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
            //var auth = await AuthProvider.GetAccessToken(vendor);

            //if (auth != null && !string.IsNullOrWhiteSpace(auth.access_token))
            //{
            //    var languageCode = GetLanguageCode(languageId);
            //    var countries = await GetCountries(auth.access_token, languageCode);

            //    if (countries == null || countries.Count == 0)
            //        return new ServiceResponseBase(null, false, "Pandora2 ulke servisi basarisiz!");

            //    var apiLocations = new List<Pandora2ResponseBase.Location>();
            //    var hasLocationResponse = false;

            //    foreach (var country in countries.Where(x => x.Id > 0))
            //    {
            //        var countryLocations = await GetLocationsByCountry(auth.access_token, languageCode, country.Id);

            //        if (countryLocations != null)
            //        {
            //            hasLocationResponse = true;
            //            apiLocations.AddRange(countryLocations.Where(IsAirportLocation));
            //        }
            //    }

            //    var locations = apiLocations.Map();

            //    if (!string.IsNullOrWhiteSpace(locationName))
            //        locations = locations
            //            .Where(x => x.LocationName.ToStringNullSafe().ToLowerInvariant().Contains(locationName.ToLowerInvariant()))
            //            .ToList();

            //    return new ServiceResponseBase
            //    {
            //        Success = hasLocationResponse,
            //        Data = locations
            //    };

            return null;
            //}

            //return new ServiceResponseBase(null, false, "Kimlik dogrulama islemi basarisiz!");
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            return Task.FromResult(new ServiceResponseBase(null, false, "Pandora2 lokasyon detay servisi desteklenmiyor."));
        }

        private static string GetLanguageCode(int languageId) =>
            languageId == 2 ? "en" : languageId == 3 ? "de" : "tr";

        private async Task<List<Pandora2ResponseBase.Country>> GetCountries(string accessToken, string languageCode) =>
            await RestManager.GetAsync<List<Pandora2ResponseBase.Country>>(
                requestPath: "countries",
                headers: AuthProvider.CreateAuthHeader(accessToken, languageCode));

        private async Task<List<Pandora2ResponseBase.Location>> GetLocationsByCountry(string accessToken, string languageCode, int countryId) =>
            await RestManager.PostAsyncRestClient<List<Pandora2ResponseBase.Location>>(
                requestPath: "locations",
                parameterType: ParameterType.RequestBody,
                headers: AuthProvider.CreateAuthHeaderWithContentType(accessToken, languageCode),
                entity: AuthProvider.CreateJsonBody(new LocationRequest
                {
                    CountryId = countryId
                }));

        private static bool IsAirportLocation(Pandora2ResponseBase.Location location) =>
            !string.IsNullOrWhiteSpace(location?.Code);
    }

    public class LocationRequest
    {
        public int CountryId { get; set; }
    }
}
