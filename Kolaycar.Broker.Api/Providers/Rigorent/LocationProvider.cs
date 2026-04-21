using KolayCAR.Broker.API.Mappers.Avec;
using KolayCAR.Broker.API.Mappers.Rigorent;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Rigorent
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager HttpManager { get; set; }
        public string ProviderName => "Rigorent";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var result = await HttpManager.GetXmlAsync<RigorentResponseBase>(
            requestPath: "xml/xml_Sube.asp");

            if (result != null && result.MyCarRent != null && result.MyCarRent.Sube != null)
                return new ServiceResponseBase
                {
                    Success = result.MyCarRent.Sube.Count > 0,
                    Data = result.MyCarRent.Sube.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Rigorent lokasyonları gelmiyor!"
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
