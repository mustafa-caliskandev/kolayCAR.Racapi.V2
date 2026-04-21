using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Otorento
{
    public class AuthProvider
    {
        RestManager _restManager { get; set; }

        public AuthProvider(string apiBaseUrl)
        {
            _restManager = new RestManager(apiBaseUrl);
        }

        public async Task<OtorentoResponseBase.GetTicketResponse> GetTicket(Domain.Models.Vendor vendor, int languageId)
        {


            var result = await _restManager.PostAsyncXMLRestClient<OtorentoResponseBase.GetTicketResponseBase>(
            requestPath: "/ServiceAuthenticationService.asmx",
            parameterType: RestSharp.ParameterType.RequestBody,
            entity: CreateGetTicketRequestBodyEntity(vendor));
            ;
            if (result == null)
            {
                return null;
            }

            return result._Envelope.Body.GetTicketResponse;

        }


        public Task<ServiceResponseBase> GetLocationDetail(Domain.Models.Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, object> CreateGetTicketRequestBodyEntity(Domain.Models.Vendor vendor) =>
          new Dictionary<string, object>()
          {
                {"text/xml; charset=utf-8",  GetTicketRequestParameters(vendor) },
          };

        private string GetTicketRequestParameters(Domain.Models.Vendor vendor)
        {
            var xmlString = @$"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
              <soap12:Body>
                <GetTicket xmlns=""http://tempuri.org/"">
                  <userid>{vendor.ApiKey}</userid>
                  <password>{vendor.ApiPassword}</password>
                </GetTicket>
              </soap12:Body>
            </soap12:Envelope>";
            return xmlString;
            //var body = new StringContent(xmlString, Encoding.UTF8, "text/xml; charset=utf-8");

            //var result = new HttpClient().PostAsync("https://otorento.com.tr/Services/ServiceAuthenticationService.asmx", body).Result;

            //var body = OtorentoHelper.RequestHelper.GetOtorentoRequestObject(vendor);

            //XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //ns.Add("myNamespace", "http://flibble");
            //XmlSerializer xser = new XmlSerializer(typeof(OtorentoRequestBase));
            //xser.Serialize(Console.Out, body, ns);

            //var bodyXML = ObjectHelper.ObjectToXML(body);
            ////return result;

            //return bodyXML;
        }
    }
}
