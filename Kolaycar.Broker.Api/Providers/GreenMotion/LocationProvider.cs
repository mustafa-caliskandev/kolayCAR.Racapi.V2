using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.GreenMotion;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;
using GreenMotionHelper = KolayCAR.Broker.API.Helpers.GreenMotion;

namespace KolayCAR.Broker.API.Providers.GreenMotion
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        public string ProviderName => "GreenMotion";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetLocations(CommonModels.Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.PostAsyncXMLRestClient<GreenMotionResponseBase>(
            requestPath: string.Empty,
            parameterType: RestSharp.ParameterType.RequestBody,
            headers: GreenMotionHelper.RequestHelper.GetGreenMotionRequestHeader(),
            entity: CreateGetLocationsRequestBodyEntity(vendor, languageId));

            if (result != null && result.gm_webservice != null && result.gm_webservice.response != null && result.gm_webservice.response.servicearea != null && result.gm_webservice.response.servicearea.Count > 0)
                return new ServiceResponseBase
                {
                    Success = result.gm_webservice.response.servicearea.Count > 0,
                    Data = result.gm_webservice.response.servicearea.Map()
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Green Motion lokasyonları gelmiyor!"
            };
        }

        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            var result = await RestManager.PostAsyncXMLRestClient<GreenMotionResponseBase>(
           requestPath: string.Empty,
           parameterType: RestSharp.ParameterType.RequestBody,
           headers: GreenMotionHelper.RequestHelper.GetGreenMotionRequestHeader(),
           entity: CreateGetLocationDetailRequestBodyEntity(vendor, locationCode));

            if (result != null && result.gm_webservice != null && result.gm_webservice.response != null && result.gm_webservice.response.location_info != null)
                return new ServiceResponseBase
                {
                    Success = result.gm_webservice.response.location_info != null,
                    Data = result.gm_webservice.response.location_info.Map(locationCode)
                };

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Green Motion lokasyonları gelmiyor!"
            };
        }

        public Dictionary<string, object> CreateGetLocationsRequestBodyEntity(CommonModels.Vendor vendor, int languageId) =>
            new Dictionary<string, object>()
            {
                { "application/xml", GetLocationsRequestParameters(vendor, languageId) },
            };

        private string GetLocationsRequestParameters(CommonModels.Vendor vendor, int languageId)
        {
            var languageType = (LanguageTypes)(languageId - 1);

            var getLocationPayload = new GreenMotionRequestBase.GreenMotionGetLocationsRequest
            {
                country_id = vendor.SecretKey.ToIntNullSafe(),
                language = languageType.ToString(),
                type = "GetServiceAreas"
            };

            var body = GreenMotionHelper.RequestHelper.GetGreenMotionRequestObject(vendor, getLocationPayload);

            return ObjectHelper.ObjectToXML(body);
        }

        public Dictionary<string, object> CreateGetLocationDetailRequestBodyEntity(CommonModels.Vendor vendor, string apiLocationCode) =>
            new Dictionary<string, object>()
            {
                { "application/xml", GetLocationDetailRequestParameters(vendor, apiLocationCode) },
            };

        private string GetLocationDetailRequestParameters(CommonModels.Vendor vendor, string apiLocationCode)
        {
            var getLocationPayload = new GreenMotionRequestBase.GreenMotionGetLocationDetailRequest
            {
                location_id = apiLocationCode.ToIntNullSafe(),
                type = "GetLocationInfo"
            };

            var body = GreenMotionHelper.RequestHelper.GetGreenMotionRequestObject(vendor, getLocationPayload);
            var bodyXML = ObjectHelper.ObjectToXML(body);

            return bodyXML;
        }


    }
}
