using KolayCAR.Broker.API.Mappers.CredyCar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.CredyCar
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        public string ProviderName => "CredyCar";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await HttpManager.GetXmlAsync<CredyCarResponseBase>(
           parameters: GetLocationsRequestParameters(vendor),
           requestPath: "xml_Locations.asp");

            if (result != null && result.AytuRent != null && result.AytuRent.Location != null)
                return new ServiceResponseBase
                {
                    Success = result.AytuRent.Location.Count > 0,
                    Data = result.AytuRent.Location.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "CredyCar lokasyonları gelmiyor!"
            };

        }

        private Dictionary<string, object> GetLocationsRequestParameters(Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "User_Name", vendor.ApiKey},
                { "User_Pass",  vendor.ApiPassword },
                { "Key_Deger",  vendor.SecretKey }

            };
        }
    }
}
