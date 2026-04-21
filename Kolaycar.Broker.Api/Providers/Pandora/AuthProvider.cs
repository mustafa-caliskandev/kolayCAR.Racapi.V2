using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using PandoraHelpers = KolayCAR.Broker.API.Helpers.Pandora;

namespace KolayCAR.Broker.API.Providers.Pandora
{
    public class AuthProvider
    {
        RestManager RestManager { get; set; }
        public AuthProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }

        public async Task<PandoraResponseBase.Token> GetAccessToken(Vendor vendor)
        {
            string salt = ReservationHelper.GenerateReservationId().ToString();
            return await RestManager.PostAsyncRestClient<PandoraResponseBase.Token>(
                           requestPath: "tr/token",
                           headers: CreatePostFormHeader(),
                           entity: CreateAuthForm(vendor, salt));
        }

        //public PandoraResponseBase.Auth CreateAuthForm(Vendor vendor, string salt) =>
        //    new PandoraResponseBase.Auth
        //    {
        //        grant_type = "password",
        //        username = vendor.ApiKey,
        //        password = vendor.ApiPassword,
        //        client_id = vendor.ApiClientId,
        //        signature = PandoraHelpers.AuthHelper.GenerateSignature(vendor.ApiKey, vendor.ApiPassword, vendor.ApiClientId, salt, vendor.SecretKey),
        //        salt = salt
        //    };

        public Dictionary<string, object> CreateAuthForm(Vendor vendor, string salt) =>
            new Dictionary<string, object>()
            {
                { "grant_type", "password" },
                { "username", vendor.ApiKey },
                { "password", vendor.ApiPassword },
                { "client_id", vendor.ApiClientId },
                { "signature", PandoraHelpers.AuthHelper.GenerateSignature(vendor.ApiKey, vendor.ApiPassword, vendor.ApiClientId, salt, vendor.SecretKey) },
                { "salt", salt }
            };

        public Dictionary<string, object> CreatePostFormHeader() =>
            new Dictionary<string, object>()
            {
                { "Content-Type", "application/x-www-form-urlencoded"}
            };

        public Dictionary<string, object> CreateAuthHeader(string accessToken) =>
            new Dictionary<string, object>()
            {
                { "Authorization", $"Bearer {accessToken}"}
            };

        public Dictionary<string, object> CreateAuthHeaderWithContentType(string accessToken) =>
            new Dictionary<string, object>()
            {
                { "Authorization", $"Bearer {accessToken}"},
                { "Content-Type", "application/json"}
            };
    }
}
