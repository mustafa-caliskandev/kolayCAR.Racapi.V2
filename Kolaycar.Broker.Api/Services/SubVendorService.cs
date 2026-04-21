using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ISubVendorService
    {
        Task<IEnumerable<Subvendor>> GetAllAsync();
    }
    public class SubVendorService : ISubVendorService
    {
        private readonly ISubVendorRepository _subVendorRepository;
        private readonly ICacheService _cacheService;
        public SubVendorService(ISubVendorRepository subVendorRepository, ICacheService cacheService)
        {
            _subVendorRepository = subVendorRepository;
            _cacheService = cacheService;
        }
        public async Task<IEnumerable<Subvendor>> GetAllAsync()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.VendorKey}-SubVendorList", _subVendorRepository.GetAllAsync);

            return await _subVendorRepository.GetAllAsync();
        }
    }
}
