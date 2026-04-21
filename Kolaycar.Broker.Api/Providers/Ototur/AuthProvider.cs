using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.OtoturRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.OtoturResponseBase;

namespace KolayCAR.Broker.API.Providers.Ototur
{
    public class AuthProvider
    {
        string _apiBaseUrl;
        private readonly RestManager RestManager;
        public AuthProvider(string apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl;
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<OtoturAuthResponse> GetToken()
        {
            var result = await RestManager.PostFormUrlEncoded<OtoturAuthResponse>(
                requestPath: "gettoken",
                headers: CreateHeaderWithContentType(),
                  postData: new[] {
                              new KeyValuePair<string,string>("grant_type","password"),
                              new KeyValuePair<string,string>("username","obilet"),
                              new KeyValuePair<string,string>("password","ehLda75lU90AXLV" ) }

                );
            return result;
        }

        private OtoturAuthRequest GetEntity()
        {
            return new OtoturAuthRequest { username = "obilet", password = "ehLda75lU90AXLV" };
        }

        public Dictionary<string, object> CreateHeaderWithContentType() =>
        new Dictionary<string, object>()
        {
                { "Content-Type", "application/x-www-form-urlencoded"}
        };
    }
}
