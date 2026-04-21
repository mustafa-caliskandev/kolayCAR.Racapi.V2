using KolayCAR.Broker.API.Mappers.Eganis;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.PandoraResponseBase;

namespace KolayCAR.Broker.API.Providers.Eganis
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }

        public LocationProvider(string apiBaseUrl) 
        { 
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }
        public string ProviderName => "Eganis";

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var getToken = await AuthProvider.getToken(vendor);

            if (getToken == null)
                return new ServiceResponseBase { Success = false, Message = "Token Başarılı Bir Şekilde Alınamadı.", };

            var result = await RestManager.GetAsync<EganisResponseBase.LocationResponse.Root>
                (
                    requestPath: "/Api/GetLocations",
                    headers: getToken
                );

            if(result.data.Count == 0)
            {
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Lokasyon listesi API'den başarıyla alınamadı.",
                };
            }
            

            return new ServiceResponseBase
            {
                Success = true,
                Data = result.data.Map()
            };


        }
    }
}
