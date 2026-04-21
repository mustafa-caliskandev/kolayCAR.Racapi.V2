using KolayCAR.Broker.API.Mappers.Elibol;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Enterprise
{
    public class LocationProvider : ILocationProvider
    {
        public string ProviderName => "Enterprise";
        HttpManager HttpManager { get; set; }

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
            var result = await HttpManager.GetXmlAsync<AssistResponseBase>(
            requestPath: "xml_Locations.asp",
            headers: null);

            if (result != null && result.AytuRent != null && result.AytuRent.Location != null)
                return new ServiceResponseBase
                {
                    Success = result.AytuRent.Location.Count > 0,
                    Data = result.AytuRent.Location.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Elibol lokasyonları gelmiyor!"
            };

        }
    }
}
