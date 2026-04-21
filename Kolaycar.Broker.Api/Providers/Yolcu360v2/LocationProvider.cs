using KolayCAR.Broker.API.Mappers.Yolcu360v2;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Yolcu360v2.Yolcu360v2ResponseBaseDeleted;

namespace KolayCAR.Broker.API.Providers.Yolcu360v2
{
    public class LocationProvider : ILocationProvider
    {
        private HttpManager _httpManager;
        private AuthProvider _authProvider;
        private readonly ILocationVendorService _locationVendorService;
        public LocationProvider(string apiBaseUrl, ICacheService cacheService, ILocationVendorService locationVendorService = null)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl, cacheService);
            if (locationVendorService != null)
                _locationVendorService = locationVendorService;
        }
        public string ProviderName => throw new System.NotImplementedException();
        public Task<ServiceResponseBase> GetLocationDetail(Domain.Models.Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
        public async Task<ServiceResponseBase> GetLocations(Domain.Models.Vendor vendor, int languageId, string locationName)
        {
            //await Yolcu360v2LocationsMap(vendor);

            var token = await _authProvider.GetToken(vendor);

            var locations = await _httpManager.GetAsyncWithModel<List<Yolcu360v2LocationResponse>>(
                requestPath: $"/api/v1/locations?query={locationName}",
                headers: token
            );

            var tasks = new List<Task>();
            var locationDetails = new List<Yolcu360v2LocationDetailResponse>();

            foreach (var location in locations)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var locationDetail = await _httpManager.GetAsyncWithModel<Yolcu360v2LocationDetailResponse>(
                        requestPath: $"/api/v1/locations/{location.placeId}",
                        headers: token
                    );

                    if (locationDetail != null)
                        locationDetails.Add(locationDetail);
                }));
            }
            await Task.WhenAll(tasks);
            return new(locations.Map(locationDetails), true);
        }


        public async Task Yolcu360v2LocationsMap(Domain.Models.Vendor vendor, List<API.Models.Location> locations)
        {
            var httpManager = new HttpManager("https://places.googleapis.com");
            var tasks = new List<Task>();
            var list = new List<Locationvendor>();

            foreach (var location2 in locations)
            {
                tasks.Add(Task.Run(async () =>
                    {
                        var result = await httpManager.PostAsyncWithModel<Req, Root>("/v1/places:searchText", new Req { textQuery = location2.Locationname }, headers: new Dictionary<string, object> { { "X-Goog-Api-Key", "AIzaSyAS3cpUGZfFmCzzCp7HodNwT6RR8mLzFGI" }, { "X-Goog-FieldMask", "places.name,places.location,places.formattedAddress,places.name,places.displayName" } });

                        if (result != null)
                        {
                            try
                            {
                                string lat = result.places.FirstOrDefault().location.latitude.ToString().Length > 9
                                         ? result.places.FirstOrDefault().location.latitude.ToString().Substring(0, 9)
                                         : result.places.FirstOrDefault().location.latitude.ToString();

                                string lon = result.places.FirstOrDefault().location.longitude.ToString().Length > 9
                                               ? result.places.FirstOrDefault().location.longitude.ToString().Substring(0, 9)
                                               : result.places.FirstOrDefault().location.longitude.ToString();

                                list.Add(new Locationvendor
                                {
                                    Active = true,
                                    Apilocationname = result.places.FirstOrDefault().displayName.text,
                                    Vendorid = vendor.VendorId,
                                    Locallocationid = location2.Id,
                                    Locationcode = $"{lat}~{lon}",
                                    Ispickup = true
                                });
                            }
                            catch (System.Exception)
                            {

                            }

                        }
                    }));

                await Task.WhenAll(tasks);

            }
            ;

            await _locationVendorService.AddRangeLocationVendor(list);
        }

        public class DisplayName
        {
            public string text { get; set; }
            public string languageCode { get; set; }
        }

        public class Location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Place
        {
            public string name { get; set; }
            public string formattedAddress { get; set; }
            public Location location { get; set; }
            public DisplayName displayName { get; set; }
        }

        public class Root
        {
            public List<Place> places { get; set; }
        }
        public class Req
        {
            public string textQuery { get; set; }
        }

    }
}
