using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ILocationVendorService
    {
        Task<Locationvendor> GetLocationVendor(int pickupLocationId, int vendorId);
        Task<IEnumerable<Locationvendor>> GetLocationVendorByVendorId(int vendorId);
        Task AddRangeLocationVendor(IEnumerable<Locationvendor> locationvendors);
    }
    public class LocationVendorService : ILocationVendorService
    {
        private readonly ICacheService _cacheService;
        private readonly ILocationVendorRepository _locationVendorRepository;
        public LocationVendorService(ICacheService cacheService, ILocationVendorRepository locationVendorRepository)
        {
            _cacheService = cacheService;
            _locationVendorRepository = locationVendorRepository;
        }

        public async Task AddRangeLocationVendor(IEnumerable<Locationvendor> locationvendors) => await _locationVendorRepository.AddRangeAsync(locationvendors);
        public async Task<Locationvendor> GetLocationVendor(int pickupLocationId, int vendorId)
        {
            if (CacheSettings.UseCache)
            {
                var locationVendors = await _cacheService.GetOrCreateAsync($"{CacheSettings.LocationKey}-LocationVendor-{vendorId}", () => _locationVendorRepository.GetLocationVendorByVendorId(vendorId));
                return locationVendors.Where(e => e.Locallocationid == pickupLocationId && e.Active == true).FirstOrDefault();
            }
            else
                return await _locationVendorRepository.GetLocationVendor(pickupLocationId, vendorId);
        }

        public async Task<IEnumerable<Locationvendor>> GetLocationVendorByVendorId(int vendorId) => await _locationVendorRepository.GetLocationVendorByVendorId(vendorId);
    }
}
