using KolayCAR.Broker.API.Mappers.Europcar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.Europcar
{
    public class LocationProvider : ILocationProvider
    {
        public string ProviderName => "Europcar";
        public LocationProvider(string apiBaseUrl)
        {
            //HttpManager = new HttpManager(apiBaseUrl);
        }

        //public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId)
        //{
        //    var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
        //    requestPath: $"{vendor.ApiKey.Split('|')[0]}/link/v3/stations_{vendor.ApiPassword.Split('-')[0]}.html",
        //    parameters: GetLocationsRequestParameters(vendor));

        //    if (result != null && result.response != null && result.response.station != null && result.response.station.Count > 0)
        //    {
        //        return new ServiceResponseBase
        //        {
        //            Success = result.response.station.Count > 0,
        //            Data = result.response.station.Map()
        //        };
        //    }

        //    return new ServiceResponseBase
        //    {
        //        Success = false,
        //        Data = null
        //    };
        //}

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var _httpManager = new HttpManager(vendor.SecretKey);
            var result = await _httpManager.GetAsyncWithModel<EuropcarLocationResponse>($"/broker/station/account/{vendor.ApiClientId}?limit=all");
            //var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>("");

            //var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
            //requestPath: $"{vendor.ApiKey.Split('|')[0]}/link/v3/stations_{vendor.ApiPassword.Split('-')[0]}.html",
            //parameters: GetLocationsRequestParameters(vendor));
            if (result?.data?.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result?.data?.Count > 0,
                    Data = result.data.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }

        private Dictionary<string, object> GetLocationsRequestParameters(CommonModels.Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "AGENT",  vendor.ApiPassword.Split('-')[1]},
                { "CDP",  !string.IsNullOrEmpty(vendor.ApiClientId) ? vendor.ApiClientId : ""}
            };
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
