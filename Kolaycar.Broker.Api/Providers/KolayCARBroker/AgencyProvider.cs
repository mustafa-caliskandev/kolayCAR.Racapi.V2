using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Providers.KolayCARBroker
{
    public class AgencyProvider : IAgencyProvider
    {
        HttpManager HttpManager { get; set; }
        AuthProvider AuthProvider { get; set; }

        public AgencyProvider(string apiBaseUrl)
        {
            HttpManager = new HttpManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetAgency(GetAgenciesRequest getAgenciesRequest, CommonModels.Vendor vendor, CommonModels.ReservationToken reservationToken, CommonModels.Agency agency)
        {
            if (vendor.UseBrokerConfigurations)
            {
                var auth = await AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword));

                if (auth != null)
                {
                    var user = auth.Data as CommonModels.User;

                    var result = HttpManager.Get<CommonModels.Agency>(
                        requestPath: "agencies",
                        parameters: CreateBrokerGetAgenciesRequestParameters(getAgenciesRequest, reservationToken),
                        headers: AuthProvider.CreateAuthHeader(user.Token));

                    if (result.Success)
                        return new ServiceResponseBase
                        {
                            Success = result.Success,
                            Message = result.Message,
                            ServiceMessage = result.Message,
                            Data = result.Data as CommonModels.Agency
                        };

                    return new ServiceResponseBase
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ServiceMessage = result.Message
                    };
                }

                return new ServiceResponseBase
                {
                    Success = auth.Success,
                    Message = "Kimlik doğrulama işlemi başarısız!",
                    ServiceMessage = auth.Message
                };
            }
            else
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = agency
                };
            }
        }

        private Dictionary<string, object> CreateBrokerGetAgenciesRequestParameters(GetAgenciesRequest getAgenciesRequest, CommonModels.ReservationToken reservationToken)
        {
            return new Dictionary<string, object>()
            {
                { "languageCode",  getAgenciesRequest.LanguageCode},
                { "reservationToken",  reservationToken.APIReferenceCode},
                { "agencyId",  getAgenciesRequest.AgencyId}
            };
        }
    }
}
