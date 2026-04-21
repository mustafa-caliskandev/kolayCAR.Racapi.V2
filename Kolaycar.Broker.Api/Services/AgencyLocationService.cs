using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IAgencyLocationService : IService<Agencylocation>
    {
        Task<List<int>> GetAgencyLocationsByAgencyId(long agencyId);
    }
    public class AgencyLocationService : IAgencyLocationService
    {
        private readonly IAgencyLocationRepository _agencyLocationRepository;
        private readonly ICacheService _cacheService;
        public AgencyLocationService(IAgencyLocationRepository agencyLocationRepository, ICacheService cacheService)
        {
            _agencyLocationRepository = agencyLocationRepository;
            _cacheService = cacheService;
        }
        public async Task<List<int>> GetAgencyLocationsByAgencyId(long agencyId)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.LocationKey}-AgencyLocations-{agencyId}", () => _agencyLocationRepository.GetAgencyLocationsId(agencyId));

            return await _agencyLocationRepository.GetAgencyLocationsId(agencyId);
        }
    }
}
