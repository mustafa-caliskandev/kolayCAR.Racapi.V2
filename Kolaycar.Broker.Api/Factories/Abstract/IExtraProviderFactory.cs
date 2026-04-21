using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services.Abstract;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace KolayCAR.Broker.API.Factories.Abstract
{
    public interface IExtraProviderFactory
    {
        IExtraProvider? CreateExtraProvider(Domain.Models.Vendor vendor, IMemoryCache memoryCache, ICacheService cacheService, IConfiguration configuration);
    }
}
