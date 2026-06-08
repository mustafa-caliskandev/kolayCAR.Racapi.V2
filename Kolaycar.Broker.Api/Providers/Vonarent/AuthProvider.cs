using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Vonarent
{
    public class AuthProvider
    {
        private readonly HttpManager _httpManager;
        private const string AuthenticatePath = "/api/remote/v1/auth/authenticate";

        public AuthProvider(string apiBaseUrl, int timeout = 0)
        {
            _httpManager = new HttpManager(apiBaseUrl, timeout: timeout);
        }

        public async Task<string> GetTokenAsync(Vendor vendor)
        {
            var authResponse = await AuthenticateAsync(vendor);

            if (authResponse?.status == 1 && !string.IsNullOrWhiteSpace(authResponse.token))
                return authResponse.token;

            return null;
        }

        public async Task<VonarentAuthResponse> AuthenticateAsync(Vendor vendor)
        {
            var headers = CreateAuthenticationHeaders(vendor);

            var result = await _httpManager.GetAsync2<VonarentAuthResponse>(
                requestPath: AuthenticatePath,
                headers: headers,
                isReservationRequest: true);

            return result?.Data;
        }

        public async Task<IDictionary<string, object>> GetAuthorizedHeadersAsync(
            Vendor vendor,
            string method,
            string path,
            IDictionary<string, object> query = null,
            object body = null)
        {
            var token = await GetTokenAsync(vendor);

            if (string.IsNullOrWhiteSpace(token))
                return new Dictionary<string, object>();

            return CreateAuthorizedHeaders(vendor, token, method, path, query, body);
        }

        public IDictionary<string, object> CreateAuthorizedHeaders(
            Vendor vendor,
            string bearerToken,
            string method,
            string path,
            IDictionary<string, object> query = null,
            object body = null)
            => SignatureHelper.CreateHeaders(vendor, method, path, query, body, bearerToken);

        public IDictionary<string, object> CreateAuthenticationHeaders(Vendor vendor)
            => SignatureHelper.CreateHeaders(vendor, HttpMethod.Get.Method, AuthenticatePath);
    }
}
