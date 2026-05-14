using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IVendorVendorService
    {
        Task<IEnumerable<VendorVendor>> GetVendorVendorList();
        Task<IEnumerable<VendorVendor>> GetVendorVendorListByVendorId(int vendorId);
        Task AddRangeAsync(IEnumerable<VendorVendor> vendorVendors, int vendorId);
    }
    public class VendorVendorService : IVendorVendorService
    {
        private readonly IVendorVendorRepository _vendorVendorRepository;
        private readonly ICacheService _cacheService;
        private static readonly string VendorVendorListCacheKey = $"{CacheSettings.VendorKey} - VendorVendorList";
        public VendorVendorService(IVendorVendorRepository vendorVendorRepository, ICacheService cacheService)
        {
            _vendorVendorRepository = vendorVendorRepository;
            _cacheService = cacheService;
        }

        public async Task AddRangeAsync(IEnumerable<VendorVendor> vendorVendors, int vendorId)
        {
            var incomingVendorVendors = vendorVendors?
                .Where(e => !string.IsNullOrWhiteSpace(e.VendorName))
                .ToList();

            if (incomingVendorVendors == null || !incomingVendorVendors.Any())
                return;

            foreach (var vendorVendor in incomingVendorVendors)
                vendorVendor.VendorName = vendorVendor.VendorName.Trim();

            var vendorVendorList = await _vendorVendorRepository.GetVendorVendorListByVendorIdAsync(vendorId) ?? Enumerable.Empty<VendorVendor>();
            var existingVendorNames = new HashSet<string>(
                vendorVendorList
                    .Where(e => !string.IsNullOrWhiteSpace(e.VendorName))
                    .Select(e => e.VendorName.Trim()),
                System.StringComparer.OrdinalIgnoreCase);

            var newVendorVendors = incomingVendorVendors
                .Where(e => existingVendorNames.Add(e.VendorName))
                .ToList();

            if (!newVendorVendors.Any())
                return;

            await _vendorVendorRepository.AddRangeAsync(newVendorVendors);

            if (CacheSettings.UseCache)
            {
                await _cacheService.RemoveAsync(VendorVendorListCacheKey);
                await _cacheService.InvalidateCacheAsync(CacheTypes.Vendor);
            }
        }

        public async Task<IEnumerable<VendorVendor>> GetVendorVendorList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync(VendorVendorListCacheKey, _vendorVendorRepository.GetAllAsync);

            return await _vendorVendorRepository.GetAllAsync();
        }

        public async Task<IEnumerable<VendorVendor>> GetVendorVendorListByVendorId(int vendorId)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{VendorVendorListCacheKey}-{vendorId}", () => _vendorVendorRepository.GetVendorVendorListByVendorIdAsync(vendorId));

            return await _vendorVendorRepository.GetVendorVendorListByVendorIdAsync(vendorId);
        }
    }
}
