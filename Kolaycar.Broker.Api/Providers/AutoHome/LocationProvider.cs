using KolayCAR.Broker.API.Mappers.AutoHome;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.AutoHome
{
    public class LocationProvider : ILocationProvider
    {
        private readonly HttpManager _httpManager;
        public string ProviderName => "AutoHome";
        public LocationProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await _httpManager.GetAsync2<List<AutoHomeResponseBase.Location>>(
                requestPath: "/API/reservation/v2/GetLocations.php",
                parameters: GetLocationsParameters(vendor)
                ); ;
            if (result.Success)
            {
                if (result.Data.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Data = result.Data.Map(),
                        Success = true
                    };
                }
                return new ServiceResponseBase
                {
                    Data = null,
                    Success = false,
                    Message = result.Message
                };
            }
            return new ServiceResponseBase
            {
                Success = false,
                Message = "Autohome servisine ulaşılamadı !"
            };
        }
        private IDictionary<string, object> GetLocationsParameters(Vendor vendor)
        {
            return new Dictionary<string, object>() { { "login", vendor.ApiKey }, { "passwd", vendor.ApiPassword } };
        }
    }
}
