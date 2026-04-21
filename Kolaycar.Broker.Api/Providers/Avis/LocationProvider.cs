using KolayCAR.Broker.API.Helpers.Avis;
using KolayCAR.Broker.API.Mappers.Avis;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.AvisRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Providers.Avis
{
    public class LocationProvider : ILocationProvider
    {
        HttpManager _httpManager;
        AuthProvider _authProvider;
        public string ProviderName => "Avis";
        public LocationProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var avisLocationRequest = new AvisLocationRequest { Brand = vendor.VendorName, CountryCode = "TR" };
            var token = await _authProvider.GetTokenAsync(vendor);

            var result = await _httpManager.PostAsync2<AvisLocationRequest, AvisLocationResponse>(
                        requestPath: "/STOfficeApp/GetOfficeList",
                        headers: QueryHelper<AvisLocationRequest>.GetHeaders(token, avisLocationRequest, vendor.SecretKey),
                        entity: avisLocationRequest
            );

            if (ObjectValidationHelper.CheckResponseObject(result))
                return new ServiceResponseBase(result.Data.Data.Map(), true);

            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı!");
        }
    }
}
