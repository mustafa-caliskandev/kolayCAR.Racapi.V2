using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Resws;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static KolayCAR.Resws.ServiceSoapClient;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.KolayCAR
{
    public class LocationProvider : ILocationProvider
    {
        private readonly ServiceSoapClient _kolayCARService;
        public string ProviderName => "KolayCAR";
        public LocationProvider(Vendor vendor)
        {
            _kolayCARService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap, Helpers.KolayCAR.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl), vendor.APITimeout);

        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var getLocationsResult = await _kolayCARService.GET_LOCATIONSAsync(
                vendor.ApiKey,
                vendor.ApiPassword,
                ((LanguageTypes)languageId - 1).ToString());

            var getLocationsResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(getLocationsResult.Body.GET_LOCATIONSResult);

            return new ServiceResponseBase
            {
                Success = getLocationsResponseObject.RETURNCODE == 0,
                Message = getLocationsResponseObject.RETURNCODE != 0 ? "KolayCAR servisine ulaşılamadı!" : string.Empty,
                ServiceCode = getLocationsResponseObject.RETURNCODE.ToString(),
                ServiceMessage = getLocationsResponseObject.MESSAGE,
                Data = getLocationsResponseObject.LOCATIONS.Map()
            };
        }
    }
}
