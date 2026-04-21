using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface ICityService : IService<City>
    {
        Task<City> GetCityById(int id);
    }
    public class CityService : ICityService
    {
        private readonly ICityRepository _cityRepository;
        private readonly ICacheService _cacheService;
        public CityService(ICityRepository cityRepository, ICacheService cacheService)
        {
            _cityRepository = cityRepository;
            _cacheService = cacheService;
        }
        public async Task<City> GetCityById(int id)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"City-{id}", () => _cityRepository.GetCityByIdAsync(id));

            return await _cityRepository.GetCityByIdAsync(id);
        }
    }
}
