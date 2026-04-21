using KolayCAR.Broker.API.Mappers.Turevrac2;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Turevrac2
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager _httpManager;
        public string ProviderName => "Turevrac2";
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
            var locationResult = await _httpManager.GetAsync2<List<Turevrac2ResponseBase.Location>>(
                    requestPath: "/JsonLocations.aspx",
                    parameters: GetLocationParameters(vendor)
                );
            if (locationResult != null)
                if (locationResult.Data.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Data = locationResult.Data.Map(),
                        Success = locationResult.Success
                    };
                }
            return new ServiceResponseBase
            {
                Data = null,
                Success = locationResult.Success,
                Message = $"{vendor.VendorName} - Turevrac2 servisinden lokasyon yanıtı alınamadı!"
            };
        }

        private IDictionary<string, object> GetLocationParameters(Vendor vendor)
        {
            return new Dictionary<string, object> 
            {
                { "Key_Hack", vendor.ApiClientId },
                { "User_Name", vendor.ApiKey },
                { "User_Pass", vendor.ApiPassword }

            };
            
        }
    }
}
