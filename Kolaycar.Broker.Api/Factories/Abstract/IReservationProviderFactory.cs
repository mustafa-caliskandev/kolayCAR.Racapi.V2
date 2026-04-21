using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace KolayCAR.Broker.API.Factories.Abstract
{
    public interface IReservationProviderFactory
    {
        IReservationProvider? CreateReservationProvider(Vendor vendor, IConfigurationService configurationService, IConfiguration configuration, IMemoryCache memoryCache, ICacheService cacheService);
    }
}
