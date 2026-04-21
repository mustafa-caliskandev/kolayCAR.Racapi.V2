using KolayCAR.Broker.API.Mappers.Avec2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Avec2
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        public string ProviderName => "Avec2";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await HttpManager.GetXmlAsync<Avec2ResponseBase>(
            requestPath: "xml_Locations.asp");

            if (result != null && result.Locations != null && result.Locations.Location != null)
                return new ServiceResponseBase
                {
                    Success = result.Locations.Location.Count > 0,
                    Data = result.Locations.Location.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Avec lokasyonları gelmiyor!"
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }
    }
}
