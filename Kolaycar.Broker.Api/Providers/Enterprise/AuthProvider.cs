using KolayCAR.Broker.Infrastructure.Managers;

namespace KolayCAR.Broker.API.Providers.Enterprise
{
    public class AuthProvider
    {
        HttpManager _httpManager { get; set; }

        public AuthProvider(string apiBaseUrl)
        {
            _httpManager = new HttpManager(apiBaseUrl);
        }
    }
}
