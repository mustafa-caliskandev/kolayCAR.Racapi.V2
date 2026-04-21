using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.API.Mappers.Renteon;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Renteon
{
    public class LocationProvider : ILocationProvider
    {
        RestManager _restManager { get; set; }
        AuthProvider _authProvider { get; set; }
        public string ProviderName => "Renteon";
        public LocationProvider(string apiBaseUrl)
        {
            _restManager = new RestManager(apiBaseUrl);
            _authProvider = new AuthProvider();
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var vendorName = vendor.ApiClientId.Split("-")[1]; //connectorId-vendorName-countryCode
            var result = await _restManager.GetAsync<RenteonResponseBase.RenteonLocationResponseBase>(
                requestPath: $"/api/setup/provider/{vendorName}",
                headers: _authProvider.GetBasicAuth(vendor)
            );

            if (result != null)
            {
                var locations = result.Map();
                return new ServiceResponseBase { Data = locations, Success = true }; ;

            }
            return new ServiceResponseBase { Data = null, Success = false, Message = $"{vendor.VendorName} servisine ulaşılamadı!" };
        }
    }
}