using KolayCAR.Broker.API.Helpers.Garenta;
using KolayCAR.Broker.API.Mappers.Garenta;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Garenta
{
    public class LocationProvider : ILocationProvider
    {
        RestManager RestManager { get; set; }
        HttpManager HttpManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public string ProviderName => "Garenta";
        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            HttpManager = new HttpManager(apiBaseUrl);
            AuthProvider = new AuthProvider();
        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var result = await RestManager.PostAsync<GarentaRequestBase, GarentaResponseBase>(
                     requestPath: string.Empty,
                     entity: GetLocationsRequestBody(vendor, languageId),
                     headers: AuthProvider.CreateAuthHeaderWithContentType(vendor)
                     );

            if ((bool)result?.EXPORT.ES_OUTPUT?.LOCATION?.Any())
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.EXPORT.ES_OUTPUT.LOCATION.Map().OrderBy(x => x.LocationName).ToList()
                };
            }

            return new ServiceResponseBase();
        }

        private GarentaRequestBase GetLocationsRequestBody(Vendor vendor, int languageId) =>
            new GarentaRequestBase
            {
                sap_props = RequestHelper.GetSapProps(GarentaRequestBase.ServiceTypes.GET_LOCATIONS, vendor.ApiKey, vendor.ApiPassword),
                import = new GarentaRequestBase.Import
                {
                    IS_INPUT = new GarentaRequestBase.ISINPUT_BASE
                    {
                        BROKER_CODE = vendor.ApiKey,
                        LANGU = RequestHelper.GetLanguageType((LanguageTypes)(languageId - 1))
                    }
                }
            };
    }
}
