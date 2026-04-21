using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services.Abstract;
using Microsoft.Extensions.Caching.Memory;

namespace KolayCAR.Broker.API.Factories.Abstract
{
    public interface ILocationProviderFactory
    {
        ILocationProvider? CreateLocationProvider(Domain.Models.Vendor mappedVendor, API.Models.Vendor vendor, IMemoryCache memoryCache, ICacheService cacheService);
    }
}
