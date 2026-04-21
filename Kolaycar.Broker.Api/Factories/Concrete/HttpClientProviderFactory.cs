using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace KolayCAR.Broker.API.Factories.Concrete
{
    public class HttpClientProviderFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ConcurrentDictionary<string, HttpClient> _clients = new();

        public HttpClientProviderFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public HttpClient GetClient(string baseUrl)
        {
            return _clients.GetOrAdd(baseUrl, url =>
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(url);
                client.Timeout = TimeSpan.FromSeconds(30);
                return client;
            });
        }
    }
}
