using KolayCAR.Broker.API.Mappers.Cizgi;
using KolayCAR.Broker.API.Mappers.Turmobil;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.TurmobilResponseBase;

namespace KolayCAR.Broker.API.Providers.Turmobil
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        HttpManager _httpManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public string ProviderName => "Turmobil";

        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            _httpManager = new HttpManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await _httpManager.PostAsync2<object, LocationTurmobil>(
              requestPath: "/rest/dailyrezervation/locationList",
              headers: new Dictionary<string, object>
              {
                        {"username" , vendor.ApiKey},
                        {"password" , vendor.ApiPassword},
                        {"Accept" , "*/*"},
                        {"Connection","keep-alive" },
                        {"Accept-Encoding","gzip,deflate,br" }
              });

            //var result = await RestManager.PostAsync<object, LocationTurmobil>(
            //    requestPath: "/rest/dailyrezervation/locationList",
            //    headers: new Dictionary<string, object>
            //    {
            //            {"username" , vendor.ApiKey},
            //            {"password" , vendor.ApiPassword},
            //            {"Accept" , "*/*"}
            //    });

            if (result?.Data.data.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Data.data.Map()
                };
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
