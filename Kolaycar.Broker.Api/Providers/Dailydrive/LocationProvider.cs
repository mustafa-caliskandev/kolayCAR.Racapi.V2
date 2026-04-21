using KolayCAR.Broker.API.Mappers.Dailydrive;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Dailydrive
{
    public class LocationProvider : ILocationProvider
    {
        RestManager HttpManager { get; set; }
        public string ProviderName => "DailyDrive";
        public LocationProvider(string apiBaseUrl)
        {
            HttpManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var content = new StringContent(@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
                                 xmlns:dto='http://ws.naryaz.com/model/dto'>
                                 <soapenv:Header/>
                                 <soapenv:Body>
                                 <dto:LocationsRequest>
                                 <dto:cityName></dto:cityName>
                                 </dto:LocationsRequest>
                                 </soapenv:Body>
                                </soapenv:Envelope>", Encoding.UTF8, "text/xml");
            var result = HttpManager.PostAsyncHttpContent<DailydriveLocationResponseBase>(
                 entity: content,
                 requestPath: "",
                 headers: GetLocationsRequestParameters(vendor)).Result;

            if (result != null)
            {
                return new ServiceResponseBase
                {
                    Success = result.SOAPENVEnvelope.SOAPENVBody.Ns2LocationsResponse.Ns2Locations.Count > 0,
                    Data = result.SOAPENVEnvelope.SOAPENVBody.Ns2LocationsResponse.Ns2Locations.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Dailydrive lokasyonları gelmiyor!"
            };
        }

        private Dictionary<string, object> GetLocationsRequestParameters(CommonModels.Vendor vendor)
        {
            var authToken = Encoding.ASCII.GetBytes($"{vendor.ApiKey}:{vendor.ApiPassword}");
            var token = Convert.ToBase64String(authToken);

            return new Dictionary<string, object>()
            {
                { "Content-Type", "text/xml"  },
                { "Accept", "*/*"  },
                { "Authorization", $"Basic {token}" },
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
