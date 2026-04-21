using KolayCAR.Broker.API.Mappers.Otorento;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Otorento
{
    public class LocationProvider : ILocationProvider
    {

        public readonly RestManager _restManager;
        public readonly AuthProvider _authProvider;
        public string ProviderName => "Otorento";
        public LocationProvider(string apiBaseUrl)
        {
            _restManager = new RestManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var token = _authProvider.GetTicket(vendor, languageId);


            var result = await _restManager.PostAsyncXMLRestClient<OtorentoResponseBase.GetTicketResponseBase>(
            requestPath: "/LocationService.asmx",
            parameterType: RestSharp.ParameterType.RequestBody,
            entity: GetLocationsRequestParameters(vendor, token.Result.GetTicketResult.ToString()));

            if (result != null)
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result._Envelope.Body.SearchLocationsResponse.SearchLocationsResult.SearchLocations.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Otorento lokasyonları gelmiyor!"
            };

        }

        private Dictionary<string, object> GetLocationsRequestParameters(Vendor vendor, string token)
        {
            return new Dictionary<string, object>()
            {
                {"text/xml; charset=utf-8",  GetLocationDetailRequestParameters(vendor,token) },
            };
        }


        //public Dictionary<string, object> CreateGetLocationDetailRequestBodyEntity(Vendor vendor, string token) =>
        //  new Dictionary<string, object>()
        //  {
        //        { "application/xml", GetLocationDetailRequestParameters(vendor, token) },
        //  };

        private string GetLocationDetailRequestParameters(Vendor vendor, string token)
        {
            //!!! dil kısmını dinamik olarak çekmek için tekrardan incelenecek!!!!

            string LocationXmlBody = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <SearchLocations xmlns=""http://tempuri.org/"">
      <_Lang>tr</_Lang> 
      <_ServiceAuthenticationTicket>{token}</_ServiceAuthenticationTicket>
    </SearchLocations>
  </soap12:Body>
</soap12:Envelope>";

            return LocationXmlBody;
        }

    }
}
