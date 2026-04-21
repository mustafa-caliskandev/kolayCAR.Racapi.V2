using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IAgencyVendorService
    {
        Task<IEnumerable<Agencyvendorprofitmarkup>> GetAgencyVendorProfitmarkupListAsync();
    }
    public class AgencyVendorService : IAgencyVendorService
    {
        private readonly ICacheService _cacheService;
        private readonly IAgencyVendorProfitMarkupRepository _agencyVendorProfitMarkupRepository;
        public AgencyVendorService(ICacheService cacheService, IAgencyVendorProfitMarkupRepository agencyVendorProfitMarkupRepository)
        {
            _cacheService = cacheService;
            _agencyVendorProfitMarkupRepository = agencyVendorProfitMarkupRepository;
        }
        public async Task<IEnumerable<Agencyvendorprofitmarkup>> GetAgencyVendorProfitmarkupListAsync()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.AgencyKey}-AgencyVendorProfitMarkupList", _agencyVendorProfitMarkupRepository.GetAllAsync);

            return await _agencyVendorProfitMarkupRepository.GetAllAsync();
        }
    }
}
