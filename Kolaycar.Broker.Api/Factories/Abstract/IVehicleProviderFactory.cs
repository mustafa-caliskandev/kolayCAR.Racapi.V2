using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services.Abstract;
using Microsoft.Extensions.Caching.Memory;

namespace KolayCAR.Broker.API.Factories.Abstract
{
    public interface IVehicleProviderFactory
    {
        IVehicleProvider? CreateVehicleProvider(Domain.Models.Vendor vendor, IMemoryCache memoryCache, ICacheService cacheService, bool disableTimeout);
    }
}
