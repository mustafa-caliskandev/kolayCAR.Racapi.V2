using KolayCAR.Broker.API.Mappers.Garajlar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;

namespace KolayCAR.Broker.API.Providers.Garajlar
{
    public class LocationProvider(string apiBaseUrl) : ILocationProvider
    {
        public string ProviderName => "Garajlar";
        public HttpManager _httpManager = new HttpManager(apiBaseUrl);
        public AuthProvider _authProvider = new AuthProvider(apiBaseUrl);
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var token = await _authProvider.GetTokenHeader(vendor);
            if (token is null)
                return new (null, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var locationList = await _httpManager.GetAsyncWithModel<ResponseBase<List<LocationListResponse>>>("/api/obilet/get-location", headers: token);

            return locationList?.data?.Count > 0
              ? new(locationList.data.Map(), true)
              : new (null, false, $"{vendor.VendorName} araç listesi alınamadı!");
        }
    }
}
